using IPMS.API.Filters;
using IPMS.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [ApiExceptionFilter]
    [ValidateModelState]
    [ApiController]
    [Route("api/v1/[controller]s/")]
    public class ApiControllerBase : ControllerBase
    {
        protected IActionResult GetResponse<TData>(IPMSResponse<TData> response)
        {
            return StatusCode((int)response.Status,response);
        }
    }
}
