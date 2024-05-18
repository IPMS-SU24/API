using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using IPMS.API.Responses;
using IPMS.API.Common.Enums;

namespace IPMS.API.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(v => v.Errors.Count > 0)
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage)
                        .ToArray();
                var details = new IPMSResponse<object>
                {
                    Status = ResponseStatus.BadRequest,
                    Errors = new Dictionary<string, string[]>
                    {
                        { "Invalid",errors}
                    }
                };

                context.Result = new BadRequestObjectResult(details);
            }
        }
    }
}
