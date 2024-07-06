using FluentValidation;
using FluentValidation.Results;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;

namespace IPMS.API.Validators.Class
{
    public class ImportStudentRequestValidator : AbstractValidator<AddStudentsToClassRequest>
    {
        public ImportStudentRequestValidator(IClassService classService, IHttpContextAccessor context)
        {
            var lecturerId = context.HttpContext.User.Claims.GetUserId();
            RuleFor(x => x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.CheckImportStudentValidAsync(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
