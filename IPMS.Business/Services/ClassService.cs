using AutoFilterer.Extensions;
using Ganss.Excel;
using Ganss.Excel.Exceptions;
using Hangfire;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Models;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Class;
using IPMS.Business.Responses.Class;
using IPMS.DataAccess.Models;
using MathNet.Numerics.Distributions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using System.ComponentModel.DataAnnotations;

namespace IPMS.Business.Services
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IPMSUser> _userManager;
        private readonly IBackgoundJobService _backgoundJobService;
        private readonly IPresignedUrlService _presignedUrlService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ICommonServices _commonServices;
        private readonly IStudentGroupService _studentGroupService;

        public ClassService(IUnitOfWork unitOfWork,
            UserManager<IPMSUser> userManager,
            IBackgoundJobService backgoundJobService,
            IPresignedUrlService presignedUrlService,
            IHttpContextAccessor contextAccessor,
            ICommonServices commonServices,
            IStudentGroupService studentGroupService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _backgoundJobService = backgoundJobService;
            _presignedUrlService = presignedUrlService;
            _contextAccessor = contextAccessor;
            _commonServices = commonServices;
            _studentGroupService = studentGroupService;
        }
        public async Task<ValidationResultModel> CheckSetMaxMemberRequestValid(Guid lecturerId, SetMaxMemberRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Cannot Set Max Member"
            };
            if (!request.ClassIds.Any())
            {
                result.Message = "Must have at least on class in request";
                return result;
            }
            var now = DateTime.Now;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var minAccepptedStartDate = currentSemester.StartDate.AddHours(-1).Date;
            var requestClasses = await _unitOfWork.IPMSClassRepository.Get().Include(c => c.Semester)
                                                                        .Where
                                                                        (
                                                                            //Check Start Date of class is in Current Semester or in the future
                                                                            c => request.ClassIds.Contains(c.Id) &&
                                                                            c.Semester!.StartDate.Date >= minAccepptedStartDate &&
                                                                            //Check Change Group Deadline still in the future
                                                                            c.ChangeGroupDeadline > now &&
                                                                            //Check lecturer is teaching all class in request or not
                                                                            c.LecturerId == lecturerId
                                                                        )
                                                                        .CountAsync();

            if (requestClasses < request.ClassIds.Count())
            {
                result.Message = "Request contains class cannot modify max member or class is not existed";
                return result;
            }
            //Check all existing group have number of member less than request
            var isExistGreaterGroup = await _unitOfWork.StudentRepository.Get().Where
                                                                                (
                                                                                    stu => request.ClassIds.Contains(stu.ClassId) &&
                                                                                    stu.ProjectId != null
                                                                                )
                                                                                .GroupBy(x => new { x.ProjectId, x.ClassId })
                                                                                .AnyAsync
                                                                                (
                                                                                    group => group.Count() > request.MaxMember
                                                                                );
            if (isExistGreaterGroup)
            {
                result.Message = "At least one group have more member that max member you want";
                return result;
            }
            result.Result = true;
            result.Message = string.Empty;
            return result;
        }
        public async Task<ValidationResultModel> CheckImportStudentValidAsync(AddStudentsToClassRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel()
            {
                Message = "Cannot import students to class"
            };
            var @class = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester).Where(x => x.Id == request.ClassId && x.LecturerId == lecturerId).FirstOrDefaultAsync();
            if (@class == null)
            {
                result.Message = "Class is not exist or belong to another lecturer";
                return result;
            }
            if (@class.Semester.StartDate < DateTime.Now)
            {
                result.Message = "Cannot import student because class started";
                return result;
            }
            if (await _unitOfWork.StudentRepository.Get().AnyAsync(x => x.ClassId == request.ClassId) && !request.IsOverwrite!.Value)
            {
                result.Message = "Exist students in class";
                return result;
            }
            result.Result = true;
            result.Message = string.Empty;
            return result;
        }
        public async Task SetMaxMember(Guid lecturerId, SetMaxMemberRequest request)
        {
            var updateClasses = request.ClassIds.Select(x => new IPMSClass
            {
                Id = x
            });
            foreach (var c in updateClasses)
            {
                _unitOfWork.IPMSClassRepository.Attach(c);
                c.MaxMember = request.MaxMember;
            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<IList<ClassGroupResponse>> GetGroupsInClass(Guid classId)
        {
            var result = await _unitOfWork.ProjectRepository.Get().Include(x => x.Students).Where(x => x.Students.First().ClassId == classId)
                                                                          .Select(x => new ClassGroupResponse
                                                                          {
                                                                              Id = x.Id,
                                                                              Name = $"Group {x.GroupNum}"
                                                                          }).ToListAsync();
            result.Add(new ClassGroupResponse()
            {
                Id = NoGroup.Id,
                Name = NoGroup.Name,
            });
            return result;
        }
        public async Task<MemberInGroupResponse> GetMemberInGroupAsync(MemberInGroupRequest request)
        {
            var memberInfos = _userManager.Users.ApplyFilter(request);
            if (request.GroupFilter != null && request.GroupFilter.Any())
            {
                // If not contain No Group filter
                if (!request.GroupFilter.Remove(NoGroup.Id))
                {
                    memberInfos = memberInfos.Include(x => x.Students.Where(stu => stu.ProjectId != null && request.GroupFilter.Contains(stu.ProjectId.Value))).ThenInclude(x => x.Project);
                    memberInfos = memberInfos.Where(x => x.Students.First().ProjectId != null && request.GroupFilter.Contains(x.Students.First().ProjectId.Value));
                }
                else
                {
                    memberInfos = memberInfos.Include(x => x.Students.Where(stu => stu.ProjectId == null || request.GroupFilter.Contains(stu.ProjectId.Value))).ThenInclude(x => x.Project);
                    memberInfos = memberInfos.Where(x => x.Students.First().ProjectId == null || request.GroupFilter.Contains(x.Students.First().ProjectId.Value));
                }
            }

            return new MemberInGroupResponse()
            {
                TotalMember = await _unitOfWork.StudentRepository.Get().CountAsync(x => x.ClassId == request.Students.ClassId),
                MemberInfo = memberInfos.OrderBy(x => x.Students.First().Project.GroupNum).Select(x => new MemberInGroupData
                {
                    Id = x.Id,
                    GroupName = x.Students.First().ProjectId != null ? $"{x.Students.First().Project.GroupNum}" : NoGroup.Name,
                    StudentId = x.UserName,
                    StudentName = x.FullName
                }),
                ChangeMemberDeadline = await _unitOfWork.IPMSClassRepository.Get().Include(x=>x.Semester)
                                                                        .Where(x => x.Id == request.ClassId)
                                                                        .Select(x=>x.Semester.StartDate).FirstOrDefaultAsync()
            };
        }
        public async Task AddStudentAsync(AddStudentsToClassRequest request)
        {
            var importFileUrl = _presignedUrlService.GeneratePresignedDownloadUrl(request.FileName); // FE add Prefix
            if (importFileUrl == null)
            {
                throw new DataNotFoundException();
            }
            await ProcessImportStudentAsync(importFileUrl, request.ClassId);
        }

        public async Task<JobImportStatusResponse?> GetImportStudentStatusAsync(Guid classId)
        {
            var importJobIds = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == classId).Select(x => new { x.JobImportId, x.Information.FullName, x.Information.Email }).ToListAsync();
            if (importJobIds == null || !importJobIds.Any())
            {
                return null;
            }
            var jobConnection = JobStorage.Current.GetConnection();
            var states = new List<JobImportStatusRecord>();
            var processingStatus = "Processing";
            foreach (var job in importJobIds)
            {
                states.Add(new JobImportStatusRecord()
                {
                    JobStatus = jobConnection.GetStateData(job.JobImportId.ToString())?.Name ?? processingStatus,
                    StudentName = job.FullName,
                    StudentEmail = job.Email
                });
            }
            return states.Any() ? new JobImportStatusResponse
            {
                IsDone = !states.Any(x => x.JobStatus == processingStatus),
                States = states
            } : null;
        }
        private async Task ProcessImportStudentAsync(string importFileUrl, Guid classId)
        {
            var httpClient = new HttpClient();

            var httpResult = await httpClient.GetAsync(importFileUrl);
            var tempFile = Path.GetTempFileName();
            using (var fileStream = File.Create(tempFile))
            {
                using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                await resultStream.CopyToAsync(fileStream);
            }
            try
            {
                var excelMapper = new ExcelMapper(tempFile);
                var students = excelMapper.Fetch<StudentDataRow>().ToList();
                var validationResults = new List<ValidationResult>();
                var classForImport = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(x => x.Id == classId);
                var existStudentInAnotherClass = await _unitOfWork.StudentRepository.Get()
                                                                                .Include(x => x.Information)
                                                                                .Include(x => x.Class)
                                                                                .AnyAsync
                                                                                (
                                                                                    x => students.Select(y => y.StudentId)
                                                                                    .Contains(x.Information.UserName)
                                                                                    && x.Class.SemesterId != classForImport!.SemesterId
                                                                                );
                if (existStudentInAnotherClass) throw new BaseBadRequestException($"Exist student in another class");
                var existStudentInClass = await _unitOfWork.StudentRepository.Get().Include(x => x.Information)
                                                                             .Where(x => x.ClassId == classId)
                                                                             .ToListAsync();
                if (existStudentInClass != null)
                {
                    _unitOfWork.StudentRepository.DeleteRange(existStudentInClass);
                    await _unitOfWork.SaveChangesAsync();
                }
                foreach (var student in students)
                {
                    validationResults.Clear();
                    var validationContext = new ValidationContext(student);
                    bool isValid = Validator.TryValidateObject(student, validationContext, validationResults, true);

                    if (!isValid)
                    {
                        throw new ExcelMapperConvertException("File format is not valid!");
                    }
                    //Create student account
                    var jobId = BackgroundJob.Enqueue<IBackgoundJobService>(importService => importService.ProcessAddStudentToClass(student, classId));
                }
            }
            catch (Exception ex)
            {
                if (ex is ExcelMapperConvertException || ex is NPOI.OpenXml4Net.Exceptions.InvalidFormatException)
                {
                    throw new CannotImportStudentException(ex);
                }
                else throw;
            }
        }

        public async Task<ValidationResultModel> CheckValidRemoveOutOfClass(RemoveOutOfClassRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Cannot Remove"
            };
            //Check student exists -> also class exists
            var student = await _unitOfWork.StudentRepository.Get()
                .FirstOrDefaultAsync(x => x.InformationId == request.StudentId &&
                x.ClassId == request.ClassId &&
                x.Class.LecturerId == lecturerId);
            if (student == null)
            {
                result.Message = "Student doest not exist or in different class";
                return result;
            }
            var @class = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester).FirstAsync(x => x.Id == request.ClassId);
            if (@class.Semester.StartDate <= DateTime.Now)
            {
                result.Message = "Cannot remove at this time";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task<GetClassDetailResponse> GetClassDetail(Guid classId)
        {
            GetClassDetailResponse @class = new GetClassDetailResponse();
            var classRaw = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(classId)).Include(c => c.Students).Include(c => c.Semester).FirstOrDefaultAsync();
            if (classRaw == null)
            {
                return @class;
            }

            var lecturer = await _userManager.FindByIdAsync(classRaw.LecturerId.ToString());
            if (lecturer == null)
            {
                return @class;
            }

            @class.Id = classRaw.Id;
            @class.Semester = classRaw.Semester.Name;
            @class.ShortName = classRaw.ShortName;
            @class.Name = classRaw.Name;
            @class.Lecturer = lecturer.FullName;
            @class.NumOfStudents = classRaw.Students.Count();
            return @class;
        }
        public async Task<ValidationResultModel> UpdateClassDetailValidators(UpdateClassDetailRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"

            };

            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.Id));
            if (@class == null)
            {
                result.Message = "Class cannot found";
                return result;
            }

            if (request.MaxMember < 1)
            {
                result.Message = "Cannot set member lower than 1";
                return result;
            }

            var dupCommitteeId = request.Committees
                    .GroupBy(g => g.Id)
                    .Where(g => g.Count() > 1 && g.Key != Guid.Empty)
                    .Select(g => g.Key)
                    .ToList();
            if (dupCommitteeId.Any())
            {
                result.Message = "Cannot have duplicate committee";
                return result;
            }

            var isLecturerCommit = request.Committees.Where(c => c.Id.Equals(request.LecturerId)).Count();
            if (isLecturerCommit != 1)
            {
                result.Message = "Lecturer must have permission to commit";
                return result;
            }

            decimal percentage = 0;
            var allLecturers = (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).Select(x => x.Id).ToList(); // Find leader of project

            foreach (var committee in request.Committees)
            {
                percentage += committee.Percentage;
                if (committee.Percentage < 1 || committee.Percentage > 100)
                {
                    result.Message = "Percentage must in range 1 to 100";
                    return result;
                }
                var lecCommit = await _userManager.FindByIdAsync(committee.Id.ToString());
                if (lecCommit == null)
                {
                    result.Message = "Lecturer cannot found";
                    return result;
                }

                var isRoleLecturer = allLecturers.Any(al => al.Equals(committee.Id));
                if (isRoleLecturer == false)
                {
                    result.Message = "Committee must be a Lecturer";
                    return result;
                }
            }

            if (percentage != 100)
            {
                result.Message = "Please set sum of percentage is 100";
                return result;
            }

            var now = DateTime.Now;
            var isValidSemester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.StartDate > now && x.Id.Equals(request.SemesterId));
            if (isValidSemester == null)
            {
                result.Message = "Invalid semester";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        

        public async Task RemoveOutOfClassAsync(RemoveOutOfClassRequest request)
        {
            var student = await _unitOfWork.StudentRepository.Get().FirstAsync(x => x.InformationId == request.StudentId && x.ClassId == request.ClassId);
            _unitOfWork.StudentRepository.Delete(student);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task UpdateClassDetail(UpdateClassDetailRequest request)
        {
            var @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(request.Id)).Include(c => c.Committees).FirstOrDefaultAsync();

            @class.LecturerId = request.LecturerId;
            @class.Name = request.Name;
            @class.ShortName = request.ShortName;
            @class.MaxMember = request.MaxMember;
            @class.SemesterId = request.SemesterId;

            _unitOfWork.CommitteeRepository.DeleteRange(@class.Committees);

            await _unitOfWork.CommitteeRepository.InsertRangeAsync(request.Committees.Select(c => new Committee
            {
                LecturerId = c.Id,
                Percentage = c.Percentage,
                ClassId = request.Id
            }));

            _unitOfWork.IPMSClassRepository.Attach(@class);

            await _unitOfWork.SaveChangesAsync();
        }

      /*   
       *   {
  "id": "898657b4-c753-4783-8203-297079af9d82",
  "lecturerId": "cc76a4b5-bc4b-4539-ad02-c74c3fde8d32",
  "name": "string",
  "shortName": "string",
  "maxMember": 10,
  "semesterId": "dd34672a-f484-40f4-937c-01dab32fd770",
  "committees": [
    {
      "id": "cc76a4b5-bc4b-4539-ad02-c74c3fde8d32",
      "percentage": 100
    }
  ]
}
      */
    }
}
