using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Results;
using IPMS.API.Common.Enums;
using IPMS.API.Responses;

namespace IPMS.API.Filters
{
    public class IPMSResultFactory : IFluentValidationAutoValidationResultFactory
    {
        public IActionResult CreateActionResult(ActionExecutingContext context, ValidationProblemDetails? validationProblemDetails)
        {
            return new BadRequestObjectResult(new IPMSResponse<object>
            {
                Status = ResponseStatus.BadRequest,
                Errors = validationProblemDetails?.Errors
            });
        }
    }
}
