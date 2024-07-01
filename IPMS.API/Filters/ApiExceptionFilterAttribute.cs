using IPMS.API.Common.Enums;
using IPMS.API.Responses;
using IPMS.Business.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IPMS.API.Filters
{
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IDictionary<Type, Action<ExceptionContext>> _exceptionHandlers;
        public ApiExceptionFilterAttribute()
        {
            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(DataNotFoundException), HandleNotFoundException },
                {typeof(CannotImportStudentException), HandleCannotImportStudentException }
            };
        }

        private void HandleCannotImportStudentException(ExceptionContext context)
        {
            var exception = (CannotImportStudentException)context.Exception;

            var details = new IPMSResponse<object>()
            {
                Status = ResponseStatus.BadRequest,
                Errors = new Dictionary<string, string[]>()
                {
                    { "Invalid file",  new string[1]{ exception.Message} }
                }
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleUnknownException(ExceptionContext context)
        {
            var details = new IPMSResponse<object>()
            {
                Status = ResponseStatus.Fail
            };
            context.Result = new ObjectResult(details)
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
        public override void OnException(ExceptionContext context)
        {
            HandleException(context);
            base.OnException(context);
        }

        private void HandleException(ExceptionContext context)
        {
            Type type = context.Exception.GetType();
            if (_exceptionHandlers.ContainsKey(type))
            {
                _exceptionHandlers[type].Invoke(context);
                return;
            }
            if (!context.ModelState.IsValid)
            {
                HandleInvalidModelStateException(context);
                return;
            }
        }

        private void HandleValidationException(ExceptionContext context)
        {
            var exception = (ValidationException)context.Exception;

            var details = new IPMSResponse<object>
            {
                Status = ResponseStatus.BadRequest,
                Errors = exception.Errors
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleInvalidModelStateException(ExceptionContext context)
        {
            var details = new IPMSResponse<object>()
            {
                Status = ResponseStatus.BadRequest
            };

            context.Result = new BadRequestObjectResult(details);

            context.ExceptionHandled = true;
        }

        private void HandleNotFoundException(ExceptionContext context)
        {
            var exception = (DataNotFoundException)context.Exception;

            var details = new IPMSResponse<object>()
            {
                Status = ResponseStatus.DataNotFound
            };

            context.Result = new OkObjectResult(details);

            context.ExceptionHandled = true;
        }
    }
}
