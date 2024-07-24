using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Group;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Group
{
    public class LeaderEvaluateMemberRequestValidator : AbstractValidator<LeaderEvaluateMembersRequest>
    {
        public LeaderEvaluateMemberRequestValidator(IStudentGroupService studentGroupService)
        {
            RuleForEach(x => x.Members)
                .ChildRules(x =>
                {
                    x.RuleFor(con => con.Percentage).GreaterThan(0);
                    x.RuleFor(con => con.Percentage).LessThanOrEqualTo(100);
                });
            RuleFor(x => x.Members).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await studentGroupService.CheckValidEvaluateMembers(x);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
