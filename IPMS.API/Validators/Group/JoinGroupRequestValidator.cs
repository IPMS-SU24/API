using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class JoinGroupRequestValidator : AbstractValidator<JoinGroupRequest>
    {
        public JoinGroupRequestValidator(IStudentGroupService studentGroupService, IHttpContextAccessor context)
        {
            var studentId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x=>x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckValidRequestJoin(x,studentId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
