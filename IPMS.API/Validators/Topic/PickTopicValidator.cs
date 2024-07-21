using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;

namespace IPMS.API.Validators.Topic
{
    public class PickTopicValidator : AbstractValidator<PickTopicRequest>
    {
        public PickTopicValidator(IClassTopicService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.PickTopicValidators(x.TopicId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
