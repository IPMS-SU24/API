using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;

namespace IPMS.API.Validators.Class
{
    public class RemoveOutOfClassRequestValidator : AbstractValidator<RemoveOutOfClassRequest>
    {
        public RemoveOutOfClassRequestValidator(IClassService classService, IHttpContextAccessor accessor)
        {
            var lecturerId = accessor.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.CheckValidRemoveOutOfClass(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
