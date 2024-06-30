using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Topic;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Topic
{
    public class LecturerRegisterTopicRequestValidator : AbstractValidator<LecturerRegisterTopicRequest>
    {
        public LecturerRegisterTopicRequestValidator(ITopicService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.TopicName).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.LecturerRegisterNewTopicValidator(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
