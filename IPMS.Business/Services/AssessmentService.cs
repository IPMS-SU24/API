using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Assessment;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.Business.Responses.SubmissionModule;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Xml.Linq;

namespace IPMS.Business.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IPresignedUrlService _presignedUrlService;

        public AssessmentService(IUnitOfWork unitOfWork, ICommonServices commonServices, IPresignedUrlService presignedUrlService)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _presignedUrlService = presignedUrlService;
        }
        public async Task<AssessmentSubmissionProjectResponse> GetAssessmentById(Guid assessmentId, Guid currentUserId)
        {
            //Get currentClassId

            //Get currentSemesterId

            //Get currentProjectId
            // --> Get SubmissionModule
            //Can get ProjectSubmission base on projectId + submissionModule Id
            
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            var studiesIn = (await _commonServices.GetStudiesIn(currentUserId)).ToList();

            if (studiesIn.Count() == 0 || studiesIn == null)
                return null;

            IPMSClass? currentClass = (await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemesterId));

            if (currentClass == null) // Check null current user did not enrolled any class this semester
                return null;

            var currentStudyIn = studiesIn.FirstOrDefault(s => s.ClassId.Equals(currentClass.Id)); // Get current studying
            if (currentStudyIn == null)
                return null;

            Guid? projectId = (await _unitOfWork.ProjectRepository.Get().FirstOrDefaultAsync(p => p.Id.Equals(currentStudyIn.ProjectId)))?.Id;

            if (projectId == null)
                return null;

            Assessment? assessment = _unitOfWork.AssessmentRepository.Get().FirstOrDefault(a => a.Id.Equals(assessmentId));
            if (assessment == null)
                return null;

            var submissionsModule = _unitOfWork.SubmissionModuleRepository.Get()
                                                                .Where(sm => sm.AssessmentId.Equals(assessmentId)
                                                                        && sm.SemesterId.Equals(currentSemesterId)
                                                                        && sm.LectureId.Equals(currentClass.LecturerId))
                                                                .Include(sm => sm.ProjectSubmissions.Where(pm => pm.ProjectId.Equals(projectId))).ToList();

            AssessmentSubmissionProjectResponse response = new AssessmentSubmissionProjectResponse
            {
                Id = assessment.Id,
                Name = assessment.Name,
                SubmissionModules = submissionsModule.Select(sm => new SubmissionModuleResponse
                {
                    ModuleId = sm.Id,
                    Name = sm.Name,
                    StartDate = sm.StartDate,
                    EndDate = sm.EndDate,
                    Description = sm.Description,
                    ProjectSubmissions = sm.ProjectSubmissions.Select(ps => new ProjectSubmissionResponse
                    {
                        Id = ps.Id,
                        Name = ps.Name,
                        SubmitTime = ps.SubmissionDate,
                        Link = _presignedUrlService.GeneratePresignedDownloadUrl("PS_" + ps.Id + "_" + ps.Name) //Get base on name on S3 
                       
                    }).ToList()
                }).ToList()
            };
            return response;
        }
    }
}
