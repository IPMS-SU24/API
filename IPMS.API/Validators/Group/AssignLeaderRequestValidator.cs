using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class AssignLeaderRequestValidator : AbstractValidator<AssignLeaderRequest>
    {
        public AssignLeaderRequestValidator(IStudentGroupService studentGroupService, IHttpContextAccessor context)
        {
            var leaderId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckValidAssignLeaderRequest(x, leaderId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
