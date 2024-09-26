using Azure;
using Azure.Communication.Email;
using ClosedXML.Excel;
using Hangfire;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Hangfire;
using IPMS.Business.Common.Models;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

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

        public async Task AddJobIdToStudent(string jobId, Guid classId, string email)
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
        }

        public async Task ProcessAddStudentToClass(StudentDataRow student, Guid classId, string classCode)
        {
            await TryCatchBackgroundJobException(async () =>
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
                        throw new CannotCreateAccountException(classCode, student.StudentId);
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRole.Student.ToString()))
                    {
                        await _roleManager.CreateAsync(new IdentityRole<Guid>(UserRole.Student.ToString()));
                    }
                    await _userManager.AddToRoleAsync(stuAccount, UserRole.Student.ToString());
                    existUser = await _userManager.FindByEmailAsync(student.Email);
                    //Send mail confirm
                    try
                    {
                        var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(existUser);
                        var confirmURL = PathUtils.GetConfirmURL(existUser.Id, confirmEmailToken);
                        _mailServer.Client.SendAsync(
                            WaitUntil.Started,
                            _mailHost,
                            student.Email,
                            ConfirmEmailTemplate.Subject,
                            EmailUtils.GetFullMailContent(ConfirmEmailTemplate.GetBody(confirmURL, password)));
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

                // Set Succeeded student to JobStorage
                StoreToHash(classCode, student.StudentId, ImportJob.SucceededStatus);
            });
        }


        public async Task ProcessAddAllClassInfoToSemester(Guid semesterId, string fileName)
        {
            try
            {
                // Set batch job Id Semester Job Hash
                StoreToHash(semesterId.ToString(), JobContext.CurrentJobId, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));

                using var workbook = new XLWorkbook(fileName);
                // Get all classes
                var classCodes = GetClassCodesFromSheetNames(workbook);

                // Store numberOfClasses need to process to batchJob Hash
                StoreToHash(JobContext.CurrentJobId, ImportJob.NumberOfClassesKey, classCodes.Count.ToString());

                foreach (var classCode in classCodes)
                {
                    await TryCatchBackgroundJobException(async () =>
                    {
                        var worksheet = workbook.Worksheet(classCode);
                        var headerRowAndLecturerEmail = FindHeaderRowAndLecturerEmail(worksheet);
                        if (!headerRowAndLecturerEmail.HasValue)
                        {
                            throw new BackgroundJobException($"Not found lecturer email or wrong data format", JobContext.CurrentJobId, classCode);
                        }

                        await ValidateLecturerEmailAndClassCode(classCode, headerRowAndLecturerEmail.Value.lecturerEmail!, semesterId);

                        var headerRow = headerRowAndLecturerEmail.Value.headerRow!;
                        // Import empty class
                        var classDataRow = new ClassDataRow
                        {
                            ClassCode = classCode,
                            LecturerEmail = headerRowAndLecturerEmail.Value.lecturerEmail!
                        };

                        await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
                        {
                            await ProcessAddClassToSemester(classDataRow, semesterId);
                            // Set Processing class to JobStorage
                            StoreToHash(JobContext.CurrentJobId, classCode, ImportJob.ProcessingStatus);

                            // Then add all student to class
                            await ProcessAddAllStudentListToClass(worksheet, classCode, semesterId, headerRow.RowNumber(),
                                                                                            headerRow.Cells().Select(c => c.GetValue<string>()).ToArray());
                            // Set Done class to JobStorage
                            StoreToHash(JobContext.CurrentJobId, classCode, ImportJob.DoneStatus);

                            var lecturer = await _userManager.FindByEmailAsync(classDataRow.LecturerEmail);
                            //Send noti to lecturer
                            await _messageService.SendMessage(new NotificationMessage()
                            {
                                AccountId = lecturer.Id,
                                Message = $"You are added into Class {classCode}",
                                Title = "New Class Assigned"
                            });
                        });
                    });
                }

            }
            catch (Exception ex)
            {
                throw new CannotImportStudentException(ex);
            }
        }

        public async Task ProcessAddAllStudentListToClass(IXLWorksheet worksheet, string classCode, Guid semesterId, int headerRowNumber, string[] headerTitles)
        {
            try
            {
                await TryCatchBackgroundJobException(async () =>
                {
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
                                                                                    .Where(x => (studentList.Select(y => y.StudentId)
                                                                                        .Contains(x.Information.UserName) || studentList.Select(y => y.Email)
                                                                                        .Contains(x.Information.Email))
                                                                                        && x.Class.SemesterId == classForImport.SemesterId && x.ClassId != classForImport.Id)
                                                                                    .Select(x => new { x.Information.UserName, x.Information.Email })
                                                                                    .Distinct()
                                                                                    .ToListAsync();
                    //if (existStudentInAnotherClass) throw new BackgroundJobException($"Exist student in another class", );
                    studentList.RemoveAll(x => existStudentInAnotherClass.Select(e => e.UserName).Contains(x.StudentId) ||
                                                            existStudentInAnotherClass.Select(e => e.Email).Contains(x.Email));

                    // Store numberOfStudents need to process to batchJob Hash
                    StoreToHash(classCode, ImportJob.NumberOfStudentsKey, studentList.Count.ToString());
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
                        await TryCatchBackgroundJobException(async () =>
                        {

                            validationResults.Clear();
                            var validationContext = new ValidationContext(student);
                            bool isValid = Validator.TryValidateObject(student, validationContext, validationResults, true);

                            if (!isValid)
                            {
                                throw new BackgroundJobException($"Data is not valid", classCode, student.StudentId);
                            }
                            await ProcessAddStudentToClass(student, classForImport.Id, classCode);
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import class fail");
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
            if (lecturer == null || !await _userManager.IsInRoleAsync(lecturer, UserRole.Lecturer.ToString()))
            {
                throw new BackgroundJobException($"Lecturer with {lecturerEmail} is not exist", JobContext.CurrentJobId, classCode);
            }
            var isClassExist = await _unitOfWork.IPMSClassRepository.Get().AnyAsync(x => x.ShortName == classCode && x.SemesterId == semesterId);
            if (isClassExist)
            {
                throw new BackgroundJobException($"Class Code {classCode} is existed", JobContext.CurrentJobId, classCode);
            }
        }

        private async Task TryCatchBackgroundJobException(Func<Task> function)
        {
            try
            {
                await function();
            }
            catch (BackgroundJobException ex)
            {
                _logger.LogInformation(ex.Message);
                StoreToHash(ex.HashKey, ex.ValueKey, ex.Message);
            }
        }

        private void StoreToHash(string hashKey, string name, string value)
        {
            // Store fail message to JobStorage
            var jobConnection = JobStorage.Current.GetConnection();
            var valueDict = new Dictionary<string, string>()
                {
                    {name, value }
                };
            jobConnection.SetRangeInHash(hashKey, valueDict);
        }
    }
}
