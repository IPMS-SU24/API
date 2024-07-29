using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class UpdateIoTDeviceValidator : AbstractValidator<UpdateIoTDeviceRequest>
    {
        public UpdateIoTDeviceValidator(IIoTDataService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.UpdateIoTDeviceValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
