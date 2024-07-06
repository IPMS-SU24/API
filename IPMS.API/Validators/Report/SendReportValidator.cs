using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Report;

namespace IPMS.API.Validators.Report
{
    public class SendReportValidator : AbstractValidator<SendReportRequest>
    {
        public SendReportValidator(IReportService reportService, IHttpContextAccessor context)
        {
            var reporterId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync( async (x, validationContext, cancellationToken) =>
            {
                var validationResult =  await reportService.CheckValidReport(x, reporterId);
                if(!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
