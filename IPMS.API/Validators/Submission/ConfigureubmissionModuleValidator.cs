using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.SubmissionModule;

namespace IPMS.API.Validators.Submission
{
    public class ConfigSubmissionModuleValidator : AbstractValidator<ConfigureSubmissionModuleRequest>
    {
        public ConfigSubmissionModuleValidator(ISubmissionModuleService submissionModuleService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.SubmissionModules.Select(sm => sm.StartDate)).NotEmpty();
            RuleFor(x => x.SubmissionModules.Select(sm => sm.EndDate)).NotEmpty();
            RuleFor(x => x.SubmissionModules.Select(sm => sm.Percentage)).NotEmpty();
            RuleFor(x => x.SubmissionModules.Select(sm => sm.ModuleName)).NotEmpty();
            RuleFor(x => x.SubmissionModules.Select(sm => sm.Percentage)).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await submissionModuleService.ConfigureSubmissionModuleValidator(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
