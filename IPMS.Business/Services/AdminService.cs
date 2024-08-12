using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Models;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Responses.Admin;
using IPMS.Business.Responses.Authentication;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly ICommonServices _commonService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPresignedUrlService _presignedUrlService;

        public AdminService(UserManager<IPMSUser> userManager,
                                   ICommonServices commonService,
                                   IUnitOfWork unitOfWork,
                                   IPresignedUrlService presignedUrlService)
        {
            _userManager = userManager;
            _commonService = commonService;
            _unitOfWork = unitOfWork;
            _presignedUrlService = presignedUrlService;
        }
        public async Task<IList<LectureAccountResponse>> GetLecturerAsync()
        {
            return (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).Select(x => new LectureAccountResponse
            {
                Id = x.Id,
                Name = x.FullName,
            }).ToList();
        }
        public async Task<IEnumerable<LectureAccountResponse>> GetLecturerList(GetLecturerListRequest request)
        {
            var lecturers = (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).Select(x => new LectureAccountResponse
            {
                Id = x.Id,
                Name = x.FullName,
                Email = x.Email,
                Phone = x.PhoneNumber
            }).ToList();
            if (request.Name != null && request.Email != null)
            {

                lecturers = lecturers.Where(l => l.Name.ToLower().Contains(request.Name.ToLower()) || l.Email.ToLower().Contains(request.Email.ToLower())).ToList();
            }
            else if (request.Name != null)
            {
                lecturers = lecturers.Where(l => l.Name.ToLower().Contains(request.Name.ToLower())).ToList();

            }
            else if (request.Email != null)
            {
                lecturers = lecturers.Where(l => l.Email.ToLower().Contains(request.Email.ToLower())).ToList();

            }


            return lecturers;
        }

        public async Task<GetLecturerDetailResponse> GetLecturerDetail(Guid lecturerId)
        {
            var lecturerRaw = await _userManager.FindByIdAsync(lecturerId.ToString());
            var classes = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.LecturerId.Equals(lecturerId)).Select(c => new ClassInfoLecDetail
            {
                ClassId = c.Id,
                Name = c.Name,
                ShortName = c.ShortName
            }).ToListAsync();

            return new GetLecturerDetailResponse
            {
                Id = lecturerRaw.Id,
                Name = lecturerRaw.FullName,
                Email = lecturerRaw.Email,
                Phone = lecturerRaw.PhoneNumber,
                Classes = classes
            };
        }



        public async Task<IEnumerable<GetAllStudentResponse>> GetAllStudent(GetAllStudentRequest request)
        {
            List<GetAllStudentResponse> students = new List<GetAllStudentResponse>();
            var studentsRaw = await _userManager.GetUsersInRoleAsync(UserRole.Student.ToString());
            var leader = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(l => l.Id);
            var now = DateTime.Now;
            var curSemester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.EndDate > now && x.StartDate < now);
            var project = new Project();

            foreach (var s in studentsRaw)
            {
                var stu = new GetAllStudentResponse();
                stu.Id = s.Id;
                stu.Name = s.FullName;
                stu.Email = s.Email;
                stu.StudentId = s.UserName;
                stu.Phone = s.PhoneNumber;

                var studyIn = await GetStudyIn(s.Id, curSemester, leader);
                stu.Role = studyIn.Role;
                stu.ProjectId = studyIn.ProjectId;
                stu.Project = studyIn.Project;
                stu.ClassId = studyIn.ClassId;
                stu.Class = studyIn.Class;

                students.Add(stu);
            }

            return students;
        }

        public async Task<GetStudentDetailResponse> GetStudentDetail(Guid studentId)
        {
            GetStudentDetailResponse student = new GetStudentDetailResponse();
            if (studentId == Guid.Empty || studentId == null)
            {
                return student;
            }

            var stuRaw = await _userManager.FindByIdAsync(studentId.ToString());
            if (stuRaw == null)
            {
                return student;
            }

            student.Id = stuRaw.Id;
            student.Name = stuRaw.FullName;
            student.Email = stuRaw.Email;
            student.StudentId = stuRaw.UserName;
            student.Phone = stuRaw.PhoneNumber;

            var studyIn = await GetStudyIn(stuRaw.Id);
            student.Role = studyIn.Role;
            student.ProjectId = studyIn.ProjectId;
            student.Project = studyIn.Project;
            student.ClassId = studyIn.ClassId;
            student.Class = studyIn.Class;

            return student;
        }
        private async Task<GetStudyInResponse> GetStudyIn(Guid stuId)
        {
            var now = DateTime.Now;
            IEnumerable<Guid> leader = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(l => l.Id);

            var curSemester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.EndDate > now && x.StartDate < now);

            return await GetStudyIn(stuId, curSemester, leader);
        }

        private async Task<GetStudyInResponse> GetStudyIn(Guid stuId, Semester curSemester, IEnumerable<Guid> leader)
        {
            GetStudyInResponse student = new GetStudyInResponse();

            if (curSemester != null)
            {
                var @class = await _commonService.GetCurrentClass(stuId);
                if (@class != null)
                {
                    student.ClassId = @class.Id;
                    student.Class = @class.Name;
                    student.Role = leader.FirstOrDefault(l => l.Equals(stuId)) == Guid.Empty ? UserRole.Student.ToString() : UserRole.Leader.ToString();
                    var project = await _commonService.GetProject(stuId, @class.Id);
                    if (project != null)
                    {
                        student.ProjectId = project.Id;
                        student.Project = project.GroupNum.ToString();
                    }
                    else
                    {
                        student.Project = "";
                    }
                }
                else
                {
                    student.Class = "";
                    student.Project = "";
                    student.Role = "";
                }

            }
            else
            {
                student.Role = "";
                student.Class = "";
                student.Project = "";
            }

            return student;
        }

        public async Task<IEnumerable<GetReportListResponse>> GetReportList(GetReportListRequest request)
        {
            var reportRaw = await _unitOfWork.ReportRepository.Get().Include(r => r.Reporter).Include(r => r.ReportType).ToListAsync();

            return reportRaw.Select(r => new GetReportListResponse
            {
                Id = r.Id,
                Title = r.Title,
                Email = r.Reporter.Email,
                TypeId = r.ReportTypeId,
                Type = r.ReportType.Name,
                Date = r.CreatedAt,
                Status = r.Status,
                Content = r.Content,
                ResponseContent = r.ResponseContent == null ? string.Empty : r.ResponseContent
            });
        }

        public async Task<GetReportDetailResponse> GetReportDetail(Guid? reportId)
        {
            if (reportId == null || reportId == Guid.Empty)
            {
                return new GetReportDetailResponse();
            }

            var reportRaw = await _unitOfWork.ReportRepository.Get().Where(r => r.Id.Equals(reportId)).Include(r => r.Reporter).Include(r => r.ReportType).FirstOrDefaultAsync();

            if (reportRaw == null)
            {
                return new GetReportDetailResponse();
            }
            return new GetReportDetailResponse
            {
                Id = reportRaw.Id,
                Title = reportRaw.Title,
                Email = reportRaw.Reporter.Email,
                TypeId = reportRaw.ReportTypeId,
                Type = reportRaw.ReportType.Name,
                Date = reportRaw.CreatedAt,
                Status = reportRaw.Status,
                Content = reportRaw.Content,
                ResponseContent = reportRaw.ResponseContent == null ? "" : reportRaw.ResponseContent,
                ReportFile = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Report, reportRaw.Id, reportRaw.FileName)) ?? string.Empty,
            };

        }

        public async Task ResponseReport(ResponseReportRequest request)
        {
            if (request.Id == null)
            {
                throw new DataNotFoundException("Report not found");
            }

            if (request.ResponseContent == null)
            {
                throw new BaseBadRequestException("Please set Response");

            }

            request.ResponseContent = request.ResponseContent.Trim();

            if (request.ResponseContent == string.Empty)
            {
                throw new BaseBadRequestException("Please set Response");

            }
            var report = await _unitOfWork.ReportRepository.Get().FirstOrDefaultAsync(r => r.Id.Equals(request.Id));
            if (report == null)
            {
                throw new DataNotFoundException("Report not found");
            }
            report.ResponseContent = request.ResponseContent;
            report.Status = request.Status;

            _unitOfWork.ReportRepository.Update(report);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<GetAssessmentDetailResponse> GetAssessmentDetail(Guid? assessmentId)
        {
            if (assessmentId == null || assessmentId == Guid.Empty)
            {
                return new GetAssessmentDetailResponse();
            }
            var assessmentRaw = await _unitOfWork.AssessmentRepository.Get().Where(a => a.Id.Equals(assessmentId)).Include(a => a.Syllabus).FirstOrDefaultAsync();
            if (assessmentRaw == null)
            {
                return new GetAssessmentDetailResponse();
            }

            return new GetAssessmentDetailResponse
            {
                Id = assessmentRaw.Id,
                Name = assessmentRaw.Name,
                Description = assessmentRaw.Description,
                Order = assessmentRaw.Order,
                Percentage = assessmentRaw.Percentage,
                SyllabusId = assessmentRaw.SyllabusId,
                SyllabusName = assessmentRaw.Syllabus.Name
            };

        }

        public async Task<IEnumerable<GetAllSyllabusResponse>> GetAllSyllabus(GetAllSyllabusRequest request)
        {
            var syllabusRaw = await _unitOfWork.SyllabusRepository.Get().ToListAsync();
            return syllabusRaw.Select(s => new GetAllSyllabusResponse
            {
                Id = s.Id,
                Name = s.Name,
                ShortName = s.ShortName,
                Description = s.Description
            });

        }

        public async Task<GetSyllabusDetailResponse> GetSyllabusDetail(Guid? syllabusId)
        {
            if (syllabusId == null || syllabusId == Guid.Empty)
            {
                return new GetSyllabusDetailResponse();
            }
            var syllabusRaw = await _unitOfWork.SyllabusRepository.Get().Where(s => s.Id.Equals(syllabusId))
                        .Include(s => s.Assessments)
                        .Include(s => s.Semesters)
                        .FirstOrDefaultAsync();

            if (syllabusRaw == null)
            {
                return new GetSyllabusDetailResponse();

            }
            return new GetSyllabusDetailResponse
            {
                Id = syllabusRaw.Id,
                Name = syllabusRaw.Name,
                ShortName = syllabusRaw.ShortName,
                Description = syllabusRaw.Description,
                AssessmentInfos = syllabusRaw.Assessments.OrderBy(x=>x.Order).Select(a => new SysAssessmentInfo
                {
                    AssessmentId = a.Id,
                    Name = a.Name,
                    Order = a.Order,
                    Percentage = a.Percentage,
                    Description = a.Description

                }).ToList(),
                SemesterInfos = syllabusRaw.Semesters.OrderByDescending(x=>x.StartDate).Select(s => new SysSemesterInfo
                {
                    SemesterId = s.Id,
                    Name = s.Name
                }).ToList()
            };
        }

        public async Task<ValidationResultModel> UpdateSyllabusValidators(UpdateSyllabusRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"

            };
            var syllabus = await _unitOfWork.SyllabusRepository.Get().Where(s => s.Id.Equals(request.Id)).Include(s => s.Semesters).FirstOrDefaultAsync();
            if (syllabus == null)
            {
                result.Message = "Syllabus cannot found";
                return result;
            }
            if (syllabus.Semesters.Count() > 0)
            {
                result.Message = "Cannot update Syllabus used";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task UpdateSyllabus(UpdateSyllabusRequest request)
        {
            var syllabus = await _unitOfWork.SyllabusRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(request.Id));
            syllabus.Name = request.Name;
            syllabus.ShortName = request.ShortName;
            syllabus.Description = request.Description;
            _unitOfWork.SyllabusRepository.Update(syllabus);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task CloneSyllabus(CloneSyllabusRequest request)
        {
            if (request.ShortName.Trim() == "")
            {
                throw new BaseBadRequestException("Short Name cannot be empty");

            }
            if (request.Name.Trim() == "")
            {
                throw new BaseBadRequestException("Name cannot be empty");

            }
         
            var isShortNameExisted = await _unitOfWork.SyllabusRepository.Get().FirstOrDefaultAsync(s => s.ShortName.Equals(request.ShortName));
            if (isShortNameExisted != null)
            {
                throw new BaseBadRequestException("Short Name is duplicate");
            }

            var newSyllabus = new Syllabus
            {
                Name = request.Name,
                ShortName = request.ShortName,
                Description = "",
            };
            await _unitOfWork.SyllabusRepository.InsertAsync(newSyllabus);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<GetAllSemesterAdminResponse>> GetAllSemesterAdmin(GetAllSemesterAdminRequest request)
        {
            var semesterRaw = await _unitOfWork.SemesterRepository.Get().Include(s => s.Syllabus).ToListAsync();
            return semesterRaw.Select(s => new GetAllSemesterAdminResponse
            {
                Id = s.Id,
                Name = s.Name,
                ShortName = s.ShortName,
                Description = s.Description,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                SyllabusId = s.SyllabusId,
                SyllabusName = s.Syllabus.Name
            });
        }

        public async Task<GetSemesterDetailResponse> GetSemesterDetail(Guid? semesterId)
        {
            if (semesterId == null || semesterId == Guid.Empty)
            {
                return new GetSemesterDetailResponse();
            }
            var semesterRaw = await _unitOfWork.SemesterRepository.Get().Include(s => s.Syllabus).FirstOrDefaultAsync();

            if (semesterRaw == null)
            {
                return new GetSemesterDetailResponse();
            }

            return new GetSemesterDetailResponse
            {
                Id = semesterRaw.Id,
                Name = semesterRaw.Name,
                ShortName = semesterRaw.ShortName,
                Description = semesterRaw.Description,
                StartDate = semesterRaw.StartDate,
                EndDate = semesterRaw.EndDate,
                SyllabusId = semesterRaw.SyllabusId,
                SyllabusName = semesterRaw.Syllabus.Name
            };
        }
        public async Task<ValidationResultModel> CreateSemesterValidators(CreateSemesterRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            if (request.StartDate >= request.EndDate)
            {
                result.Message = "Please set End Date greater than Start Date";
                return result;
            }

            var lastedSes = await _unitOfWork.SemesterRepository.Get().OrderByDescending(s => s.EndDate).FirstOrDefaultAsync();
            if (lastedSes != null)
            {
                if (lastedSes.EndDate >= request.StartDate)
                {
                    result.Message = "Please set Start Date is greater than lasted Semester end date";
                    return result;
                }
            }
            var syllabus = await _unitOfWork.SyllabusRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(request.SyllabusId));
            if (syllabus == null)
            {
                result.Message = "Syllabus is not existed";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task CreateSemester(CreateSemesterRequest request)
        {
            await _unitOfWork.SemesterRepository.InsertAsync(new Semester
            {
                Name = request.Name,
                ShortName = request.ShortName,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                SyllabusId = request.SyllabusId
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ValidationResultModel> UpdateSemesterValidators(UpdateSemesterRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            if (request.StartDate >= request.EndDate)
            {
                result.Message = "Please set End Date greater than Start Date";
                return result;
            }

            var isExisted = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(request.Id));
            if (isExisted == null)
            {
                result.Message = "Semester cannot found";
                return result;
            }
            if (isExisted.StartDate <= DateTime.Now)
            {
                result.Message = "Cannot update semester started";
                return result;
            }
            var lastedSes = await _unitOfWork.SemesterRepository.Get().Where(s => s.Id.Equals(request.Id) == false).OrderByDescending(s => s.EndDate).FirstOrDefaultAsync();
            if (lastedSes != null)
            {
                if (lastedSes.EndDate >= request.StartDate)
                {
                    result.Message = "Please set Start Date is greater than lasted Semester end date";
                    return result;
                }
            }

            var syllabus = await _unitOfWork.SyllabusRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(request.SyllabusId));
            if (syllabus == null)
            {
                result.Message = "Syllabus is not existed";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task UpdateSemester(UpdateSemesterRequest request)
        {
            var semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(request.Id));
            semester.Name = request.Name;
            semester.ShortName = request.ShortName;
            semester.Description = request.Description;
            semester.StartDate = request.StartDate;
            semester.EndDate = request.EndDate;
            semester.SyllabusId = request.SyllabusId;
            _unitOfWork.SemesterRepository.Update(semester);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteSemester(Guid? semesterId)
        {
            if (semesterId == null || semesterId == Guid.Empty)
            {
                throw new DataNotFoundException("Semester not found");
            }
            var semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(s => s.Id.Equals(semesterId));

            if (semester == null)
            {
                throw new DataNotFoundException("Semester not found");
            }

            if (semester.StartDate <= DateTime.Now)
            {
                throw new BaseBadRequestException("Cannot update semester started");
            }

            _unitOfWork.SemesterRepository.Delete(semester);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
