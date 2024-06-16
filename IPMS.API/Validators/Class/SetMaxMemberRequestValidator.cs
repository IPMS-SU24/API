using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Class
{
    public class SetMaxMemberRequestValidator : AbstractValidator<SetMaxMemberRequest>
    {
        public SetMaxMemberRequestValidator(IClassService classService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.MaxMember).GreaterThan(0);
            RuleFor(x=>x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.CheckSetMaxMemberRequestValid(lecturerId,x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
