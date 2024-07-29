using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Class
{
    public class UpdateClassDetailValidator : AbstractValidator<UpdateClassDetailRequest>
    {
        public UpdateClassDetailValidator(IClassService classService, IHttpContextAccessor context)
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ShortName).NotEmpty();
            RuleFor(x => x.Committees.Select(c => c.Percentage)).NotEmpty();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.UpdateClassDetailValidators(x);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
