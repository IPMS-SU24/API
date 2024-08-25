using FluentValidation;
using IPMS.API.Common.Extensions;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Class;
using IPMS.Business.Services;

namespace IPMS.API.Validators.Class
{
    public class ClassGradeRequestValidator : AbstractValidator<ClassExportGradeRequest>
    {
        public ClassGradeRequestValidator(IHttpContextAccessor accessor, IClassService classService)
        {
            var lecturerId = accessor.HttpContext.User.Claims.GetUserId();
            RuleFor(x=>x).CustomAsync(async (x, validationContext, cancellationToken) =>
            {
                var validationResult = await classService.CheckExportGradeValid(x, lecturerId);
                if (!validationResult.Result)
                {
                    validationContext.AddBusinessFailure(validationResult.Message);
                }
            });
        }
    }
}
