using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Group
{
    public class LecturerAddStudentsToGroupRequestValidator : AbstractValidator<LecturerAddStudentsToGroupRequest>
    {
        public LecturerAddStudentsToGroupRequestValidator(IStudentGroupService studentGroupService, IHttpContextAccessor context)
        {

            RuleFor(x=>x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckValidForLecturerAddStudentToGroup(x, context.HttpContext.User.Claims.GetUserId());
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
