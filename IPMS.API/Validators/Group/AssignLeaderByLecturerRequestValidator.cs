using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class AssignLeaderByLecturerRequestValidator : AbstractValidator<AssignLeaderByLecturerRequest>
    {
        public AssignLeaderByLecturerRequestValidator(IStudentGroupService studentGroupService, IHttpContextAccessor accessor)
        {
            var lecturerId = accessor.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckValidAssignLeaderByLecturer(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
