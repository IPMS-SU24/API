using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Services;

namespace IPMS.API.Validators.FavoriteTopic
{
    public class AssignTopicListValidator : AbstractValidator<AssignTopicListRequest>
    {
        public AssignTopicListValidator(IFavoriteTopicListService favoriteTopicListService, IHttpContextAccessor httpContext)
        {
            var lecturerId = httpContext.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ClassesId).NotEmpty();
            RuleFor(x => x.ListId).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await favoriteTopicListService.AssignTopicListValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
