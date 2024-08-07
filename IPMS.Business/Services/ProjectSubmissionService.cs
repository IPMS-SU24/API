using IPMS.Business.Common.Constants;
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
using NPOI.Util;
using Org.BouncyCastle.Bcpg;
using System.Collections.Immutable;
using System.Reflection;

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
            var submissionModule = _unitOfWork.SubmissionModuleRepository.Get().Include(x => x.ClassModuleDeadlines).FirstOrDefault(sm => sm.Id.Equals(request.SubmissionModuleId)); // Find submission module
            if (submissionModule == null)
            {
                result.Message = "Submission module does not exist";
                return result;
            }

            if (submissionModule.ClassModuleDeadlines.Where(y => y.ClassId == _commonServices.GetClass()!.Id).First().EndDate < request.SubmissionDate) // Validation submit time
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
            var submission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => ps.Id.Equals(request.SubmissionId)).Include(s => s.SubmissionModule).FirstOrDefaultAsync();

            if (submission == null)
            {
                result.Message = "Project submission cannot found";
                return result;
            }

            @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Committees.Select(c => c.LecturerId).Contains(lecturerId)
                                        && c.Students.Any(s => s.ProjectId.Equals(submission.ProjectId)))
                                .Include(c => c.ClassModuleDeadlines.Where(cm => cm.SubmissionModuleId.Equals(submission.SubmissionModuleId))) // can not set FirstOrDefault
                                .Include(c => c.Committees.Where(c => c.LecturerId.Equals(lecturerId)))
                                .FirstOrDefaultAsync();

            if (@class == null)
            {
                result.Message = "Class cannot found";
                return result;
            }

            var lastAss = await _unitOfWork.AssessmentRepository.Get().Where(x=>x.Modules.Any(m=>m.Id == submission.SubmissionModuleId)).OrderByDescending(a => a.Order).FirstOrDefaultAsync();

            if (lastAss!.Id != submission.SubmissionModule.AssessmentId && lecturerId != @class.LecturerId) // check committee grade final assessment
            {
                result.Message = "Committee cannot grade submission different final assessment";
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

            var lastSubmission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => ps.SubmissionDate <= moduleDeadline.EndDate && ps.SubmissionModuleId.Equals(submission.SubmissionModuleId) && submission.ProjectId == ps.ProjectId).OrderByDescending(ps => ps.SubmissionDate).FirstOrDefaultAsync();

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
            var submission = await _unitOfWork.ProjectSubmissionRepository.Get().Where(ps => ps.Id.Equals(request.SubmissionId))
                                    .Include(s => s.SubmissionModule)
                                    .FirstOrDefaultAsync();

            @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Committees.Select(c => c.LecturerId).Contains(lecturerId)
                                       && c.Students.Any(s => s.ProjectId.Equals(submission.ProjectId)))
                                    .Include(c => c.Committees.Where(c => c.LecturerId.Equals(lecturerId)))
                                    .FirstOrDefaultAsync();

            var committee = await _unitOfWork.CommitteeRepository.Get().Where(c => c.ClassId.Equals(@class.Id)).ToListAsync();
            var curCommittee = committee.FirstOrDefault(c => c.LecturerId.Equals(lecturerId))!.Id;

            var grade = await _unitOfWork.LecturerGradeRepository.Get().FirstOrDefaultAsync(lg => lg.CommitteeId.Equals(curCommittee) && lg.SubmissionId.Equals(request.SubmissionId));
            if (grade == null)
            {
                grade = new LecturerGrade
                {
                    CommitteeId = curCommittee,
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
            await _unitOfWork.SaveChangesAsync(); // save to cal average below

            #region FinalGrade

            var lastAss = await _unitOfWork.AssessmentRepository.Get().OrderByDescending(a => a.Order).FirstOrDefaultAsync();
            if (lastAss.Id == submission.SubmissionModule.AssessmentId) //is final assessment --> Check with committee
            {
                var lecGrade = await _unitOfWork.LecturerGradeRepository.Get().Where(lg => committee.Select(c => c.Id).Contains(lg.CommitteeId) && lg.SubmissionId.Equals(submission.Id)).ToListAsync();
                if (committee.Count() == lecGrade.Count()) // all committee graded
                {
                    decimal avg = 0;
                    foreach (var c in committee)
                    {
                        avg += c.Percentage / 100 * lecGrade.First(lg => lg.CommitteeId.Equals(c.Id)).Grade!.Value;
                    }

                    submission.FinalGrade = avg;
                }

            }
            else // is not final assessment --> Add final grade because just lecturer grade  ---- have validate for lecturer is teaching this class
            {
                submission.FinalGrade = request.Grade;
            }

            _unitOfWork.ProjectSubmissionRepository.Update(submission);

            #endregion

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<GetClassesCommitteeResponse>> GetClassesCommittee(GetClassesCommitteeRequest request, Guid lecturerId)
        {
            List<GetClassesCommitteeResponse> classes = new List<GetClassesCommitteeResponse>();
            var semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(s => s.ShortName.ToLower().Equals(request.SemesterCode.ToLower()));
            if (semester == null)
            {
                return classes;
            }
            classes = _unitOfWork.CommitteeRepository.Get().Where(c => c.Class.SemesterId.Equals(semester.Id) && !c.Class.LecturerId.Equals(lecturerId) && c.LecturerId.Equals(lecturerId))
                .Include(c => c.Class)
                .ThenInclude(c => c.Students)
                .Select(c => new GetClassesCommitteeResponse
                {
                    ClassId = c.ClassId,
                    GroupNum = c.Class.Students.Where(s => s.ProjectId != null).GroupBy(s => s.ProjectId).Count(),
                    ClassCode = c.Class.ShortName,
                    ClassName = c.Class.Name,
                    StudentNum = c.Class.Students.Count()
                }).ToList();

            return classes;
        }

        public async Task<IEnumerable<GetFinalAssessmentResponse>> GetFinalAssessment(GetFinalAssessmentRequest request, Guid lecturerId)
        {
            List<GetFinalAssessmentResponse> final = new List<GetFinalAssessmentResponse>();

            var validCommittee = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(request.ClassId)).Include(c => c.Committees.Where(com => com.LecturerId.Equals(lecturerId))).FirstOrDefaultAsync();
            if (validCommittee == null) // class does not exist
            {
                return final;
            }

            if (validCommittee.Committees.Count() != 1) // current lecturer does not have permission
            {
                return final;
            }

            var lastAss = await _unitOfWork.AssessmentRepository.Get().OrderByDescending(a => a.Order).FirstOrDefaultAsync();

            if (lastAss == null) // cannot find assessment
            {
                return final;
            }

            var semester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            if (semester == null)
            {
                return final;
            }

            var submissions = await _unitOfWork.ProjectSubmissionRepository.Get().Where(pm => pm.ProjectId.Equals(request.GroupId))
                                    .Include(pm => pm.Grades.Where(g => g.CommitteeId.Equals(validCommittee.Committees.First().Id)))
                                    .OrderByDescending(pm => pm.SubmissionDate) // to get first or default below
                                    .ToListAsync();
            var modules = await _unitOfWork.SubmissionModuleRepository.Get().Where(sm => sm.AssessmentId.Equals(lastAss.Id) && sm.SemesterId.Equals(semester.Id)).Include(sm => sm.ClassModuleDeadlines.Where(cm => cm.ClassId.Equals(request.ClassId))).ToListAsync();
            foreach (var m in modules)
            {
                if (m.ClassModuleDeadlines.Count() != 1) // 1 submission - 1 class - not found --> config wrong
                {
                    return final;
                }
                var submitted = submissions.FirstOrDefault(s => s.SubmissionModuleId.Equals(m.Id) && s.SubmissionDate <= m.ClassModuleDeadlines.First().EndDate);
                decimal grade = 0;
                if (submitted != null)
                {
                    grade = submitted.Grades.Count() == 1 ? submitted.Grades.First().Grade!.Value : 0;
                }
                final.Add(new GetFinalAssessmentResponse
                {
                    ModuleId = m.Id,
                    SubmissionId = submitted == null ? Guid.Empty : submitted.Id,
                    Description = m.Description,
                    ModuleName = m.Name,
                    Percentage = m.Percentage,
                    FileLink = submitted == null ? "" : _presignedUrl.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, submitted.Id, submitted.Name)) ?? string.Empty,
                    Grade = grade,
                });


            }
            return final;
        }

        public async Task<GetGradeResponse> GetGradeAsync(Guid studentId, Guid projectId)
        {
            var targetStudent = await _unitOfWork.StudentRepository.Get().FirstOrDefaultAsync(x => x.InformationId == studentId && x.ProjectId == projectId);
            if (targetStudent == null)
            {
                throw new DataNotFoundException("Not Found Project");
            }
            var semesterId = await _unitOfWork.IPMSClassRepository.Get().Where(x => x.Id == targetStudent.ClassId).Select(x => x.SemesterId).FirstOrDefaultAsync();
            var response = new GetGradeResponse
            {
                AssessmentGrades = await _unitOfWork.SubmissionModuleRepository.Get().Include(x => x.Assessment)
                                                                                            .Where(x => x.SemesterId == semesterId)
                                                                                            .GroupBy(x => x.AssessmentId)
                                                                                            .Select(x => new AssessmentGrade
                                                                                            {
                                                                                                Id = x.Key,
                                                                                                Order = x.First().Assessment.Order,
                                                                                                Name = x.First().Assessment.Name,
                                                                                                Percentage = x.First().Assessment.Percentage,
                                                                                                SubmissionGrades = x.Select(sm => new SubmissionGrade
                                                                                                {
                                                                                                    Id = sm.Id,
                                                                                                    Percentage = sm.Percentage,
                                                                                                    Name = sm.Name,
                                                                                                    Grade = sm.ProjectSubmissions.FirstOrDefault(ps => ps.ProjectId == projectId) != null ?
                                                                                                            sm.ProjectSubmissions.First(ps => ps.ProjectId == projectId).FinalGrade : null
                                                                                                }).ToList(),
                                                                                            }).ToListAsync()
            };
            //var modules = response.AssessmentGrades.SelectMany(x=>x.SubmissionGrades).ToList();
            //var assessmentGrades = await _unitOfWork.ProjectSubmissionRepository.Get()
            //                                                                      .Include(x => x.SubmissionModule)
            //                                                                      .ThenInclude(x => x.Assessment)
            //                                                                      .Where(x => x.ProjectId == projectId && modules.Select(m => m.Id).Contains(x.SubmissionModuleId))
            //                                                                      .OrderBy(x => x.SubmissionModule.Assessment.Order)
            //                                                                      .GroupBy(x => x.SubmissionModule.AssessmentId)
            //                                                                      .Select(x => new AssessmentGrade
            //                                                                      {
            //                                                                          Id = x.Key,
            //                                                                          Name = x.First().SubmissionModule.Assessment.Name,
            //                                                                          Percentage = x.First().SubmissionModule.Assessment.Percentage,
            //                                                                          SubmissionGrades = x.Select(sg => new SubmissionGrade
            //                                                                          {
            //                                                                              Grade = sg.FinalGrade,
            //                                                                              Percentage = sg.SubmissionModule.Percentage,
            //                                                                              Name = sg.SubmissionModule.Name
            //                                                                          }).ToList(),
            //                                                                      }).ToListAsync();
            //if (assessmentGrades == null || !assessmentGrades.Any())
            //{
            //    throw new DataNotFoundException();
            //}

            //Calc Total
            response.Total = 0;
            foreach (var assGrade in response.AssessmentGrades)
            {
                if (assGrade.AssessmentAvg == null)
                {
                    response.Total = null;
                    break;
                }
                else
                {
                    response.Total += assGrade.AssessmentAvg * (assGrade.Percentage / 100);
                }
            }
            response.AssessmentGrades = response.AssessmentGrades.OrderBy(x => x.Order).ToList();
            return response;
        }
    }
}
