using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Topic
{
    public class RegisterTopicRequestValidator : AbstractValidator<RegisterTopicRequest>
    {
        public RegisterTopicRequestValidator(ITopicService topicService, IHttpContextAccessor context)
        {
            var leaderId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.TopicName).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.CheckRegisterValid(x, leaderId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
