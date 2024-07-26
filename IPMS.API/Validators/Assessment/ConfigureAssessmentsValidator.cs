using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Assessment;

namespace IPMS.API.Validators.Assessment
{
    public class ConfigureAssessmentsValidator : AbstractValidator<ConfigureAssessmentsRequest>
    {
        public ConfigureAssessmentsValidator(IAssessmentService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.SyllabusId).NotEmpty();
            RuleFor(x => x.Assessments).NotEmpty();
            RuleFor(x => x.Assessments.Select(a => a.Name)).NotEmpty();
            RuleFor(x => x.Assessments.Select(a => a.Percentage)).NotEmpty();
            RuleFor(x => x.Assessments.Select(a => a.Order)).NotEmpty();
            RuleFor(x => x.Assessments.Select(a => a.IsDelete)).NotEmpty();

            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.ConfigureAssessmentsValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
