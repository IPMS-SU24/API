using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.SubmissionModule;

namespace IPMS.API.Validators.Submission
{
    public class ConfigureClassModuleDeadlineValidator : AbstractValidator<ConfigureClassModuleDeadlineRequest>
    {
        public ConfigureClassModuleDeadlineValidator(ISubmissionModuleService submissionModuleService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ClassId).NotEmpty();
            RuleFor(x => x.SubmissionModuleId).NotEmpty();
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await submissionModuleService.ConfigureClassModuleDeadlineValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
