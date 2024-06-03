using AutoMapper;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Services
{
    public class ProjectSubmissionService : IProjectSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;

        public ProjectSubmissionService(IUnitOfWork unitOfWork, ICommonServices commonServices, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
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
                        ProjectId = currentProject.Id,
                        SubmissionModuleId = request.SubmissionModuleId,
                        SubmitterId = currentUserId
                    };

                    await _unitOfWork.ProjectSubmissionRepository.Insert(submission); // Insert
                    _unitOfWork.SaveChanges(); // Save changes
                    return true;

                }
            } catch (Exception ex)
            {
                return false;
            }
           
            
        }
    }
}
