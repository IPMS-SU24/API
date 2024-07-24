using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectSubmission;
using IPMS.Business.Requests.SubmissionModule;

namespace IPMS.API.Validators.Submission
{
    public class LecturerEvaluateValidators : AbstractValidator<LecturerEvaluateRequest>
    {
        public LecturerEvaluateValidators(ISubmissionModuleService submissionModuleService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x.ClassId).NotEmpty();
            RuleFor(x => x.GroupId).NotEmpty();
            RuleFor(x => x.Members.Select(m => m.StudentId)).NotEmpty();
            RuleFor(x => x.Members.Select(m => m.Percentage)).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await submissionModuleService.LecturerEvaluateValidator(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddFailure(validationResult.Message);
                }
            });
        }
    }
}
