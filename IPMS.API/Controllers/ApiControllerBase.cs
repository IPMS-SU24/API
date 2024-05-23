using IPMS.API.Filters;
using IPMS.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [ApiExceptionFilter]
    [ValidateModelState]
    [ApiController]
    [Route("api/v1/[controller]s/")]
    public class ApiControllerBase : ControllerBase
    {
    }
}
