using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Singleton;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.DataAccess.Common.Extensions;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
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

        public async Task<NearSubmissionDeadlineData> GetNearSubmissionDeadlines(Guid studentId)
        {
            var currentStudent = await _unitOfWork.StudentRepository.Get().FirstOrDefaultAsync(x => x.InformationId == studentId) ?? throw new DataNotFoundException();
            var currentSemester = await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork);
            //Get all modules of semester
            var modules = currentSemester?.CurrentSemester?.Syllabus?.Assessments
                                                           .SelectMany(x => x.Modules).Where(x => x.EndDate > DateTime.Now).ToList();
            var projectSubmissions = await _unitOfWork.ProjectSubmissionRepository.Get()
                                                      .Where(x => x.ProjectId == currentStudent.ProjectId && x.SubmissionLink != null)
                                                      .Select(x => x.SubmissionModuleId).ToListAsync();
            var nearDeadlineSubmissions = modules.SkipWhile(x => projectSubmissions.Contains(x.Id)).Select(x=>new NearDealineSubmission
            {
                AssessmentId = x.AssessmentId.ToString()!,
                EndDate = x.EndDate,
                Name = x.Name,
                SubmissionModuleId = x.Id.ToString()
            });
            return new NearSubmissionDeadlineData
            {
                Submissions = nearDeadlineSubmissions.ToList()
            };
        }

        public async Task<GetProjectDetailData> GetProjectDetail(Guid studentId)
        {
            var currentStudent = await _unitOfWork.StudentRepository.Get().Where(x => x.InformationId == studentId && x.ProjectId != null)
                                                             .Include(x => x.Class).FirstOrDefaultAsync();
            if (currentStudent == null || currentStudent.ProjectId == null)
            {
                throw new DataNotFoundException();
            }
            //Init Data include Project info
            var response = new GetProjectDetailData
            {
                ProjectId = currentStudent.ProjectId!.Value,
                TopicName = await _unitOfWork.ClassTopicRepository.Get()
                                                                   .Where(x => x.ProjectId == currentStudent.ProjectId!.Value && x.ClassId == currentStudent.ClassId)
                                                                   .Include(x => x.Topic).Select(x => x.Topic!.Name).FirstOrDefaultAsync() ?? string.Empty
            };

            //Get Project Submission
            var projectSubmissions = await _unitOfWork.ProjectSubmissionRepository
                                                    .Get().Where(x => x.ProjectId == currentStudent.ProjectId)
                                                    .Include(x => x.SubmissionModule).ToListAsync();
            response.Submission.Done = projectSubmissions.Count;
            //Count Submission
            var currentSemesterInfo = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork));
            response.Submission.Total = await _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.LectureId == currentStudent.Class.LecturerId &&
                                                                    x.SemesterId == currentSemesterInfo.CurrentSemester!.Id)
                                                                    .CountAsync();

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
                if (start > now)
                {
                    detail.Status = AssessmentStatus.NotYet.GetDisplayName();
                    response.Assessements.Add(detail);
                    continue;
                }
                //Case 2: Deadline in the future, Start time in the past => status InProgress
                if (start <= now && deadline > now)
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
