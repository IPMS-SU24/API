using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Assessment;

namespace IPMS.API.Validators.Assessment
{
    public class GetAssessmentByIdValidator : AbstractValidator<GetAssessmentByIdRequest>
    {
        public GetAssessmentByIdValidator(IAssessmentService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.GetAssessmentByIdValidators(x.Id);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
