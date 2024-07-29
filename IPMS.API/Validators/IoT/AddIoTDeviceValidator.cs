using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class AddIoTDeviceValidator : AbstractValidator<AddIoTDeviceRequest>
    {
        public AddIoTDeviceValidator(IIoTDataService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.AddIoTDeviceValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
