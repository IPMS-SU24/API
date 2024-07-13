using FluentValidation;
using IPMS.Business.Requests.FavoriteTopic;

namespace IPMS.API.Validators.FavoriteTopic
{
    public class CreateFavoriteTopicListValidator : AbstractValidator<CreateFavoriteTopicListRequest>
    {
        public CreateFavoriteTopicListValidator()
        {
            RuleFor(x=>x.ListName).MaximumLength(50);
        }
    }
}
