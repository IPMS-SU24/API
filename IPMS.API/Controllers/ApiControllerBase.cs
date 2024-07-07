using IPMS.API.Filters;
using IPMS.API.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IPMS.API.Controllers
{
    [ServiceFilter(typeof(ApiExceptionFilterAttribute))]
    [ValidateModelState]
    [ApiController]
    [Route("api/v1/[controller]/")]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult GetActionResponse<TData>(IPMSResponse<TData> response)
        {
            
            return StatusCode((int) response.Status, response);
        }
    }
}
