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
        private readonly ICommonServices _commonService;
        public ProjectDashboardService(IUnitOfWork unitOfWork, ICommonServices commonService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }

        public async Task<NearSubmissionDeadlineData> GetNearSubmissionDeadlines(Guid studentId)
        {
            var currentStudent = await _unitOfWork.StudentRepository.Get().FirstOrDefaultAsync(x => x.InformationId == studentId) ?? throw new DataNotFoundException();
            var currentSemester = await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork);
            var projectSubmissions = await _unitOfWork.ProjectSubmissionRepository.Get()
                                                      .Where(x => x.ProjectId == currentStudent.ProjectId && x.Name != null)
                                                      .Select(x => x.SubmissionModuleId).ToListAsync();
            var nearDeadlineSubmissions = _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.SemesterId == currentSemester.CurrentSemester!.Id).SelectMany(x => x.ClassModuleDeadlines)
                                                           .Where(x => x.EndDate > DateTime.Now && x.ClassId == _commonService.GetClass()!.Id && !projectSubmissions.Contains(x.SubmissionModuleId))
                                                           .OrderBy(x => x.EndDate)
                                                           .Select(x => new NearDealineSubmission
                                                           {
                                                               AssessmentId = x.SubmissionModule.AssessmentId.ToString()!,
                                                               EndDate = x.EndDate,
                                                               Name = x.SubmissionModule.Name,
                                                               SubmissionModuleId = x.Id.ToString()
                                                           });
            return new NearSubmissionDeadlineData
            {
                Submissions = await nearDeadlineSubmissions.ToListAsync()
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
            var projectSubmissions = await _commonService.GetProjectSubmissions(currentStudent.ProjectId!.Value);
            response.Submission.Done = projectSubmissions.Count();
            //Count Submission
            var currentSemesterInfo = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork));
            response.Submission.Total = await _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.LectureId == currentStudent.Class.LecturerId &&
                                                                    x.SemesterId == currentSemesterInfo.CurrentSemester!.Id)
                                                                    .CountAsync();

            var submissions = projectSubmissions.GroupBy(x => x.SubmissionModule.AssessmentId).ToDictionary(x => x.Key);
            //var mapAssessmentDetailTasks = new List<Task<AssessmentDetail>>();
            foreach (var assessment in currentSemesterInfo.CurrentSemester!.Syllabus!.Assessments.OrderBy(x => x.Order))
            {
                var submissionsOfAssessment = new List<ProjectSubmission>();
                var isHaveSubmission = submissions.TryGetValue(assessment.Id, out var assessmentSubmissions);
                if (isHaveSubmission)
                {
                    submissionsOfAssessment = assessmentSubmissions!.ToList();
                }
                response.Assessments.Add(await MapAssessmentDetail(submissionsOfAssessment, assessment, currentSemesterInfo.CurrentSemester));
                //mapAssessmentDetailTasks.Add(MapAssessmentDetail(submissionsOfAssessment, assessment, ));
            }
            //response.Assessments.AddRange(await Task.WhenAll(mapAssessmentDetailTasks));
            return response;
        }
        private async Task<AssessmentDetail> MapAssessmentDetail(List<ProjectSubmission> submissionsOfAssessment, Assessment assessment, Semester semester)
        {
            return new AssessmentDetail
            {
                AssessmentId = assessment.Id,
                AssessmentName = assessment.Name,
                AssessmentStatus = await _commonService.GetAssessmentStatus(assessment.Id, submissionsOfAssessment, semester)
            };
        }
    }
}
