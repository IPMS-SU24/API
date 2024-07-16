using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class RemoveStudentOutGroupValidator : AbstractValidator<RemoveStudentOutGroupRequest>
    {
        public RemoveStudentOutGroupValidator(IStudentGroupService studentGroupService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.StudentId).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.RemoveStudentOutGroupValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
