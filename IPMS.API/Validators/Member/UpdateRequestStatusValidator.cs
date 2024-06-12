using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;
using IPMS.Business.Requests.MemberHistory;

namespace IPMS.API.Validators.Member
{
    public class UpdateRequestStatusValidator : AbstractValidator<UpdateRequestStatusRequest>
    {
        public UpdateRequestStatusValidator(IMemberHistoryService memberHistoryService, IHttpContextAccessor context)
        {
            var studentId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Status).NotEmpty();
            RuleFor(x => x.ReviewId).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await memberHistoryService.UpdateRequestStatusValidators(x, studentId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
