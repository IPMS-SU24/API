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
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

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
                                                                .Include(sm => sm.ProjectSubmissions)
                                                                .Include(sm => sm.ClassModuleDeadlines).Select(sm => new SubmissionModuleResponse
                                                                {
                                                                    ModuleId = sm.Id,
                                                                    Name = sm.Name,
                                                                    StartDate = sm.ClassModuleDeadlines.Where(cl => cl.ClassId.Equals(@class.Id)).First().StartDate,
                                                                    EndDate = sm.ClassModuleDeadlines.Where(cl => cl.ClassId.Equals(@class.Id)).First().EndDate,
                                                                    Description = sm.Description,
                                                                    ProjectSubmissions = sm.ProjectSubmissions.Where(pm => pm.ProjectId.Equals(project.Id)).Select(ps => new ProjectSubmissionResponse
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
                assessments = _unitOfWork.AssessmentRepository.Get().Where(a => a.Name.ToLower().Contains(request.Name.ToLower()) || a.Id.Equals(request.Id)).Include(a => a.Syllabus).ThenInclude(s => s.Semesters);
            }
            else
            {
                assessments = _unitOfWork.AssessmentRepository.Get().Where(a => a.Name.ToLower().Contains(request.Name.ToLower())).Include(a => a.Syllabus).ThenInclude(s => s.Semesters);
            }

            var semesterId = await _unitOfWork.SemesterRepository.Get().Select(s => s.Id).ToListAsync();

            var now = DateTime.Now;
            var curSemester = (await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.EndDate > now && x.StartDate < now)); //null .Id = 500
            Guid curSemesterId = Guid.Empty;
            if (curSemester != null)
            {
                curSemesterId = curSemester.Id;

            }
            IEnumerable<GetAllAssessmentsResponse> response = assessments.Select(a => new GetAllAssessmentsResponse
            {
                Id = a.Id,
                Order = a.Order,
                Name = a.Name,
                Percentage = a.Percentage,
                Description = a.Description,
                SyllabusName = a.Syllabus.Name,
                IsEdited = a.Syllabus.Semesters.All(s => s.Id.Equals(curSemesterId) == false)
            });

            return response;
        }
        public async Task<ValidationResultModel> ConfigureAssessmentsValidators(ConfigureAssessmentsRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            var isNewDelete = request.Assessments.Any(a => a.IsDelete == true && a.Id == Guid.Empty);
            if (isNewDelete)
            {
                result.Message = "Cannot set new Assessment Deleted";
                return result;
            }

            var dupRequestId = request.Assessments
                    .GroupBy(g => g.Id)
                    .Where(g => g.Count() > 1 && g.Key != Guid.Empty)
                    .Select(g => g.Key)
                    .ToList();
            if (dupRequestId.Any())
            {
                result.Message = "Does not send duplicate request";
                return result;
            }

            var dupRequestOrder = request.Assessments // for case delete, undeleted just set order not same
                    .GroupBy(g => g.Order)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();
            if (dupRequestOrder.Any())
            {
                result.Message = "Does not send duplicate Order";
                return result;
            }

            var syllabus = await _unitOfWork.SyllabusRepository.Get().Where(s => s.Id.Equals(request.SyllabusId)).Include(s => s.Semesters).Include(s => s.Assessments).FirstOrDefaultAsync();
            if (syllabus == null)
            {
                result.Message = "Syllabus not found";
                return result;
            }
            
            if (syllabus.Semesters.Count() > 0)
            {
                result.Message = "Cannot configure assessment used";
                return result;
            }
            var assMatches = syllabus.Assessments.Count(a => request.Assessments.Select(ra => ra.Id).Contains(a.Id));
            var reqAssExisted = request.Assessments.Count(a => a.Id != Guid.Empty);
            if (assMatches != reqAssExisted || assMatches != syllabus.Assessments.Count())
            {
                result.Message = "Please send all assessments existed";
                return result;
            }

            var sumPercentage = request.Assessments.Where(a => a.IsDelete == false).Sum(a => a.Percentage);
            if (sumPercentage != 100)
            {
                result.Message = "Please set percentage is 100%";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task ConfigureAssessments(ConfigureAssessmentsRequest request)
        {
            var insertList = new List<Assessment>();    
            foreach(var ass in request.Assessments)
            {
                if (ass.Id == Guid.Empty)
                {
                    insertList.Add(new Assessment
                    {
                        Name = ass.Name,
                        Order = ass.Order,
                        Description = ass.Description,
                        Percentage = ass.Percentage,
                        SyllabusId = request.SyllabusId,
                    });
                } else
                {
                    var uptAss = await _unitOfWork.AssessmentRepository.Get().FirstOrDefaultAsync(a => a.Id.Equals(ass.Id));
                    uptAss.Name = ass.Name;
                    uptAss.Order = ass.Order;
                    uptAss.Description = ass.Description;
                    uptAss.Percentage = ass.Percentage;
                    uptAss.IsDeleted = ass.IsDelete;
                    _unitOfWork.AssessmentRepository.Update(uptAss);

                }
            }
            await _unitOfWork.AssessmentRepository.InsertRangeAsync(insertList);
            await _unitOfWork.SaveChangesAsync();
        }


    }
}
