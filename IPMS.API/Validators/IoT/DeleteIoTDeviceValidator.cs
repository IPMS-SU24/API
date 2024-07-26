using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class DeleteIoTDeviceValidator : AbstractValidator<DeleteIoTDeviceRequest>
    {
        public DeleteIoTDeviceValidator(IIoTDataService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Id).NotEmpty();


            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.DeleteIoTDeviceValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
