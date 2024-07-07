using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.Business.Responses.SubmissionModule;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class SubmissionModuleService : ISubmissionModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private List<SubmissionModule> _submissionModules = new();
        private List<Assessment> _assessments = new();
        private Guid _currentSemesterId;
        public SubmissionModuleService(IUnitOfWork unitOfWork, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
        }
        public async Task<ValidationResultModel> ConfigureSubmissionModuleValidator(ConfigureSubmissionModuleRequest request, Guid currentUserId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            bool isPerEqZero = request.SubmissionModules.Any(sm => sm.Percentage == 0);
            if (isPerEqZero) // percentage <= 0
            {
                result.Message = "Cannot set percentage lower or equals 0%";
                return result;
            }

            decimal isPerSumEqHundred = request.SubmissionModules.Where(sm => sm.IsDeleted == false).Sum(sm => sm.Percentage);
            if (isPerSumEqHundred != 100) // sub percentage != 100
            {
                result.Message = "Sum of percentage is different 100%";
                return result;
            }

            bool isStartGreaterEnd = request.SubmissionModules.Any(sm => sm.StartDate >= sm.EndDate);
            if (isStartGreaterEnd) // start date > end date
            {
                result.Message = "Start Date cannot equal or greater than End Date";
                return result;
            }

            bool isEndLessNow = request.SubmissionModules.Any(sm => sm.EndDate <= DateTime.Now 
                    || (sm.EndDate.Date == DateTime.Now.Date && sm.EndDate.Hour <= DateTime.Now.Hour)); // compare for hour

            if (isEndLessNow) // end date hour <= now hour
            {
                result.Message = "End Date must greater than now";
                return result;
            }

            bool isNameNull = request.SubmissionModules.Any(sm => sm.ModuleName == null || sm.ModuleName.Trim() == "");
            if (isNameNull) // module name null
            {
                result.Message = "Module Name cannot be null";
                return result;
            }

            _currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            _submissionModules = await _unitOfWork.SubmissionModuleRepository.Get().Where(sm => sm.AssessmentId.Equals(request.AssessmentId) 
                                            && sm.SemesterId.Equals(_currentSemesterId) 
                                            && sm.LectureId.Equals(currentUserId)).ToListAsync();
            foreach (var submissionModule in request.SubmissionModules) { 
                if (submissionModule.Description == null)
                {
                    submissionModule.Description = "";
                }
                if (submissionModule.ModuleId == null) // case create new module
                {
                    submissionModule.ModuleId = Guid.Empty;
                }

                if (submissionModule.ModuleId == Guid.Empty && submissionModule.IsDeleted == true) // submission module is not existed
                {
                    result.Message = "Cannot delete not exist module";
                    return result;
                }

                if (submissionModule.ModuleId != Guid.Empty)
                {
                    var isExisted = _submissionModules.FirstOrDefault(sm => sm.Id.Equals(submissionModule.ModuleId));
                    if (isExisted == null) // submission module is not existed
                    {
                        result.Message = "Module is not existed to update";
                        return result;
                    }
                }

            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task ConfigureSubmissionModule(ConfigureSubmissionModuleRequest request, Guid currentUserId)
        {
            var classes = await _commonServices.GetAllCurrentClassesOfLecturer(currentUserId);
            foreach (var submissionModule in request.SubmissionModules)
            {
                if (submissionModule.ModuleId == Guid.Empty) // create
                {
                    Guid id = Guid.NewGuid();
                    SubmissionModule subModule = new SubmissionModule
                    {
                        Id = id,
                        Name = submissionModule.ModuleName,
                        Description = submissionModule.Description,
                        Percentage = submissionModule.Percentage,
                        SemesterId = _currentSemesterId,
                        AssessmentId = request.AssessmentId,
                        LectureId = currentUserId,
                        ClassModuleDeadlines = classes.Select(@class=> new ClassModuleDeadline
                        {
                            ClassId = @class.Id,
                            SubmissionModuleId = id
                        }).ToList()
                    };
                    await _unitOfWork.SubmissionModuleRepository.InsertAsync(subModule);


                }
                else // update
                {
                    SubmissionModule subModule = _submissionModules.FirstOrDefault(sm => sm.Id.Equals(submissionModule.ModuleId));
                    subModule.Name = submissionModule.ModuleName;
                    subModule.Description = submissionModule.Description;
                    subModule.Percentage = submissionModule.Percentage;
                    subModule.IsDeleted = submissionModule.IsDeleted;
                    _unitOfWork.SubmissionModuleRepository.Update(subModule);


                }
            }
            await _unitOfWork.SaveChangesAsync();

        }
        public async Task<ValidationResultModel> GetAssessmentSubmissionModuleByClassValidator(GetSubmissionModuleByClassRequest request, Guid currentUserId)
        {
            _currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
        
            

            Semester semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(_currentSemesterId));
            if (semester == null)
            {
                result.Message = "Semester does not exist!";
                return result;
            }
            _assessments = await _unitOfWork.AssessmentRepository.Get().Where(a => a.SyllabusId.Equals(semester.SyllabusId)).ToListAsync();
            if (_assessments.Count == 0)
            {
                result.Message = "Assessment does not exist!";
                return result;
            }

            IPMSClass @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.classId)
                                            && c.LecturerId.Equals(currentUserId)
                                            && c.SemesterId.Equals(_currentSemesterId));
            if (@class == null) // validate class
            {
                result.Message = "Class does not exist!";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;

        }
        public async Task<IEnumerable<GetAssessmentSubmissionModuleByClassResponse>> GetAssessmentSubmissionModuleByClass(GetSubmissionModuleByClassRequest request, Guid currentUserId)
        {
            List<GetAssessmentSubmissionModuleByClassResponse> assessments = new List<GetAssessmentSubmissionModuleByClassResponse>();
            _submissionModules = await _unitOfWork.SubmissionModuleRepository.Get().Include(x=>x.ClassModuleDeadlines.Where(y=>y.ClassId == request.classId)).Where(sm => sm.SemesterId.Equals(_currentSemesterId)
                                            && sm.LectureId.Equals(currentUserId)).Include(sm => sm.ProjectSubmissions).ThenInclude(ps => ps.Grades).ToListAsync();
            List<LecturerGrade> graded = await _unitOfWork.LecturerGradeRepository.Get().Where(lg => lg.CommitteeId.Equals(currentUserId)).ToListAsync();

            var classTopics = await _unitOfWork.ClassTopicRepository.Get().Where(ct => ct.ClassId.Equals(request.classId) && ct.ProjectId != Guid.Empty).ToListAsync(); //get class topics have picked == project in class

            foreach (var assessment in _assessments)
            {
                List<ProjectSubmissionModule> modules = _submissionModules.Where(sm => sm.AssessmentId.Equals(assessment.Id)).Select(sm => new ProjectSubmissionModule
                {
                    ModuleId = sm.Id,
                    Title = sm.Name,
                    Graded = graded.Count(g => sm.ProjectSubmissions.Any(ps => g.SubmissionId.Equals(ps.Id))),
                    Submissions = sm.ProjectSubmissions.GroupBy(ps => ps.ProjectId).Count(),
                    Total = classTopics.Count(),
                    StartDate = sm.ClassModuleDeadlines.First().StartDate,
                    EndDate = sm.ClassModuleDeadlines.First().EndDate,
                    Percentage = sm.Percentage

                }).ToList();
                GetAssessmentSubmissionModuleByClassResponse submission = new GetAssessmentSubmissionModuleByClassResponse
                {
                    AssessmentId = assessment.Id,
                    Title = assessment.Name,
                    Percentage = assessment.Percentage,
                    Modules = modules
                };
                assessments.Add(submission);
            }

            return assessments;
        }

        
    }
}
