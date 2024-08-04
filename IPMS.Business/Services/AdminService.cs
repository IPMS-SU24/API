﻿using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Models;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Requests.Authentication;
using IPMS.Business.Responses.Admin;
using IPMS.Business.Responses.Authentication;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IPMS.Business.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ICommonServices _commonService;
        private readonly MailServer _mailServer;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly JWTConfig _jwtConfig;
        private readonly string _mailHost;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPresignedUrlService _presignedUrlService;

        public AdminService(UserManager<IPMSUser> userManager,
                                   RoleManager<IdentityRole<Guid>> roleManager,
                                   IOptions<JWTConfig> jwtConfig,
                                   ILogger<AuthenticationService> logger,
                                   ICommonServices commonService,
                                   MailServer mailServer,
                                   IConfiguration configuration,
                                   IUnitOfWork unitOfWork,
                                   IPresignedUrlService presignedUrlService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtConfig = jwtConfig.Value;
            _logger = logger;
            _commonService = commonService;
            _mailServer = mailServer;
            _mailHost = configuration["MailFrom"];
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
                Email = x.Email
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
                ReportFile = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Report, reportRaw.Id, reportRaw.Title)) ?? string.Empty,
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
                AssessmentInfos = syllabusRaw.Assessments.Select(a => new SysAssessmentInfo
                {
                    AssessmentId = a.Id,
                    Name = a.Name,
                    Order = a.Order,
                    Percentage = a.Percentage

                }).ToList(),
                SemesterInfos = syllabusRaw.Semesters.Select(s => new SysSemesterInfo
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

        public async Task CloneSyllabus(Guid? syllabusId)
        {
            if (syllabusId == null || syllabusId == Guid.Empty)
            {
                throw new DataNotFoundException("Syllabus not found");
            }
            var syllabus = await _unitOfWork.SyllabusRepository.Get().Where(s => s.Id.Equals(syllabusId)).Include(s => s.Assessments).FirstOrDefaultAsync();

            if (syllabus == null)
            {
                throw new DataNotFoundException("Syllabus not found");
            }
            var newSyllabus = new Syllabus
            {
                Name = syllabus.Name,
                ShortName = syllabus.ShortName,
                Description = syllabus.Description,
            };

            var assessments = syllabus.Assessments.Select(a => new Assessment
            {
                Name = a.Name,
                Description = a.Description,
                Order = a.Order,
                Percentage = a.Percentage,
                SyllabusId = newSyllabus.Id
            });
            await _unitOfWork.SyllabusRepository.InsertAsync(newSyllabus);
            await _unitOfWork.AssessmentRepository.InsertRangeAsync(assessments);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}