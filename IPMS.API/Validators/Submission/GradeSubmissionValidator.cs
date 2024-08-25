using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Requests.SubmissionModule;

namespace IPMS.API.Validators.Submission
{
    public class GradeSubmissionValidators : AbstractValidator<GradeSubmissionRequest>
    {
        public GradeSubmissionValidators(IProjectSubmissionService submissionModuleService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.SubmissionId).NotEmpty();
           // RuleFor(x => x.Grade).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await submissionModuleService.GradeSubmissionValidators(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
