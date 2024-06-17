using FluentValidation;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;

namespace IPMS.API.Validators.Submission
{
    public class UpdateProjectSubmissionValidator : AbstractValidator<UpdateProjectSubmissionRequest>
    {
        public UpdateProjectSubmissionValidator(IProjectSubmissionService topicService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.SubmissionDate).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await topicService.UpdateProjectSubmissionValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
