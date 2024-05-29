using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Singleton;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses;
using IPMS.DataAccess.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;

namespace IPMS.Business.Services
{
    public class ProjectDashboardService : IProjectDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProjectDashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<GetProjectDetailData> GetProjectDetail(Guid studentId)
        {
            var currentStudent = await _unitOfWork.StudentRepository.Get().Where(x => x.InformationId == studentId && x.ProjectId != null)
                                                             .GetQueryActive().Include(x=>x.Class).FirstOrDefaultAsync();
            if (currentStudent == null || currentStudent.ProjectId == null)
            {
                 throw new DataNotFoundException();
            }
            //Init Data include Project info
            var response = new GetProjectDetailData
            {
                ProjectId = currentStudent.ProjectId!.Value,
                TopicName = await _unitOfWork.ClassTopicRepository.Get()
                                                                   .Where(x=>x.ProjectId == currentStudent.ProjectId!.Value && x.ClassId == currentStudent.ClassId)
                                                                   .Include(x=>x.Topic).Select(x=>x.Topic!.Name).FirstOrDefaultAsync() ?? string.Empty
            };

            //Get Project Submission
            var projectSubmissions = await _unitOfWork.ProjectSubmissionRepository
                                                    .Get().Where(x => x.ProjectId == currentStudent.ProjectId)
                                                    .GetQueryActive().Include(x=>x.SubmissionModule).ToListAsync();
            response.Submission.Done = projectSubmissions.Count;
            //Count Submission
            var currentSemesterInfo = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork));
            response.Submission.Total =  await _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.LectureId == currentStudent.Class.LecturerId &&
                                                                    x.SemesterId == currentSemesterInfo.CurrentSemester!.Id)
                                                                    .GetQueryActive().CountAsync();

            var submissions = projectSubmissions.GroupBy(x => x.SubmissionModule.AssessmentId).ToDictionary(x => x.Key);

            foreach (var assessment in currentSemesterInfo.CurrentSemester!.Syllabus!.Assessments)
            {
                var now = DateTime.Now;
                var detail = new AssessmentDetail
                {
                    AssessmentId = assessment.Id,
                    AssessmentName = assessment.Name
                };
                //Check Assessment Deadline, Start Date
                var deadline = assessment.Modules.MaxBy(x => x.EndDate)!.EndDate;
                var start = assessment.Modules.MinBy(x => x.StartDate)!.StartDate;
                //Case 1: Start Time in the future => status NotYet
                if(start > now)
                {
                    detail.Status = AssessmentStatus.NotYet.GetDisplayName();
                    response.Assessements.Add(detail);
                    continue;
                }
                //Case 2: Deadline in the future, Start time in the past => status InProgress
                if(start <= now && deadline > now)
                {
                    detail.Status = AssessmentStatus.InProgress.GetDisplayName();
                    response.Assessements.Add(detail);
                    continue;
                }
                //Case 3: Deadline in the past
                //Case 3.1: All module is submitted
                var isHaveSubmit = submissions.TryGetValue(assessment.Id, out var submit);
                if (isHaveSubmit && submit.Count() == assessment.Modules.Count)
                {
                    detail.Status = AssessmentStatus.Done.GetDisplayName();
                    response.Assessements.Add(detail);
                    continue;
                }
                //Case 3.2: At least one module have not submitted yet => status Expired
                detail.Status = AssessmentStatus.Expired.GetDisplayName();
                response.Assessements.Add(detail);
            }
            return response;
        }
    }
}
