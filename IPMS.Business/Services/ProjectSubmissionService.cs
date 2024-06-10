using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ProjectSubmissionService : IProjectSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IPresignedUrlService _presignedUrl;

        public ProjectSubmissionService(IUnitOfWork unitOfWork, ICommonServices commonServices, IPresignedUrlService presignedUrl) 
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _presignedUrl = presignedUrl;
        }

        public async Task<IQueryable<GetAllSubmissionResponse>> GetAllSubmission(GetAllSubmissionRequest request, Guid currentUserId)
        {
            request.searchValue = request.searchValue.Trim().ToLower();
            Guid projectId = (await _commonServices.GetProject(currentUserId))!.Id;

            IQueryable<ProjectSubmission> projectSubmissions = _unitOfWork.ProjectSubmissionRepository
                                                  .Get().Where(x => x.ProjectId == projectId
                                                            && (x.SubmissionModule!.Name.ToLower().Contains(request.searchValue)
                                                                || x.SubmissionModule.Assessment!.Name.ToLower().Contains(request.searchValue)))
                                                  .Include(x => x.SubmissionModule).ThenInclude(x => x!.Assessment)
                                                  .Include(x => x.Submitter);

            if (request.submitterId != null) // Query with submitter
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmitterId.Equals(request.submitterId));
            }

            if (request.assessmentId != null) // Query with assessment
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmissionModule!.AssessmentId.Equals(request.assessmentId)).AsQueryable();
            }

            if (request.startDate != null)  // Query with startDate
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmissionDate >= request.startDate);
            }

            if (request.endDate != null) // Query with endDate
            {
                projectSubmissions = projectSubmissions.Where(x => x.SubmissionDate <= request.endDate);
            }

            var groupNewest = _unitOfWork.ProjectSubmissionRepository // IsNewest base on all of submission in submission module
                                                  .Get().Where(x => x.ProjectId == projectId)
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

        public async Task<bool> UpdateProjectSubmission(UpdateProjectSubmissionRequest request, Guid currentUserId)
        {
            try
            {
                var submissionModule = _unitOfWork.SubmissionModuleRepository.Get().FirstOrDefault(sm => sm.Id.Equals(request.SubmissionModuleId)); // Find submission module

                if (submissionModule.EndDate < request.SubmissionDate) // Validation submit time
                {
                    return false;
                }

                ProjectSubmission? submission = _unitOfWork.ProjectSubmissionRepository.Get().FirstOrDefault(ps => ps.Id.Equals(request.Id));

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
            catch (Exception ex)
            {
                return false;
            }


        }
    }
}
