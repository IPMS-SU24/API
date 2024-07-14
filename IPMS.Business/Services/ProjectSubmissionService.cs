﻿using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg;

namespace IPMS.Business.Services
{
    public class ProjectSubmissionService : IProjectSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IPresignedUrlService _presignedUrl;
        private IPMSClass @class;

        public ProjectSubmissionService(IUnitOfWork unitOfWork, ICommonServices commonServices, IPresignedUrlService presignedUrl, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _presignedUrl = presignedUrl;
            
        }

        public async Task<IQueryable<GetAllSubmissionResponse>> GetAllSubmission(GetAllSubmissionRequest request, Guid currentUserId)
        {
            if (request.SearchValue == null)
            {
                request.SearchValue = "";
            }
            request.SearchValue = request.SearchValue.Trim().ToLower();
            Project project = _commonServices.GetProject() ?? throw new DataNotFoundException();
            IQueryable<ProjectSubmission> projectSubmissions = _unitOfWork.ProjectSubmissionRepository
                                                  .Get().Where(x => x.ProjectId == project.Id
                                                            && (x.SubmissionModule!.Name.ToLower().Contains(request.SearchValue)
                                                                || x.SubmissionModule.Assessment!.Name.ToLower().Contains(request.SearchValue)))
                                                  .Include(x => x.SubmissionModule).ThenInclude(x => x!.Assessment)
                                                  .Include(x => x.Submitter);

            if (request.SubmitterId != null) // Query with submitter
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmitterId.Equals(request.SubmitterId));
            }

            if (request.AssessmentId != null) // Query with assessment
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmissionModule!.AssessmentId.Equals(request.AssessmentId)).AsQueryable();
            }

            if (request.StartDate != null)  // Query with startDate
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmissionDate >= request.StartDate);
            }

            if (request.EndDate != null) // Query with endDate
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmissionDate <= request.EndDate);
            }

            var groupNewest = _unitOfWork.ProjectSubmissionRepository // IsNewest base on all of submission in submission module
                                                  .Get().Where(x => x.ProjectId == project.Id)
                                                  .GroupBy(x => x.SubmissionModuleId)
                                                    .Select(group => new
                                                    {
                                                        moudleId = group.Key,
                                                        NewestSubmissionnId = group.OrderByDescending(x => x.SubmissionDate).FirstOrDefault()!.Id
                                                    }).ToList();

            IQueryable<GetAllSubmissionResponse> response = projectSubmissions.Select(x => new GetAllSubmissionResponse
            {
                ModuleName = x.SubmissionModule!.Name,
                AssesmentName = x.SubmissionModule.Assessment!.Name,
                SubmitDate = x.SubmissionDate,
                SubmitterName = x.Submitter!.FullName,
                SubmitterId = x.SubmitterId,
                Grade = x.FinalGrade,
                Link = _presignedUrl.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, x.Id, x.Name)) ?? string.Empty,
                FileName = x.Name,
                IsNewest = groupNewest.Select(gn => gn.NewestSubmissionnId).Contains(x.Id),
                AssessmentId = x.SubmissionModule.AssessmentId,
                ModuleId = x.SubmissionModuleId,
            });


            return response;
        }
        public async Task<ValidationResultModel> UpdateProjectSubmissionValidators(UpdateProjectSubmissionRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            var submissionModule = _unitOfWork.SubmissionModuleRepository.Get().Include(x => x.ClassModuleDeadlines.Where(y => y.ClassId == _commonServices.GetClass()!.Id)).FirstOrDefault(sm => sm.Id.Equals(request.SubmissionModuleId)); // Find submission module
            if (submissionModule == null)
            {
                result.Message = "Submission module does not exist";
                return result;
            }

            if (submissionModule.ClassModuleDeadlines.First().EndDate < request.SubmissionDate) // Validation submit time
            {
                result.Message = "Cannot submit at this time";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task<bool> UpdateProjectSubmission(UpdateProjectSubmissionRequest request, Guid currentUserId)
        {

            ProjectSubmission? submission = await _unitOfWork.ProjectSubmissionRepository.Get().FirstOrDefaultAsync(ps => ps.Id.Equals(request.Id));

            if (submission != null) // It's mean that have submission before, so that update
            {
                submission.Name = request.Name;
                submission.SubmitterId = currentUserId;
                submission.SubmissionDate = request.SubmissionDate;

                _unitOfWork.ProjectSubmissionRepository.Update(submission); // Update
                _unitOfWork.SaveChanges(); // Save changes
                return true;
            }
            else // haven't submitted yet
            {
                var currentProject = await _commonServices.GetProject(currentUserId); // find current project

                submission = new ProjectSubmission
                {
                    Id = request.Id,
                    Name = request.Name,
                    SubmissionDate = request.SubmissionDate,
                    ProjectId = currentProject!.Id,
                    SubmissionModuleId = request.SubmissionModuleId,
                    SubmitterId = currentUserId
                };

                await _unitOfWork.ProjectSubmissionRepository.InsertAsync(submission); // Insert
                _unitOfWork.SaveChanges(); // Save changes
                return true;

            }

        }

        public async Task<ValidationResultModel> GradeSubmissionValidators(GradeSubmissionRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            if (request.Grade < 0 || request.Grade > 10)
            {
                result.Message = "Cannot graded lower than 0 and greater than 10";
                return result;
            }

            var x = _unitOfWork.ProjectSubmissionRepository.Get().OrderByDescending(ps => ps.SubmissionDate).Select(ps => ps.Id).ToList();
            var submission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => ps.Id.Equals(request.SubmissionId)).FirstOrDefaultAsync();

            if (submission == null)
            {
                result.Message = "Project submission cannot found";
                return result;
            }

            @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.LecturerId.Equals(lecturerId)
                                        && c.Students.Any(s => s.ProjectId.Equals(submission.ProjectId)))
                                .Include(c => c.ClassModuleDeadlines.Where(cm => cm.SubmissionModuleId.Equals(submission.SubmissionModuleId))) // can not set FirstOrDefault
                                .Include(c => c.Committees.Where(c => c.LecturerId.Equals(lecturerId)))
                                .FirstOrDefaultAsync();

            if (@class == null)
            {
                result.Message = "Class cannot found";
                return result;
            }

            if (@class.ClassModuleDeadlines.Count() != 1)
            {
                result.Message = "Please set deadline for submission";
                return result;
            }
            if (@class.Committees.Count() != 1)
            {
                result.Message = "Lecturer does not have permission to grade this class";
                return result;
            }
            var moduleDeadline = @class.ClassModuleDeadlines.FirstOrDefault();
            var now = DateTime.Now;

            if (moduleDeadline.EndDate >= now)
            {
                result.Message = "The submission deadline has not yet passed";
                return result;
            }

            var semester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            if (semester == null)
            {
                result.Message = "Is not in Semester now";
                return result;
            }

            if (semester.StartDate > now || semester.EndDate < now)
            {

                result.Message = "Is not in Semester now";
                return result;
            }

            if (moduleDeadline.EndDate < submission.SubmissionDate)
            {
                result.Message = "Cannot graded for expired submission";
                return result;
            }

            var lastSubmission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => ps.SubmissionDate <= moduleDeadline.EndDate).OrderByDescending(ps => ps.SubmissionDate).FirstOrDefaultAsync();

            if (lastSubmission.Id.Equals(submission.Id) == false)
            {
                result.Message = "Cannot grade different last submission";
                return result;
            }


            if (@class.SemesterId.Equals(semester.Id) == false)
            {
                result.Message = "Class is not in Semester";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task GradeSubmission(GradeSubmissionRequest request, Guid lecturerId)
        {
            var submission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => ps.Id.Equals(request.SubmissionId)).FirstOrDefaultAsync();

            @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.LecturerId.Equals(lecturerId)
                                       && c.Students.Any(s => s.ProjectId.Equals(submission.ProjectId)))
                               .Include(c => c.ClassModuleDeadlines.Where(cm => cm.SubmissionModuleId.Equals(submission.SubmissionModuleId))) // can not set FirstOrDefault
                               .Include(c => c.Committees.Where(c => c.LecturerId.Equals(lecturerId)))
                               .FirstOrDefaultAsync();
            var grade = await _unitOfWork.LecturerGradeRepository.Get().FirstOrDefaultAsync(lg => lg.CommitteeId.Equals(@class.Committees.FirstOrDefault().Id) && lg.SubmissionId.Equals(request.SubmissionId));
            if (grade == null)
            {
                grade = new LecturerGrade
                {
                    CommitteeId = @class.Committees.FirstOrDefault().Id,
                    SubmissionId = request.SubmissionId,
                    Grade = request.Grade

                };
                await _unitOfWork.LecturerGradeRepository.InsertAsync(grade);
            }
            else
            {
                grade.Grade = request.Grade;
                _unitOfWork.LecturerGradeRepository.Update(grade);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
