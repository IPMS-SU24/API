using FluentValidation;
using IPMS.Business.Interfaces.Services;

namespace IPMS.API.Validators.Assessment
{
    public class GetAssessmentByIdValidator : AbstractValidator<Guid>
    {
        public GetAssessmentByIdValidator(IAssessmentService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.GetAssessmentByIdValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
