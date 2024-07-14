using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;

namespace IPMS.API.Validators.Topic
{
    public class ReviewSuggestedTopicRequestValidator : AbstractValidator<ReviewSuggestedTopicRequest>
    {
        public ReviewSuggestedTopicRequestValidator(ITopicService topicService, IHttpContextAccessor context)
        {
            var leaderId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ClassId).NotEmpty();
            RuleFor(x => x.TopicId).NotEmpty();
          //  RuleFor(x => x.IsApproved).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.ReviewSuggestedTopicValidators(x, leaderId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
