using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;

namespace IPMS.API.Validators.Group
{
    public class CreateGroupRequestValidator : AbstractValidator<CreateGroupValidateRequest>
    {
        public CreateGroupRequestValidator(IStudentGroupService studentGroupService)
        {
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckStudentValidForCreateGroup(x.StudentId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
