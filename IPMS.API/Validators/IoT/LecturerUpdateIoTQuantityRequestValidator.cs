using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Services;

namespace IPMS.API.Validators.IoT
{
    public class LecturerUpdateIoTQuantityRequestValidator : AbstractValidator<UpdateIoTQuantityRequest>
    {
        public LecturerUpdateIoTQuantityRequestValidator(IIoTDataService iotDataService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.Quantity).GreaterThan(0);
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await iotDataService.CheckLecturerUpdateIoTValid(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
