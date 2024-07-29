using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;

namespace IPMS.API.Validators.Class
{
    public class ImportClassRequestValidator : AbstractValidator<ImportClassRequest>
    {
        public ImportClassRequestValidator(IClassService classService)
        {
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.CheckImportClassValidAsync(x);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
