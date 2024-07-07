using IPMS.Business.Common.Extensions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ProjectSubmissionService : IProjectSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IPresignedUrlService _presignedUrl;
        private readonly IHttpContextAccessor _context;

        public ProjectSubmissionService(IUnitOfWork unitOfWork, ICommonServices commonServices, IPresignedUrlService presignedUrl, IHttpContextAccessor context) 
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _presignedUrl = presignedUrl;
            _context  = context;
        }

        public async Task<IQueryable<GetAllSubmissionResponse>> GetAllSubmission(GetAllSubmissionRequest request, Guid currentUserId)
        {
            if (request.SearchValue == null)
            {
                request.SearchValue = "";
            }
            request.SearchValue = request.SearchValue.Trim().ToLower();
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");


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
                AssesmentName =  x.SubmissionModule.Assessment!.Name,
                SubmitDate = x.SubmissionDate,
                SubmitterName = x.Submitter!.FullName,
                SubmitterId = x.SubmitterId,
                Grade = x.FinalGrade,
                Link = _presignedUrl.GeneratePresignedDownloadUrl(x.Name),
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
            var submissionModule = _unitOfWork.SubmissionModuleRepository.Get().Include(x=>x.ClassModuleDeadlines.Where(y=> y.ClassId == _commonServices.GetClass()!.Id)).FirstOrDefault(sm => sm.Id.Equals(request.SubmissionModuleId)); // Find submission module
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
    }
}
