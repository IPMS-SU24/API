using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.Business.Responses.SubmissionModule;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IPMS.Business.Services
{
    public class SubmissionModuleService : ISubmissionModuleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private List<SubmissionModule> _submissionModules = new();
        private List<Assessment> _assessments = new();
        private Guid _currentSemesterId;
        private readonly IPresignedUrlService _presignedUrlService;

        public SubmissionModuleService(IUnitOfWork unitOfWork, ICommonServices commonServices, IPresignedUrlService presignedUrlService)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _presignedUrlService = presignedUrlService;
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
            var isCurrentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester.ShortName == request.SemesterCode;
            if(isCurrentSemester)
            {
                result.Message = "Cannot update module in current semester";
                return result;
            }
            _submissionModules = await _unitOfWork.SubmissionModuleRepository.Get().Include(x=>x.Semester).Where(sm => sm.AssessmentId.Equals(request.AssessmentId) 
                                            && sm.Semester.ShortName.Equals(request.SemesterCode) 
                                            && sm.LectureId.Equals(currentUserId)).ToListAsync();
            foreach (var submissionModule in request.SubmissionModules) { 
                if (submissionModule.Description == null)
                {
                    submissionModule.Description = "";
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
            var classes = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester)
                                                                .Where(x => x.Semester.ShortName == request.SemesterCode && x.LecturerId == currentUserId)
                                                                .Select(x => x.Id).ToListAsync();
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
                        ClassModuleDeadlines = classes.Select(classId=> new ClassModuleDeadline
                        {
                            ClassId = classId,
                            SubmissionModuleId = id,
                            StartDate = submissionModule.StartDate,
                            EndDate = submissionModule.EndDate,
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
                                            && c.LecturerId.Equals(currentUserId));
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

            var classTopics = await _unitOfWork.ClassTopicRepository.Get().Where(ct => ct.ClassId.Equals(request.classId) && ct.ProjectId != null).ToListAsync(); //get class topics have picked == project in class

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

        public async Task<IEnumerable<GetSubmissionsResponse>> GetSubmissions(GetSubmissionsRequest request, Guid lecturerId)
        {
            IEnumerable<GetSubmissionsResponse> submissions = new List<GetSubmissionsResponse>();
            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.LecturerId.Equals(lecturerId) && c.Id.Equals(request.ClassId));
            if (@class == null)
            {
                return submissions;
            }
            var deadline = await _unitOfWork.ClassModuleDeadlineRepository.Get().FirstOrDefaultAsync(d => d.SubmissionModuleId.Equals(request.ModuleId) && d.ClassId.Equals(request.ClassId));
            if (deadline == null)
            {
                return submissions;
            }
            //var deadline = await _unitOfWork.ClassModuleDeadline.
            /*  var mockSubmissions = await _unitOfWork.SubmissionModuleRepository.Get().Where(s => s.Id.Equals(request.ModuleId))
                      .Include(s => s.ProjectSubmissions).ThenInclude(p => p.Project)
                      .Include(s => s.ProjectSubmissions).ThenInclude(p => p.Grades.Where(g => g.CommitteeId.Equals(lecturerId)))
                      .ToListAsync();
  */
            submissions = await _unitOfWork.ProjectSubmissionRepository.Get().Where(p => p.SubmissionModuleId.Equals(request.ModuleId) && p.SubmissionDate <= deadline.EndDate)
                .OrderByDescending(p => p.SubmissionDate).GroupBy(p => p.ProjectId).Select(group =>  new GetSubmissionsResponse
                {
                    SubmitDate = group.First().SubmissionDate,
                    DownloadUrl = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, group.First().Id, group.First().Name)) ?? String.Empty,
                    GroupNum = group.First().Project.GroupNum,
                    Grade = group.First().Grades.FirstOrDefault(g => g.SubmissionId.Equals(group.First().Id)).Grade ?? 0,
                    SubmissionId = group.First().Id,
                    GroupId = group.First().ProjectId,
                }).ToListAsync();

            return submissions;
        }
    }
}
