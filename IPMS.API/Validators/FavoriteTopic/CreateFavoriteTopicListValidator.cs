using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Services;

namespace IPMS.API.Validators.FavoriteTopic
{
    public class CreateFavoriteTopicListValidator : AbstractValidator<CreateFavoriteTopicListRequest>
    {
        public CreateFavoriteTopicListValidator(IFavoriteTopicListService favoriteTopicListService, IHttpContextAccessor httpContext)
        {
            var lecturerId = httpContext.HttpContext.User.Claims.GetUserId();
            RuleFor(x=>x.ListName).MaximumLength(50);
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await favoriteTopicListService.CheckValidCreate(lecturerId, x);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
