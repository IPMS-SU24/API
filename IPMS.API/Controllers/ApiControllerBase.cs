using IPMS.API.Filters;
using IPMS.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [ApiExceptionFilterAttribute]
    [ValidateModelState]
    [ApiController]
    [Route("api/v1/[controller]s/")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult GetActionResponse<TData>(IPMSResponse<TData> response)
        {
            
            return StatusCode((int) response.Status, response);
        }
    }
}
