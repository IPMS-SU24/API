using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Services;

namespace IPMS.API.Validators.IoT
{
    public class ReturnIoTComponentsValidators : AbstractValidator<ReturnIoTComponentsRequest>
    {
        public ReturnIoTComponentsValidators(IBorrowIoTService iotDataService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ProjectId).NotEmpty();
            RuleFor(x => x.CreatedAt).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await iotDataService.ReturnIoTComponentsValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
