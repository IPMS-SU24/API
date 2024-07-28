using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;

namespace IPMS.API.Validators.Class
{
    public class UpdateClassDeadlineValidator : AbstractValidator<UpdateClassDeadlineRequest>
    {
        public UpdateClassDeadlineValidator(IClassService classService, IHttpContextAccessor accessor)
        {
            var lecturerId = accessor.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ClassId).NotEmpty();
            RuleFor(x => x.CreateGroup).NotEmpty();
            RuleFor(x => x.ChangeGroup).NotEmpty();
            RuleFor(x => x.ChangeTopic).NotEmpty();
            RuleFor(x => x.BorrowIot).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.UpdateClassDeadlineValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
