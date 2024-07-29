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

namespace IPMS.Business.Services
{
    public class BackgroundJobService : IBackgoundJobService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
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
                                    RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _mailServer = mailServer;
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _httpContext = httpContext;
            _mailHost = configuration["MailFrom"];
            _roleManager = roleManager;
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
                //save class
                var newClass = new IPMSClass()
                {
                    LecturerId = @class.LecturerId,
                    ShortName = @class.ClassCode,
                    SemesterId = semesterId,
                    Name = string.Empty,
                    JobImportId = int.Parse(JobContext.CurrentJobId)
                };
                await _unitOfWork.IPMSClassRepository.InsertAsync(newClass);
                var committee = new Committee()
                {
                    ClassId = newClass.Id,
                    LecturerId = @class.LecturerId,
                    Percentage = 100
                };
                await _unitOfWork.CommitteeRepository.InsertAsync(committee);
                await _unitOfWork.SaveChangesAsync();
                //Send noti to lecturer
                await _messageService.SendMessage(new NotificationMessage()
                {
                    AccountId = @class.LecturerId,
                    Message = $"You are added into Class {@class.ClassCode}",
                    Title = "New Class Assigned"
                });
            });
        }

        public async Task ProcessAddStudentToClass(StudentDataRow student, Guid classId)
        {
            await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
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
                            WaitUntil.Completed,
                            _mailHost,
                            student.Email,
                            ConfirmEmailTemplate.Subject,
                            EmailUtils.GetFullMailContent(ConfirmEmailTemplate.GetBody(confirmURL, password)));
                        EmailSendResult statusMonitor = emailSendOperation.Value;

                        if (statusMonitor.Status == EmailSendStatus.Failed)
                        {
                            throw new SendMailFailException();
                        }
                    }
                    catch (RequestFailedException ex)
                    {
                        throw new SendMailFailException(ex);
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
            });
        }
    }
}
