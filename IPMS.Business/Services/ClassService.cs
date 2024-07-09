using AutoFilterer.Extensions;
using Ganss.Excel;
using Ganss.Excel.Exceptions;
using Hangfire;
using IPMS.Business.Common.Constants;
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
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public ClassService(IUnitOfWork unitOfWork, UserManager<IPMSUser> userManager, IBackgoundJobService backgoundJobService, IPresignedUrlService presignedUrlService, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _backgoundJobService = backgoundJobService;
            _presignedUrlService = presignedUrlService;
            _contextAccessor = contextAccessor;
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
            if (!await _unitOfWork.IPMSClassRepository.Get().AnyAsync(x => x.Id == request.ClassId && x.LecturerId == lecturerId))
            {
                result.Message = "Class is not exist or belong to another lecturer";
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
                MemberInfo = memberInfos.Select(x => new MemberInGroupData
                {
                    Id = x.Id,
                    GroupName = x.Students.First().ProjectId != null ? $"{x.Students.First().Project.GroupNum}" : NoGroup.Name,
                    StudentId = x.UserName,
                    StudentName = x.FullName
                })
            };
        }
        public async Task AddStudentAsync(AddStudentsToClassRequest request)
        {
            var importFileUrl = _presignedUrlService.GeneratePresignedDownloadUrl(request.FileName);
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
                var existStudentInClass = await _unitOfWork.StudentRepository.Get().Include(x=>x.Information).Where(x => x.ClassId == classId && students.Select(x=>x.StudentId).Contains(x.Information.UserName)).ToListAsync();
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
                    var jobId = BackgroundJob.Enqueue<IBackgoundJobService>(importService => importService.ProcessAddStudentToClass(student, classId, _contextAccessor.HttpContext.Request.Host.Value));
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
    }
}
