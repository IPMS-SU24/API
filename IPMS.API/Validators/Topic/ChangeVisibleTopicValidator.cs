using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Topic
{
    public class ChangeVisibleTopicValidator : AbstractValidator<ChangeVisibleTopicRequest>
    {
        public ChangeVisibleTopicValidator(ITopicService topicService)
        {
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.ChangeVisibleValidator(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
