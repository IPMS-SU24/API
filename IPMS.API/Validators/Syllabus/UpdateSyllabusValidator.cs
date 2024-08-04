using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Admin;
using IPMS.Business.Requests.IoTComponent;

namespace IPMS.API.Validators.IoT
{
    public class UpdateSyllabusValidator : AbstractValidator<UpdateSyllabusRequest>
    {
        public UpdateSyllabusValidator(IAdminService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();

            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.UpdateSyllabusValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
