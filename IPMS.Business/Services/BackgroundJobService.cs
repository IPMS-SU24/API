using Azure.Communication.Email;
using Azure;
using Hangfire;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Models;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using IPMS.Business.Common.Models;
using IPMS.Business.Common.Utils;
using IPMS.Business.Common.Exceptions;
using IPMSBackgroundService.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using MathNet.Numerics.Distributions;
using NPOI.Util;
using IPMS.Business.Common.Hangfire;
using IPMS.Business.Common.Enums;
using Microsoft.Extensions.Configuration;
using Ganss.Excel.Exceptions;
using Ganss.Excel;
using System.ComponentModel.DataAnnotations;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Extensions;

namespace IPMS.Business.Services
{
    public class BackgroundJobService : IBackgoundJobService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ILogger<BackgroundJobService> _logger;
        private readonly MailServer _mailServer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private readonly IHttpContextAccessor _httpContext;
        private readonly string _mailHost;

        public BackgroundJobService(UserManager<IPMSUser> userManager,
                                    MailServer mailServer,
                                    IUnitOfWork unitOfWork,
                                    IMessageService messageService,
                                    IHttpContextAccessor httpContext,
                                    IConfiguration configuration,
                                    RoleManager<IdentityRole<Guid>> roleManager,
                                    ILogger<BackgroundJobService> logger)
        {
            _userManager = userManager;
            _mailServer = mailServer;
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _httpContext = httpContext;
            _mailHost = configuration["MailFrom"];
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task AddJobIdToStudent(string jobId,Guid classId, string email)
        {
            await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
            {
                var studentInserted = await _unitOfWork.StudentRepository.Get().Include(x => x.Information).FirstOrDefaultAsync(x => x.ClassId == classId && x.Information.Email == email);
                studentInserted!.JobImportId = int.Parse(jobId);
                _unitOfWork.StudentRepository.Update(studentInserted);
                await _unitOfWork.SaveChangesAsync();
            });
        }

