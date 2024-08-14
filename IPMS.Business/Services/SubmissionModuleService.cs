using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.SubmissionModule;
using IPMS.Business.Responses.SubmissionModule;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;

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

            bool isNameNull = request.SubmissionModules.Any(sm => sm.ModuleName == null || sm.ModuleName.Trim() == "");
            if (isNameNull) // module name null
            {
                result.Message = "Module Name cannot be null";
                return result;
            }
            
            var semester = await _unitOfWork.SemesterRepository.Get().Where(s => s.ShortName.Equals(request.SemesterCode)).FirstOrDefaultAsync();
            if (semester == null)
            {
                result.Message = "Semester not found";
                return result;
            }
            if (semester.StartDate  <= DateTime.Now)
            {
                result.Message = "Cannot configure for semester started";
                return result;
            }
            bool isInterval = request.SubmissionModules.Any(sm => sm.StartDate < semester.StartDate || sm.EndDate > semester.EndDate);
            if (isInterval)
            {
                result.Message = "Please set a deadline during the semester.";
                return result;
            }

            _submissionModules = await _unitOfWork.SubmissionModuleRepository.Get().Include(x => x.Semester).Where(sm => sm.AssessmentId.Equals(request.AssessmentId)
                                            && sm.Semester.ShortName.Equals(request.SemesterCode)
                                            && sm.LectureId.Equals(currentUserId)).ToListAsync();
            foreach (var submissionModule in request.SubmissionModules)
            {
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
            var semesterId = _unitOfWork.SemesterRepository.Get().First(x => x.ShortName == request.SemesterCode).Id;
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
                        SemesterId = semesterId,
                        AssessmentId = request.AssessmentId,
                        LectureId = currentUserId,
                        ClassModuleDeadlines = classes.Select(classId => new ClassModuleDeadline
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
            var semesterId = (await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(x => x.Id == request.classId)).SemesterId;
            _submissionModules = await _unitOfWork.SubmissionModuleRepository.Get().Include(x => x.ClassModuleDeadlines.Where(y => y.ClassId == request.classId)).Where(sm => sm.SemesterId.Equals(semesterId)
                                            && sm.LectureId.Equals(currentUserId)).Include(sm => sm.ProjectSubmissions).ThenInclude(ps => ps.Grades).ToListAsync();
            List<LecturerGrade> graded = await _unitOfWork.LecturerGradeRepository.Get().Where(lg => lg.Committee.LecturerId.Equals(currentUserId)).ToListAsync();

            //var classTopics = await _unitOfWork.ClassTopicRepository.Get().Where(ct => ct.ClassId.Equals(request.classId) && ct.ProjectId != null).ToListAsync(); //get class topics have picked == project in class
            var projectCount = await _unitOfWork.StudentRepository.Get().Where(stu => stu.ClassId == request.classId && stu.ProjectId != null).Select(x => x.ProjectId).Distinct().CountAsync();
            foreach (var assessment in _assessments.OrderBy(x=>x.Order))
            {
                List<ProjectSubmissionModule> modules = _submissionModules.Where(sm => sm.AssessmentId.Equals(assessment.Id)).Select(sm => new ProjectSubmissionModule
                {
                    ModuleId = sm.Id,
                    Title = sm.Name,
                    Graded = graded.Count(g => sm.ProjectSubmissions.Any(ps => g.SubmissionId.Equals(ps.Id))),
                    Submissions = sm.ProjectSubmissions.GroupBy(ps => ps.ProjectId).Count(),
                    Total = projectCount,
                    StartDate = sm.ClassModuleDeadlines.First().StartDate,
                    EndDate = sm.ClassModuleDeadlines.First().EndDate,
                    Percentage = sm.Percentage,
                    Description = sm.Description

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

            var submissionRaw = await _unitOfWork.ProjectSubmissionRepository.Get().Where(p => p.SubmissionModuleId.Equals(request.ModuleId) && p.SubmissionDate <= deadline.EndDate)
                .OrderByDescending(p => p.SubmissionDate).Include(ps => ps.Project).Include(ps => ps.Grades.Where(g => g.Committee.LecturerId.Equals(lecturerId))).ToListAsync();
            submissions = submissionRaw.GroupBy(p => p.ProjectId).Select(group => new GetSubmissionsResponse
                {
                    SubmitDate = group.First().SubmissionDate,
                    DownloadUrl = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, group.First().Id, group.First().Name)) ?? String.Empty,
                    GroupNum = group.First().Project.GroupNum,
                    Grade = group.First().Grades.FirstOrDefault(g => g.SubmissionId.Equals(group.First().Id)) == null ? 0 
                        : group.First().Grades.FirstOrDefault(g => g.SubmissionId.Equals(group.First().Id)).Grade,
                    SubmissionId = group.First().Id,
                    GroupId = group.First().ProjectId,
                    Response = group.First().Grades.FirstOrDefault(g => g.SubmissionId.Equals(group.First().Id)) == null ? string.Empty
                        : group.First().Grades.FirstOrDefault(g => g.SubmissionId.Equals(group.First().Id)).Response,
            }).ToList();

            return submissions;
        }

        public async Task<ValidationResultModel> LecturerEvaluateValidator(LecturerEvaluateRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            var isAbnormalPer = request.Members.Any(s => s.Percentage < 0 || s.Percentage > 100);
            if (isAbnormalPer)
            {
                result.Message = "Percentage must be between 0 and 100";
                return result;
            }

            _currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.SemesterId.Equals(_currentSemesterId) && c.LecturerId.Equals(lecturerId));
            if (@class == null)
            {
                result.Message = "Class does not exist";
                return result;
            }

            var prjStudents = await _unitOfWork.StudentRepository.Get().Where(s => s.ProjectId.Equals(request.GroupId) && s.ClassId.Equals(request.ClassId))
                                    .ToListAsync();

            if (prjStudents.Count() != request.Members.Count())
            {
                result.Message = "Please evaluate all members in group";
                return result;
            }

            var isAnyOutGroup = prjStudents.Any(ps => request.Members.Select(m => m.StudentId).Contains(ps.InformationId) == false);
            if (isAnyOutGroup == true)
            {
                result.Message = "Student does not in group";
                return result;
            }

            return await CheckFinalGradeSubmission(_currentSemesterId, lecturerId, @class.Id, request.GroupId);

        }

        public async Task LecturerEvaluate(LecturerEvaluateRequest request, Guid lecturerId)
        {
            var _currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;

            var prjStudents = await _unitOfWork.StudentRepository.Get().Where(s => s.ProjectId.Equals(request.GroupId) && s.ClassId.Equals(request.ClassId))
                                    .ToListAsync();

            #region CalcStuFinalGrade
            var assessments = await _unitOfWork.AssessmentRepository.Get().Where(a => a.SyllabusId.Equals(_currentSemester.SyllabusId))
                                    .Include(a => a.Modules.Where(sm => sm.SemesterId.Equals(_currentSemester.Id) && sm.LectureId.Equals(lecturerId)))
                                    .ThenInclude(sm => sm.ClassModuleDeadlines.Where(cd => cd.ClassId.Equals(request.ClassId)))
                                    .ToListAsync();
            decimal totalAvg = 0;
            foreach (var ass in assessments)
            {
                decimal assessmentAvg = 0;
                foreach (var sub in ass.Modules)
                {
                    var prjSubmission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps =>
                        ps.SubmissionDate <= sub.ClassModuleDeadlines.First().EndDate
                        && ps.ProjectId.Equals(request.GroupId)
                        && ps.SubmissionModuleId.Equals(sub.Id)).OrderByDescending(ps => ps.SubmissionDate).FirstOrDefaultAsync();

                    if (prjSubmission != null)
                    {
                        assessmentAvg += sub.Percentage / 100 * (prjSubmission.FinalGrade == null ? 0 : prjSubmission.FinalGrade!.Value);
                    }
                }
                totalAvg += ass.Percentage / 100 * assessmentAvg;
            }

            foreach (var stu in prjStudents)
            {
                stu.FinalPercentage = request.Members.First(s => s.StudentId.Equals(stu.InformationId)).Percentage;
                stu.FinalGrade = stu.FinalPercentage / 100 * totalAvg;
                _unitOfWork.StudentRepository.Update(stu);
            }

            await _unitOfWork.SaveChangesAsync();

            // update final percentage and final grade
            // cal final grade
            // 1.1 get all assessment
            // 1.2 get all submission module on every assessment
            // 1.3 get all last submission of current project of every submission module
            //  If any last submission does not have final grade -> Prevent evaluate
            //  If have submission module but not have submission: Grade = 0
            // 1.4 Calc submission base on submission module percentage -> assessment grade
            // 1.5 Base on assessment grade -> Calc final grade
            // 1.6 From final grade --> Calc for student with final percentage
            #endregion

        }

        public async Task<ValidationResultModel> CheckFinalGradeSubmission(Guid semesterId, Guid lecturerId, Guid classId, Guid projectId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            var subModules = await _unitOfWork.SubmissionModuleRepository.Get().Where(sm =>
                                sm.SemesterId.Equals(semesterId)
                                && sm.LectureId.Equals(lecturerId))
                                .Include(sm =>
                                    sm.ClassModuleDeadlines.Where(cd => cd.ClassId.Equals(classId)))
                                .ToListAsync();

            var submissions = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => subModules.Select(sm => sm.Id).Contains(ps.SubmissionModuleId) && ps.ProjectId.Equals(projectId)).OrderByDescending(ps => ps.SubmissionDate).ToListAsync();
            foreach (var module in subModules)
            {
                var sub = submissions.FirstOrDefault(s => s.SubmissionDate <= module.ClassModuleDeadlines.First().EndDate && s.SubmissionModuleId.Equals(module.Id));
                if (sub != null)
                {
                    if (sub.FinalGrade == null)
                    {
                        result.Message = "Please grade all submission before evaluate percentage";
                        return result;
                    }
                }
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public Task<ValidationResultModel> CalcFinalGradeValidator(CalcFinalGradeRequest request, Guid lecturerId)
        {
            throw new NotImplementedException();
        }
        public Task CalcFinalGrade(CalcFinalGradeRequest request, Guid lecturerId)
        {
            throw new NotImplementedException();
        }

        public async Task<ValidationResultModel> ConfigureClassModuleDeadlineValidators(ConfigureClassModuleDeadlineRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            var @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId)).Include(c => c.Semester).FirstOrDefaultAsync();
            if (@class == null)
            {
                result.Message = "Class not found";
                return result;
            }

            if (@class.Semester == null)
            {
                result.Message = "Semester not found";
                return result;
            }

            if (@class.Semester.EndDate < DateTime.Now)
            {
                result.Message = "Cannot set for class in previous semester";
                return result;
            }

            if (request.StartDate >= request.EndDate)
            {
                result.Message = "Cannot set Start Date greater than End Date";
                return result;
            }

            if (request.StartDate < @class.Semester.StartDate)
            {
                result.Message = "Cannot set Start Date Module lower than Start Date Semester";
                return result;
            }

            if (request.EndDate > @class.Semester.EndDate)
            {
                result.Message = "Cannot set End Date Module greater than End Date Semester";
                return result;
            }

            var submissionModule = await _unitOfWork.SubmissionModuleRepository.Get().FirstOrDefaultAsync(sm => sm.Id.Equals(request.SubmissionModuleId));
            if (submissionModule == null)
            {
                result.Message = "Submission Module not found";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task ConfigureClassModuleDeadline(ConfigureClassModuleDeadlineRequest request, Guid lecturerId)
        {
            var classModuleDl = await _unitOfWork.ClassModuleDeadlineRepository.Get().FirstOrDefaultAsync(cmd => cmd.SubmissionModuleId.Equals(request.SubmissionModuleId) && cmd.ClassId.Equals(request.ClassId));
            if (classModuleDl == null)
            {
                await _unitOfWork.ClassModuleDeadlineRepository.InsertAsync(new ClassModuleDeadline
                {
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    SubmissionModuleId = request.SubmissionModuleId,
                    ClassId = request.ClassId
                });
            }
            else
            {
                classModuleDl.StartDate = request.StartDate;
                classModuleDl.EndDate = request.EndDate;
                classModuleDl.SubmissionModuleId = request.SubmissionModuleId;
                classModuleDl.ClassId = request.ClassId;
                _unitOfWork.ClassModuleDeadlineRepository.Update(classModuleDl);
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
