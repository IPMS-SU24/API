using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class SwapGroupRequestValidator : AbstractValidator<SwapGroupRequest>
    {
        public SwapGroupRequestValidator(IStudentGroupService studentGroupService, IHttpContextAccessor context)
        {
            var studentId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckValidRequestSwap(x,studentId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
