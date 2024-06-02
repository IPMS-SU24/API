﻿using IPMS.Business.Common.Enums;
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
            //Get all modules of semester
            var modules = currentSemester?.CurrentSemester?.Syllabus?.Assessments
                                                           .SelectMany(x => x.Modules).Where(x => x.EndDate > DateTime.Now).ToList();
            var projectSubmissions = await _unitOfWork.ProjectSubmissionRepository.Get()
                                                      .Where(x => x.ProjectId == currentStudent.ProjectId && x.Name != null)
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
            var projectSubmissions = await _commonService.GetProjectSubmissions(currentStudent.ProjectId!.Value);
            response.Submission.Done = projectSubmissions.Count();
            //Count Submission
            var currentSemesterInfo = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork));
            response.Submission.Total = await _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.LectureId == currentStudent.Class.LecturerId &&
                                                                    x.SemesterId == currentSemesterInfo.CurrentSemester!.Id)
                                                                    .CountAsync();

            var submissions = projectSubmissions.GroupBy(x => x.SubmissionModule.AssessmentId).ToDictionary(x => x.Key);

            foreach (var assessment in currentSemesterInfo.CurrentSemester!.Syllabus!.Assessments)
            {
                var detail = new AssessmentDetail
                {
                    AssessmentId = assessment.Id,
                    AssessmentName = assessment.Name
                };
                var submissionsOfAssessment = new List<ProjectSubmission>();
                var isHaveSubmission = submissions.TryGetValue(assessment.Id, out var assessmentSubmissions);
                if (isHaveSubmission)
                {
                    submissionsOfAssessment = assessmentSubmissions!.ToList();
                }
                detail.AssessmentStatus = await _commonService.GetAssessmentStatus(assessment.Id, submissionsOfAssessment);
                response.Assessments.Add(detail);
            }
            return response;
        }
    }
}
