using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectPreference;

namespace IPMS.API.Validators.Submission
{
    public class UpdateProjectPreferencesStatusValidator : AbstractValidator<UpdateProjectPreferenceStatusRequest>
    {
        public UpdateProjectPreferencesStatusValidator(IProjectService submissionModuleService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.Projects.Select(p => p.ProjectId)).NotEmpty();
            RuleFor(x => x.Projects.Select(p => p.IsPublished)).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await submissionModuleService.UpdateProjectPreferencesStatusValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
