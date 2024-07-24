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
        private readonly ILogger<ApiExceptionFilterAttribute> _logger;

        public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
        {
            // Register known exception types and handlers.
            _exceptionHandlers = new Dictionary<Type, Action<ExceptionContext>>
            {
                { typeof(ValidationException), HandleValidationException },
                { typeof(DataNotFoundException), HandleNotFoundException },
                { typeof(BaseBadRequestException), HandleBaseBadRequestException }
            };
            _logger = logger;
        }

        private void HandleCannotImportStudentException(ExceptionContext context)
        {
            var exception = (CannotImportStudentException)context.Exception;
            _logger.LogError(exception.InnerException, exception.Message);
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
        private void HandleBaseBadRequestException(ExceptionContext context)
        {
            var exception = (BaseBadRequestException)context.Exception;

            var details = new IPMSResponse<object>()
            {
                Status = ResponseStatus.BadRequest,
                Errors = new Dictionary<string, string[]>()
                {
                    { "Invalid request", string.IsNullOrEmpty(exception.Message) ?  new string[1]{ exception.Message} : exception.Errors}
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
            if(type.IsSubclassOf(typeof(BaseBadRequestException))) type = typeof(BaseBadRequestException);
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
            if (!string.IsNullOrEmpty(exception.Message))
            {
                details.Message = exception.Message;
            }
            context.Result = new OkObjectResult(details);

            context.ExceptionHandled = true;
        }
    }
}
