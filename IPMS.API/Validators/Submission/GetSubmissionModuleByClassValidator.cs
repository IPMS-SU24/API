using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.SubmissionModule;

namespace IPMS.API.Validators.Submission
{
    public class GetSubmissionModuleByClassValidator : AbstractValidator<GetSubmissionModuleByClassRequest>
    {
        public GetSubmissionModuleByClassValidator(ISubmissionModuleService submissionModuleService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.classId).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await submissionModuleService.GetAssessmentSubmissionModuleByClassValidator(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
