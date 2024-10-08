﻿using AutoFilterer.Extensions;
using AutoMapper;
using ClosedXML.Excel;
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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Linq;
using System.Data;
using System.Globalization;

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
        private readonly IProjectSubmissionService _projectSubmissionService;
        private readonly IMapper _mapper;
        private readonly ILogger<ClassService> _logger;

        public ClassService(IUnitOfWork unitOfWork,
            UserManager<IPMSUser> userManager,
            IBackgoundJobService backgoundJobService,
            IPresignedUrlService presignedUrlService,
            IHttpContextAccessor contextAccessor,
            ICommonServices commonServices,
            IStudentGroupService studentGroupService,
            IProjectSubmissionService projectSubmissionService,
            IMapper mapper,
            ILogger<ClassService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _backgoundJobService = backgoundJobService;
            _presignedUrlService = presignedUrlService;
            _contextAccessor = contextAccessor;
            _commonServices = commonServices;
            _studentGroupService = studentGroupService;
            _projectSubmissionService = projectSubmissionService;
            _mapper = mapper;
            _logger = logger;
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
            var isGreaterThanStudentInClass = await _unitOfWork.StudentRepository.Get()
                .Where(x => request.ClassIds.Contains(x.ClassId)).GroupBy(x => x.ClassId).AnyAsync(x => request.MaxMember > x.Count());
            if (isGreaterThanStudentInClass)
            {
                result.Message = "Reach number of student of class";
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
                    StudentName = x.FullName,
                    Email = x.Email,
                    StudentClassId = x.Students.First().Id
                }),
                ChangeMemberDeadline = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester)
                                                                        .Where(x => x.Id == request.ClassId)
                                                                        .Select(x => x.Semester.StartDate).FirstOrDefaultAsync()
            };
        }
        public async Task AddStudentAsync(AddStudentsToClassRequest request)
        {
            var importFileUrl = _presignedUrlService.GeneratePresignedDownloadUrl(request.FileName); // FE add Prefix
            if (importFileUrl == null)
            {
                throw new DataNotFoundException();
            }
            //await ProcessImportStudentsAndClassesAsync(importFileUrl, request);
            Thread.Sleep(1000);
        }

        public async Task<JobImportStatusResponse<JobImportStudentStatusRecord>?> GetImportStudentStatusAsync(string classCode)

        {
            //var importJobIds = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == classId && x.JobImportId != null).Select(x => new { x.JobImportId, x.Information.FullName, x.Information.Email }).ToListAsync();
            var jobConnection = JobStorage.Current.GetConnection();
            var jobMonitoringApi = JobStorage.Current.GetMonitoringApi();
            var queueName = "import_student";
            var enqueuedJobs = jobMonitoringApi.EnqueuedJobs(queueName, 0, int.MaxValue).Where(x => x.Value.Job.Args.Count() == 3 && x.Value.Job.Args[2].ToString() == classCode).ToList();
            var fetchedJobs = jobMonitoringApi.FetchedJobs(queueName, 0, int.MaxValue).Where(x => x.Value.Job.Args.Count() == 3 && x.Value.Job.Args[2].ToString() == classCode).ToList();
            var succeededJobs = jobMonitoringApi.SucceededJobs(0, int.MaxValue).Where(x => x.Value.Job.Args.Count() == 3 && x.Value.Job.Args[2].ToString() == classCode).ToList();
            if ((enqueuedJobs == null || !enqueuedJobs.Any()) && (fetchedJobs == null || !fetchedJobs.Any()) && (succeededJobs == null || !succeededJobs.Any()))
            {
                return null;
            }
            var states = new List<JobImportStudentStatusRecord>();
            //var processingStatus = "Processing";
            if (enqueuedJobs != null)
            {
                foreach (var job in enqueuedJobs)
                {
                    states.Add(new JobImportStudentStatusRecord()
                    {
                        JobStatus = jobConnection.GetStateData(job.Key)!.Name,
                        StudentId = (job.Value.Job.Args[0] as StudentDataRow).Email
                    });
                }
            }
            if (fetchedJobs != null)
            {
                foreach (var job in fetchedJobs)
                {
                    states.Add(new JobImportStudentStatusRecord()
                    {
                        JobStatus = jobConnection.GetStateData(job.Key)!.Name,
                        StudentId = (job.Value.Job.Args[0] as StudentDataRow).Email
                    });
                }
            }
            if (succeededJobs != null)
            {
                foreach (var job in succeededJobs)
                {
                    states.Add(new JobImportStudentStatusRecord()
                    {
                        JobStatus = jobConnection.GetStateData(job.Key)!.Name,
                        StudentId = (job.Value.Job.Args[0] as StudentDataRow).Email
                    });
                }
            }
            return states.Any() ? new JobImportStatusResponse<JobImportStudentStatusRecord>
            {
                IsDone = !states.Any(x => x.JobStatus != "Succeeded"),
                States = states
            } : null;
        }
        private async Task ProcessImportStudentsAndClassesAsync(string importFileUrl, ImportClassRequest request)
        {
            var httpClient = new HttpClient();

            var httpResult = await httpClient.GetAsync(importFileUrl);
            var tempFileFullName = Path.Combine(Path.GetTempPath(), request.FileName);
            using (var fileStream = File.Create(tempFileFullName))
            {
                using var resultStream = await httpResult.Content.ReadAsStreamAsync();
                await resultStream.CopyToAsync(fileStream);
            }
            BackgroundJob.Enqueue<IBackgoundJobService>(importService => importService.ProcessAddAllClassInfoToSemester(request.SemesterId, tempFileFullName));
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
                x.ClassId == request.ClassId);
            if (student == null)
            {
                result.Message = "Student does not exist or in different class";
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
            var classRaw = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(classId)).Include(c => c.Students).Include(c => c.Semester).Include(c => c.Committees).ThenInclude(c => c.Lecturer).FirstOrDefaultAsync();
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
            @class.SemesterId = classRaw.SemesterId;
            @class.Semester = classRaw.Semester.Name;
            @class.ShortName = classRaw.ShortName;
            @class.LecturerId = classRaw.LecturerId;
            @class.Lecturer = lecturer.FullName;
            @class.Email = lecturer.Email;
            @class.NumOfStudents = classRaw.Students.Count();
            @class.Committees = classRaw.Committees.Select(c => new CommitteeResponse
            {
                CommitteeId = c.LecturerId,
                Name = c.Lecturer.FullName,
                Percentage = c.Percentage
            }).ToList();
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
            var allLecturers = (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).Select(x => x.Id).ToList(); // Find lecturers

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

            if (await IsClassCodeExistInSemesterAsync(@class.Id, @class.ShortName, request.SemesterId))
            {
                result.Message = "Class Code " + @class.ShortName + " is existed";
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

        public async Task AddClassesAsync(ImportClassRequest request)
        {
            var importFileUrl = _presignedUrlService.GeneratePresignedDownloadUrl(request.FileName); // FE add Prefix
            if (importFileUrl == null)
            {
                throw new DataNotFoundException();
            }
            await ProcessImportStudentsAndClassesAsync(importFileUrl, request);
        }

        private async Task ProcessImportClassesAsync(string importFileUrl, Guid semesterId)
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
                //var excelMapper = new ExcelMapper(tempFile);
                //var classes = excelMapper.Fetch<ClassDataRow>().ToList();
                //var validationResults = new List<ValidationResult>();
                //foreach (var @class in classes)
                //{
                //    validationResults.Clear();
                //    var validationContext = new ValidationContext(@class);
                //    bool isValid = Validator.TryValidateObject(@class, validationContext, validationResults, true);

                //    if (!isValid)
                //    {
                //        throw new ExcelMapperConvertException("File format is not valid!");
                //    }

                //    //Continue validate classCode exist, lecturerId exist
                //    if (await IsClassCodeExistInSemesterAsync(@class.ClassCode, semesterId))
                //    {
                //        throw new BaseBadRequestException($"Class Code {@class.ClassCode} is existed");
                //    }
                //    var lecturer = await _userManager.FindByEmailAsync(@class.LecturerEmail);
                //    if (lecturer == null || !await _userManager.IsInRoleAsync(lecturer, UserRole.Lecturer.ToString()))
                //    {
                //        throw new BaseBadRequestException("Lecturer is not found");
                //    }
                //    //Create student account
                //    var jobId = BackgroundJob.Enqueue<IBackgoundJobService>(importService => importService.ProcessAddClassToSemester(@class, semesterId));
                //}
            }
            catch (Exception ex)
            {
                //if (ex is ExcelMapperConvertException || ex is NPOI.OpenXml4Net.Exceptions.InvalidFormatException)
                //{
                //    throw new BaseBadRequestException("Import file is not exist or cannot map to class data", ex);
                //}
                //else throw;
            }
        }
        public async Task UpdateClassDetail(UpdateClassDetailRequest request)
        {
            var @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(request.Id)).Include(c => c.Committees).FirstOrDefaultAsync();

            @class.LecturerId = request.LecturerId;
            @class.ShortName = request.ShortName;
            @class.SemesterId = request.SemesterId;

            _unitOfWork.CommitteeRepository.DeleteRange(@class.Committees);

            await _unitOfWork.CommitteeRepository.InsertRangeAsync(request.Committees.Select(c => new Committee
            {
                LecturerId = c.Id,
                Percentage = c.Percentage,
                ClassId = request.Id
            }));

            _unitOfWork.IPMSClassRepository.Update(@class);

            await _unitOfWork.SaveChangesAsync();
            #region Data
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
            #endregion
        }

        public async Task<bool> IsClassCodeExistInSemesterAsync(string classCode, Guid semesterId)
        {
            return await _unitOfWork.IPMSClassRepository.Get().AnyAsync(x => x.ShortName == classCode && x.SemesterId == semesterId);
        }
        public async Task<bool> IsClassCodeExistInSemesterAsync(Guid classId, string classCode, Guid semesterId)
        {
            return await _unitOfWork.IPMSClassRepository.Get().AnyAsync(x => x.ShortName == classCode && x.SemesterId == semesterId && x.Id.Equals(classId) == false);
        }

        public async Task<IEnumerable<GetClassDetailResponse>> GetClassList(GetClassListRequest request)
        {
            IEnumerable<GetClassDetailResponse> classes = new List<GetClassDetailResponse>();
            if (request.Name == null)
            {
                request.Name = "";
            }

            var allLecturers = (await _userManager.GetUsersInRoleAsync(UserRole.Lecturer.ToString())).ToList(); // Find lecturers
            List<IPMSClass> classRaw = new List<IPMSClass>();
            if (request.SemesterId != Guid.Empty)
            {
                classRaw = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.SemesterId.Equals(request.SemesterId)).Include(c => c.Students).Include(c => c.Semester).ToListAsync();
            }
            else
            {
                classRaw = await _unitOfWork.IPMSClassRepository.Get().Include(c => c.Semester).Include(c => c.Students).ToListAsync();

            }
            if (request.Id != Guid.Empty)
            {
                classRaw = classRaw.Where(c => c.Name.ToLower().Contains(request.Name.ToLower()) || c.Id.Equals(request.Id)).ToList(); ;
            }
            else
            {
                classRaw = classRaw.Where(c => c.Name.ToLower().Contains(request.Name.ToLower())).ToList();

            }

            classes = classRaw.OrderByDescending(x => x.Semester.StartDate).ThenBy(x => x.ShortName).Select(c => new GetClassDetailResponse
            {
                Id = c.Id,
                ShortName = c.ShortName,
                Email = allLecturers.FirstOrDefault(l => l.Id.Equals(c.LecturerId)) == null ? "None" : allLecturers.FirstOrDefault(l => l.Id.Equals(c.LecturerId))!.Email,
                LecturerId = c.LecturerId,
                Lecturer = allLecturers.FirstOrDefault(l => l.Id.Equals(c.LecturerId)) == null ? "None" : allLecturers.FirstOrDefault(l => l.Id.Equals(c.LecturerId))!.FullName,
                SemesterId = c.SemesterId,
                Semester = c.Semester.Name,
                NumOfStudents = c.Students.Count(),
            }).ToList();

            return classes;
        }

        public async Task<JobImportStatusResponse<JobImportClassStatusRecord>?> GetImportClassStatusAsync(Guid semesterId)
        {
            var jobConnection = JobStorage.Current.GetConnection();
            // Get Newest Job of semester
            var jobList = jobConnection.GetAllEntriesFromHash(semesterId.ToString())?.OrderByDescending(x => DateTime.ParseExact(x.Value, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)).ToList();

            if (jobList == null || !jobList.Any())
            {
                return null;
            }
            var response = new JobImportStatusResponse<JobImportClassStatusRecord>();
            var newestJobId = jobList.First().Key;

            var importClassData = jobConnection.GetAllEntriesFromHash(newestJobId);
            if (importClassData == null)
            {
                return response;
            }
            foreach (var classState in importClassData.Where(x => x.Key != ImportJob.NumberOfClassesKey))
            {
                var importClassStatus = new JobImportClassStatusRecord
                {
                    ClassCode = classState.Key,
                    StudentStatus = new JobImportStatusResponse<JobImportStudentStatusRecord>()
                };
                if (classState.Value != ImportJob.DoneStatus && classState.Value != ImportJob.ProcessingStatus)
                {
                    importClassStatus.Error = classState.Value;
                    importClassStatus.StudentStatus = null;
                }
                else
                {
                    var importStudentData = jobConnection.GetAllEntriesFromHash(classState.Key);
                    if (importStudentData != null)
                    {
                        foreach (var stuState in importStudentData.Where(x => x.Key != ImportJob.NumberOfStudentsKey))
                        {
                            var status = new JobImportStudentStatusRecord
                            {
                                StudentId = stuState.Key,
                                JobStatus = ImportJob.SucceededStatus
                            };
                            if (stuState.Value != ImportJob.SucceededStatus)
                            {
                                status.JobStatus = ImportJob.FailedStatus;
                                status.Error = stuState.Value;
                            }
                            await Task.Run(() => importClassStatus.StudentStatus.States.Add(status));
                        }
                        importClassStatus.StudentStatus.IsDone = int.Parse(importStudentData[ImportJob.NumberOfStudentsKey]) == importClassStatus.StudentStatus.States.Count();
                    }
                }
                await Task.Run(() => response.States.Add(importClassStatus));
            }
            response.IsDone = int.Parse(importClassData[ImportJob.NumberOfClassesKey]) == response.States.Count();
            if (response.IsDone)
            {
                var jobData = jobConnection.GetJobData(newestJobId);
                var filePath = jobData?.Job.Args[1].ToString();
                try
                {
                    if(filePath != null)
                    {
                        FileUtils.DeleteFile(filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Cannot delete file {filePath}", filePath);
                }
            }
            return response;
        }

        public async Task<ValidationResultModel> CheckImportClassValidAsync(ImportClassRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Cannot Import Class"
            };
            var semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.Id == request.SemesterId);
            if (semester == null)
            {
                result.Message = "Not found Semester";
                return result;
            }
            if (semester.StartDate <= DateTime.Now)
            {
                result.Message = "Cannot add class at this time";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task<ValidationResultModel> UpdateClassDeadlineValidators(UpdateClassDeadlineRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"

            };
            var @class = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId)).Include(c => c.Semester).FirstOrDefaultAsync();
            if (@class == null)
            {
                result.Message = "Class cannot found";
                return result;
            }
            if (@class.Semester == null)
            {
                result.Message = "Class not set semester";
                return result;
            }

            var endSemester = @class.Semester.EndDate;
            if (request.CreateGroup > endSemester || request.ChangeGroup > endSemester) // semester is continuous
            {
                result.Message = "Deadline must before end of semester";
                return result;
            }

            var startSemester = @class.Semester.StartDate;
            if (request.CreateGroup < startSemester || request.ChangeGroup < startSemester) // semester is continuous
            {
                result.Message = "Deadline must after start of semester";
                return result;
            }

            if (request.ChangeTopic.HasValue && request.ChangeTopic.Value > endSemester)
            {
                result.Message = "Deadline must before end of semester";
                return result;
            }

            if (request.BorrowIot.HasValue && request.BorrowIot.Value > endSemester)
            {
                result.Message = "Deadline must before end of semester";
                return result;
            }

            if (request.ChangeTopic.HasValue && request.ChangeTopic.Value < startSemester)
            {
                result.Message = "Deadline must after start of semester";
                return result;
            }

            if (request.BorrowIot.HasValue && request.BorrowIot.Value < startSemester)
            {
                result.Message = "Deadline must after start of semester";
                return result;
            }
            // create group -> change topic -> change group , borrow 2 cái này cùng cấp, không cần so sánh
            if (request.ChangeTopic.HasValue && request.CreateGroup > request.ChangeTopic)
            {
                result.Message = "Please set Create Group deadline before Change Topic Deadline";
                return result;
            }

            if (request.ChangeTopic.HasValue && request.ChangeTopic > request.ChangeGroup)
            {
                result.Message = "Please set Change Topic deadline before Change Group Deadline";
                return result;
            }

            if (request.ChangeTopic.HasValue && request.BorrowIot.HasValue && request.ChangeTopic > request.BorrowIot)
            {
                result.Message = "Please set Change Topic deadline before Borrow Iot Devices Deadline";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task UpdateClassDeadline(UpdateClassDeadlineRequest request, Guid lecturerId)
        {
            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId));
            @class.CreateGroupDeadline = request.CreateGroup;
            @class.ChangeGroupDeadline = request.ChangeGroup;
            @class.ChangeTopicDeadline = request.ChangeTopic;
            @class.BorrowIoTComponentDeadline = request.BorrowIot;
            _unitOfWork.IPMSClassRepository.Update(@class);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<GetClassDeadlineResponse> GetClassDeadline(Guid classId, Guid lecturerId)
        {
            var classDeadline = new GetClassDeadlineResponse();
            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(classId) && c.LecturerId.Equals(lecturerId));
            if (@class == null)
            {
                return classDeadline;
            }

            classDeadline.CreateGroup = @class.CreateGroupDeadline;
            classDeadline.ChangeGroup = @class.ChangeGroupDeadline;
            classDeadline.ChangeTopic = @class.ChangeTopicDeadline;
            classDeadline.BorrowIot = @class.BorrowIoTComponentDeadline;
            return classDeadline;
        }

        public async Task<ClassGradeExportResponse> ExportGradesAsync(ClassExportGradeRequest request)
        {
            var studentGrades = await ProcessClassGrades(request.ClassId);
            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(x => x.Id == request.ClassId);
            var fileName = $"{S3KeyPrefix.ExportGrade}_{@class.Id}_{@class.ShortName}_Grade.xlsx";
            // Save to tempfile
            var filePath = SaveFile(fileName, studentGrades);

            //Get Upload presign

            var uploadUrl = _presignedUrlService.GeneratePresignedUploadUrl(fileName);
            await _presignedUrlService.UploadToS3(filePath, uploadUrl);
            return new ClassGradeExportResponse
            {
                ExportFileUrl = _presignedUrlService.GeneratePresignedDownloadUrl(fileName)!
            };
        }
        private async Task<IList<ClassGradeDataRow>> ProcessClassGrades(Guid classId)
        {
            var studentGrades = await _unitOfWork.StudentRepository.Get().Include(x => x.Project).Include(x => x.Information)
                .Where(x => x.ClassId == classId)
                .OrderBy(x => x.Project.GroupNum).ThenBy(x => x.Information.UserName)
                .Select(x => new ClassGradeDataRow
                {
                    StudentId = x.Information.UserName,
                    StudentName = x.Information.FullName,
                    Group = x.ProjectId != null ? x.Project.GroupNum : null,
                    ProjectTechnicalId = x.ProjectId != null ? x.ProjectId.Value : null,
                    StudentTechnicalId = x.InformationId,
                    Final = x.FinalGrade,
                    Contribute = x.FinalPercentage
                }).ToListAsync();
            if (!studentGrades.Any())
            {
                throw new DataNotFoundException("Not found any student in class");
            }
            var groups = studentGrades.Where(x => x.ProjectTechnicalId != null).Select(x => x.ProjectTechnicalId.Value).Distinct();
            var groupGrades = new Dictionary<Guid, Responses.ProjectSubmission.GetGradeResponse>();
            foreach (var group in groups)
            {
                groupGrades.Add(group, await _projectSubmissionService.GetGradeAsync(
                    studentGrades.Where(x => x.ProjectTechnicalId == group).First().StudentTechnicalId, group)
                    );
            }
            foreach (var student in studentGrades)
            {
                if (student.ProjectTechnicalId != null)
                {
                    _mapper.Map(groupGrades[student.ProjectTechnicalId.Value], student);
                }
            }
            return studentGrades;
        }
        private string SaveFile(string fileName, IEnumerable<ClassGradeDataRow> studentGrades)
        {
            var tempPath = Path.GetTempPath();
            var filePath = Path.Combine(tempPath, fileName);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Class Grades");

                worksheet.Range("A1:A2").Merge().Value = "StudentId";
                worksheet.Range("B1:B2").Merge().Value = "Student Name";
                worksheet.Range("C1:C2").Merge().Value = "Group";
                worksheet.Range("D1:D2").Merge().Value = "Contribute Percentage";

                worksheet.Range("A1:D2").Style.Font.Bold = true;
                worksheet.Range("A1:D2").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range("A1:D2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range("A1:D2").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                int startCol = 5;

                List<string> assessmentTitle = new List<string> { "Group Average", "Student Average" };
                foreach (var assessment in studentGrades.First().AssessmentGrades)
                {
                    int colSpan = assessment.SubmissionGrades.Count;

                    worksheet.Range(1, startCol, 1, startCol + colSpan + assessmentTitle.Count - 1).Merge().Value = assessment.Name;
                    worksheet.Range(1, startCol, 1, startCol + colSpan + assessmentTitle.Count - 1).Style.Font.Bold = true;
                    worksheet.Range(1, startCol, 1, startCol + colSpan + assessmentTitle.Count - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(1, startCol, 1, startCol + colSpan + assessmentTitle.Count - 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                    for (int i = 0; i < colSpan; i++)
                    {
                        worksheet.Cell(2, startCol + i).Value = assessment.SubmissionGrades[i].Name;
                        worksheet.Cell(2, startCol + i).Style.Font.Bold = true;
                        worksheet.Cell(2, startCol + i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(2, startCol + i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    for (int i = 0; i < assessmentTitle.Count; i++)
                    {
                        worksheet.Cell(2, startCol + colSpan + i).Value = assessmentTitle[i];
                        worksheet.Cell(2, startCol + colSpan + i).Style.Font.Bold = true;
                        worksheet.Cell(2, startCol + colSpan + i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(2, startCol + colSpan + i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }
                    startCol += colSpan + assessmentTitle.Count;
                }

                worksheet.Range(1, startCol, 2, startCol).Merge().Value = "Group Grade";
                worksheet.Range(1, startCol + 1, 2, startCol + 1).Merge().Value = "Final Grade";

                // Apply styles
                worksheet.Range(1, startCol, 2, startCol + 2).Style.Font.Bold = true;
                worksheet.Range(1, startCol, 2, startCol + 2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                // Fill in the data
                int row = 3;
                foreach (var dataRow in studentGrades)
                {

                    worksheet.Cell(row, 1).Value = dataRow.StudentId;
                    worksheet.Cell(row, 2).Value = dataRow.StudentName;
                    worksheet.Cell(row, 3).Value = dataRow.Group;
                    worksheet.Cell(row, 4).Value = dataRow.Contribute?.ToString("F2");

                    int col = 5;

                    foreach (var assessment in dataRow.AssessmentGrades)
                    {
                        foreach (var submission in assessment.SubmissionGrades)
                        {
                            worksheet.Cell(row, col).Value = submission.Grade?.ToString("F2");
                            col++;
                        }

                        worksheet.Cell(row, col).Value = assessment.AssessmentAvg?.ToString("F2");
                        col++;

                        worksheet.Cell(row, col).Value = (assessment.AssessmentAvg * dataRow.Contribute / 100 )? .ToString("F2");
                        col++;

                    }

                    worksheet.Cell(row, col).Value = dataRow.Total?.ToString("F2");
                    worksheet.Cell(row, col + 1).Value = dataRow.Final?.ToString("F2");

                    row++;
                }

                // Add borders
                var usedRange = worksheet.RangeUsed();
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;


                worksheet.ColumnsUsed().AdjustToContents();

                workbook.SaveAs(filePath);
            }
            return filePath;
        }

        public async Task<ValidationResultModel> CheckExportGradeValid(ClassExportGradeRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Cannot Export"
            };
            if (!await _unitOfWork.IPMSClassRepository.Get().AnyAsync(x => x.LecturerId == lecturerId && x.Id == request.ClassId))
            {
                result.Message = "Not Found Class for Export";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task<IList<ClassGradeDataRow>> GetClassGrades(ClassExportGradeRequest request)
        {
            return await ProcessClassGrades(request.ClassId);
        }
    }
}
