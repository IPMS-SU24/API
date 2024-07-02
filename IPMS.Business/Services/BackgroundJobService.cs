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

namespace IPMS.Business.Services
{
    public class BackgroundJobService : IBackgoundJobService
    {
        private readonly UserManager<IPMSUser> _userManager;
        private readonly MailServer _mailServer;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMessageService _messageService;
        private readonly IHttpContextAccessor _httpContext;

        public BackgroundJobService(UserManager<IPMSUser> userManager, MailServer mailServer, IUnitOfWork unitOfWork, IMessageService messageService, IHttpContextAccessor httpContext)
        {
            _userManager = userManager;
            _mailServer = mailServer;
            _unitOfWork = unitOfWork;
            _messageService = messageService;
            _httpContext = httpContext;
        }
        [AutomaticRetry(Attempts = 5)]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public async Task ProcessAddStudentToClass(List<StudentDataRow> students, Guid classId, string serverDomain)
        {
            await _unitOfWork.RollbackTransactionOnFailAsync(async () =>
            {
                var existStudentInClass = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == classId).ToListAsync();
                if (existStudentInClass != null)
                {
                    _unitOfWork.StudentRepository.HardDeleteRange(existStudentInClass);
                }
                await _unitOfWork.SaveChangesAsync();
                foreach (var student in students)
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
                        existUser = await _userManager.FindByEmailAsync(student.Email);
                        //Send mail confirm
                        try
                        {
                            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(existUser);
                            var confirmURL = PathUtils.GetConfirmURL(serverDomain, existUser.Id, confirmEmailToken);
                            EmailSendOperation emailSendOperation = await _mailServer.Client.SendAsync(
                                WaitUntil.Completed,
                                "DoNotReply@5c9fc577-26d6-49e3-98e1-62c04cc4e4e0.azurecomm.net",
                                student.Email,
                                ConfirmEmailTemplate.Subject,
                                ConfirmEmailTemplate.GetBody(confirmURL, password));
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
                        await _messageService.SendMessage(new NotificationMessage()
                        {
                            AccountId = existUser.Id,
                            Message = "You are added into new IOT102 Class",
                            Title = "New Class Assigned"
                        });
                    }
                    await _unitOfWork.StudentRepository.InsertAsync(new Student
                    {
                        ClassId = classId,
                        InformationId = existUser.Id
                    });
                    await _unitOfWork.SaveChangesAsync();
                }
            });
        }
    }
}
