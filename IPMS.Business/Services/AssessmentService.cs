using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Assessment;
using IPMS.Business.Responses.Assessment;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.Business.Responses.SubmissionModule;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IPresignedUrlService _presignedUrlService;
        private readonly IHttpContextAccessor _context;


        public AssessmentService(IUnitOfWork unitOfWork, ICommonServices commonServices, IPresignedUrlService presignedUrlService, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _presignedUrlService = presignedUrlService;
            _context = context;
        }
        public async Task<ValidationResultModel> GetAssessmentByIdValidators(Guid assessmentId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            IPMSClass? @class = _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
            if (@class == null)
            {
                result.Message = ("Student did not enrolled any class this semester");
                return result;
            }

            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");
            if (project == null)
            {
                result.Message = ("Student not in any project currently");
                return result;

            }
            Assessment? assessment = _unitOfWork.AssessmentRepository.Get().FirstOrDefault(a => a.Id.Equals(assessmentId));
            if (assessment == null)
            {
                result.Message = ("Assessment does not exist");
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task<AssessmentSubmissionProjectResponse> GetAssessmentById(Guid assessmentId)
        {
            //Get currentClassId

            //Get currentSemesterId

            //Get currentProjectId
            // --> Get SubmissionModule
            //Can get ProjectSubmission base on projectId + submissionModule Id

            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            IPMSClass? @class = _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");
            Assessment? assessment = _unitOfWork.AssessmentRepository.Get().FirstOrDefault(a => a.Id.Equals(assessmentId));



            var submissionsModule = _unitOfWork.SubmissionModuleRepository.Get()
                                                                .Where(sm => sm.AssessmentId.Equals(assessmentId)
                                                                        && sm.SemesterId.Equals(currentSemesterId)
                                                                        && sm.LectureId.Equals(@class.LecturerId))
                                                                .Include(sm => sm.ProjectSubmissions.Where(pm => pm.ProjectId.Equals(project.Id)))
                                                                .Include(sm => sm.ClassModuleDeadlines.Where(cl => cl.ClassId == @class.Id)).Select(sm => new SubmissionModuleResponse
                                                                {
                                                                    ModuleId = sm.Id,
                                                                    Name = sm.Name,
                                                                    StartDate = sm.ClassModuleDeadlines.First().StartDate,
                                                                    EndDate = sm.ClassModuleDeadlines.First().EndDate,
                                                                    Description = sm.Description,
                                                                    ProjectSubmissions = sm.ProjectSubmissions.Select(ps => new ProjectSubmissionResponse
                                                                    {
                                                                        Id = ps.Id,
                                                                        Name = ps.Name,
                                                                        SubmitTime = ps.SubmissionDate,
                                                                        Link = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, ps.Id, ps.Name)) ?? string.Empty //Get base on name on S3 

                                                                    }).ToList()
                                                                }).ToList();

            AssessmentSubmissionProjectResponse response = new AssessmentSubmissionProjectResponse
            {
                Id = assessment.Id,
                Name = assessment.Name,
                SubmissionModules = submissionsModule
            };
            return response;
        }

        public async Task<IEnumerable<GetAllAssessmentsResponse>> GetAllAssessments(GetAllAssessmentsRequest request)
        {

            if (request.Name == null)
            {
                request.Name = "";
            }
            IQueryable<Assessment> assessments;
            if (request.Id != Guid.Empty)
            {
                assessments = _unitOfWork.AssessmentRepository.Get().Where(a => a.Name.ToLower().Contains(request.Name.ToLower()) || a.Id.Equals(request.Id)).Include(a => a.Syllabus);
            }
            else
            {
                assessments = _unitOfWork.AssessmentRepository.Get().Where(a => a.Name.ToLower().Contains(request.Name.ToLower())).Include(a => a.Syllabus);
            }

            IEnumerable<GetAllAssessmentsResponse> response = assessments.Select(a => new GetAllAssessmentsResponse
            {
                Id = a.Id,
                Order = a.Order,
                Name = a.Name,
                Percentage = a.Percentage,
                Description = a.Description,
                SyllabusName = a.Syllabus.Name
            });

            return response;
        }
    }
}