        public async Task ProcessAddClassToSemester(ClassDataRow @class, Guid semesterId)
        {
            await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
            {
                var lecturer = await _userManager.FindByEmailAsync(@class.LecturerEmail);
                //save class
                var newClass = new IPMSClass()
                {
                    LecturerId = lecturer.Id,
                    ShortName = @class.ClassCode,
                    SemesterId = semesterId,
                    Name = string.Empty,
                    JobImportId = int.Parse(JobContext.CurrentJobId)
                };
                await _unitOfWork.IPMSClassRepository.InsertAsync(newClass);
                var committee = new Committee()
                {
                    ClassId = newClass.Id,
                    LecturerId = lecturer.Id,
                    Percentage = 100
                };
                await _unitOfWork.CommitteeRepository.InsertAsync(committee);
                await _unitOfWork.SaveChangesAsync();
                //Send noti to lecturer
                await _messageService.SendMessage(new NotificationMessage()
                {
                    AccountId = lecturer.Id,
                    Message = $"You are added into Class {@class.ClassCode}",
                    Title = "New Class Assigned"
                });
            });
        }

        public async Task ProcessAddStudentToClass(StudentDataRow student, Guid classId, string classCode)
        {
                //Check mail is exist
                var existUser = await _userManager.FindByEmailAsync(student.Email);
                //If not exist => create account
                if (existUser == null)
                {
                    var password = PasswordGeneratorUtils.GenerateRandomPassword();
                    var stuAccount = new IPMSUser
                    {
                        FullName = student.StudentName,
                        UserName = student.StudentId,
                        Email = student.Email,
                        SecurityStamp = Guid.NewGuid().ToString()

                    };
                    var result = await _userManager.CreateAsync(stuAccount, password);
                    if (!result.Succeeded)
                    {
                        throw new CannotCreateAccountException();
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRole.Student.ToString()))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.Student.ToString()));
                    }
                    await _userManager.AddToRoleAsync(stuAccount,UserRole.Student.ToString());
                    existUser = await _userManager.FindByEmailAsync(student.Email);
                    //Send mail confirm
                    try
                    {
                        var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(existUser);
                        var confirmURL = PathUtils.GetConfirmURL(existUser.Id, confirmEmailToken);
                        EmailSendOperation emailSendOperation = await _mailServer.Client.SendAsync(
                            WaitUntil.Started,
                            _mailHost,
                            student.Email,
                            ConfirmEmailTemplate.Subject,
                            EmailUtils.GetFullMailContent(ConfirmEmailTemplate.GetBody(confirmURL, password)));
                        EmailSendResult statusMonitor = emailSendOperation.Value;

                        if (statusMonitor.Status == EmailSendStatus.Failed)
                        {
                            //throw new SendMailFailException();
                        }
                    }
                    catch (RequestFailedException ex)
                    {
                        //throw new SendMailFailException(ex);
                    }
                }
                //If exist => create student info in class, send notification
                else
                {
                    existUser.FullName = student.StudentName;
                    existUser.UserName = student.StudentId;
                    await _userManager.UpdateAsync(existUser);
                    await _messageService.SendMessage(new NotificationMessage()
                    {
                        AccountId = existUser.Id,
                        Message = "You are added into new IOT102 Class",
                        Title = "New Class Assigned"
                    });
                }
                var existStudent = await _unitOfWork.StudentRepository.Get().IgnoreQueryFilters()
                                                                            .FirstOrDefaultAsync(x => 
                                                                                                x.InformationId == existUser.Id &&
                                                                                                x.ClassId == classId);
                if (existStudent != null)
                {
                    existStudent.IsDeleted = false;
                    existStudent.ProjectId = null;
                    existStudent.ContributePercentage = null;
                    existStudent.FinalGrade = null;
                    existStudent.JobImportId = int.Parse(JobContext.CurrentJobId);
                    _unitOfWork.StudentRepository.Update(existStudent);
                }
                else
                {
                    await _unitOfWork.StudentRepository.InsertAsync(new Student
                    {
                        ClassId = classId,
                        InformationId = existUser.Id,
                        JobImportId = int.Parse(JobContext.CurrentJobId)
                    });
                }
                await _unitOfWork.SaveChangesAsync();
        }


        public async Task ProcessAddAllClassInfoToSemester(Guid semesterId, string fileName)
        {
            try
            {
                using var workbook = new XLWorkbook(fileName);
                //Get all classes
                var classCodes = GetClassCodesFromSheetNames(workbook);
                foreach (var classCode in classCodes)
                {
                    var worksheet = workbook.Worksheet(classCode);
                    var headerRowAndLecturerEmail = FindHeaderRowAndLecturerEmail(worksheet);
                    if (!headerRowAndLecturerEmail.HasValue)
                    {
                        throw new BaseBadRequestException($"Sheet {classCode} is not valid");
                    }

                    await ValidateLecturerEmailAndClassCode(classCode, headerRowAndLecturerEmail.Value.lecturerEmail!, semesterId);

                    var headerRow = headerRowAndLecturerEmail.Value.headerRow!;
                    //import empty class
                    var classDataRow = new ClassDataRow
                    {
                        ClassCode = classCode,
                        LecturerEmail = headerRowAndLecturerEmail.Value.lecturerEmail!
                    };
                    var importEmptyClassJobId = BackgroundJob.Enqueue<IBackgoundJobService>(importService => importService.ProcessAddClassToSemester(classDataRow, semesterId));
                    
                    //Then add all student to class
                    BackgroundJob.ContinueJobWith<IBackgoundJobService>(importEmptyClassJobId,
                        importService => importService.ProcessAddAllStudentListToClass(fileName, classCode, semesterId, headerRow.RowNumber(),
                                                                                    headerRow.Cells().Select(c => c.GetValue<string>()).ToArray()));
                    
                }

            }
            catch (Exception ex)
            {
                throw new CannotImportStudentException(ex);
            }
        }

        public async Task ProcessAddAllStudentListToClass(string fileName, string classCode, Guid semesterId, int headerRowNumber, string[] headerTitles)
        {
            try
            {
                await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
                {
                    using var workbook = new XLWorkbook(fileName);
                    var worksheet = workbook.Worksheet(classCode);
                    // Find the end of the used range
                    var lastRow = worksheet.RangeUsed().LastRow().RowNumber();

                    // Define the range from header row to the last used row
                    var range = worksheet.Range(headerRowNumber + 1, 1, lastRow, worksheet.ColumnsUsed().Count());

                    var studentList = new List<StudentDataRow>();

                    // Iterate through the rows in the range
                    foreach (var row in range.Rows())
                    {
                        var student = new StudentDataRow
                        {
                            StudentId = row.Cell(Array.IndexOf(headerTitles, nameof(StudentDataRow.StudentId)) + 1).GetValue<string>(),
                            StudentName = row.Cell(Array.IndexOf(headerTitles, "Student Name") + 1).GetValue<string>(),
                            Email = row.Cell(Array.IndexOf(headerTitles, nameof(StudentDataRow.Email)) + 1).GetValue<string>()
                        };
                        studentList.Add(student);
                    }

                    var validationResults = new List<ValidationResult>();
                    var classForImport = await _unitOfWork.IPMSClassRepository.Get().SingleAsync(x => x.ShortName == classCode && x.SemesterId == semesterId);
                    var existStudentInAnotherClass = await _unitOfWork.StudentRepository.Get()
                                                                                    .Include(x => x.Information)
                                                                                    .Include(x => x.Class)
                                                                                    .AnyAsync
                                                                                    (
                                                                                        x => studentList.Select(y => y.StudentId)
                                                                                        .Contains(x.Information.UserName)
                                                                                        && x.Class.SemesterId != classForImport.SemesterId
                                                                                    );
                    if (existStudentInAnotherClass) throw new BaseBadRequestException($"Exist student in another class");
                    var existStudentInClass = await _unitOfWork.StudentRepository.Get().Include(x => x.Information)
                                                                                 .Where(x => x.ClassId == classForImport.Id)
                                                                                 .ToListAsync();
                    if (existStudentInClass != null)
                    {
                        _unitOfWork.StudentRepository.DeleteRange(existStudentInClass);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    foreach (var student in studentList)
                    {
                        validationResults.Clear();
                        var validationContext = new ValidationContext(student);
                        bool isValid = Validator.TryValidateObject(student, validationContext, validationResults, true);

                        if (!isValid)
                        {
                            throw new BaseBadRequestException($"Student {student.StudentId} in Class {classCode} is not valid");
                        }
                        BackgroundJob.Enqueue<IBackgoundJobService>(importService => importService.ProcessAddStudentToClass(student, classForImport.Id, classCode));
                    }
                });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"Import class fail");
                throw new BaseBadRequestException($"Sheet {classCode} is not valid");
            }
        }

        private static (IXLRow? headerRow, string? lecturerEmail)? FindHeaderRowAndLecturerEmail(IXLWorksheet worksheet)
        {
            (IXLRow? headerRow, string? lecturerEmail) result = new();
            foreach (var row in worksheet.RowsUsed())
            {
                if (row.Cell(1).GetValue<string>() == "Lecturer Email")
                {
                    result.lecturerEmail = row.Cell(2).GetValue<string>();
                    //Validate email
                }
                if (row.Cell(1).GetValue<string>() == "StudentId"
                && row.Cell(2).GetValue<string>() == "Email"
                && row.Cell(3).GetValue<string>() == "Student Name") 
                {
                    result.headerRow = row;
                    break;
                }
            }
            if (result.headerRow != null && result.lecturerEmail != null) return result;
            return null;
        }

        private List<string> GetClassCodesFromSheetNames(XLWorkbook workbook)
        {
            var sheetNames = new List<string>();
            foreach (var worksheet in workbook.Worksheets)
            {
                sheetNames.Add(worksheet.Name);
            }
            return sheetNames;
        }
        private async Task ValidateLecturerEmailAndClassCode(string classCode, string lecturerEmail, Guid semesterId)
        {
            var lecturer = await _userManager.FindByEmailAsync(lecturerEmail);
            if(lecturer == null || !await _userManager.IsInRoleAsync(lecturer, UserRole.Lecturer.ToString()))
            {
                throw new BaseBadRequestException($"Lecturer with {lecturerEmail} is not exist");
            }
            var isClassExist = await _unitOfWork.IPMSClassRepository.Get().AnyAsync(x => x.ShortName == classCode && x.SemesterId == semesterId);
            if (isClassExist)
            {
                throw new BaseBadRequestException($"Class Code {classCode} is existed");
            }
        }
    }
}
