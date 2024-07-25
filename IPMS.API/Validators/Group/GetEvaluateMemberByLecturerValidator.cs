using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class GetEvaluateMemberByLecturerValidator : AbstractValidator<GetMemberContributionRequest>
    {
        public GetEvaluateMemberByLecturerValidator(IHttpContextAccessor context, IStudentGroupService studentGroupService)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckGetContributeByLecturer(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
