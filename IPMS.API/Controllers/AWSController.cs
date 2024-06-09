using IPMS.API.Common.Attributes;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [Route("api/v1/[controller]/")]
    public class AWSController : ApiControllerBase
    {
        private readonly IPresignedUrlService _presignedUrlService;
        public AWSController(IPresignedUrlService PresignedUrlService)
        {
            _presignedUrlService = PresignedUrlService;
        }

        [EnumAuthorize(UserRole.Student, UserRole.Lecturer, UserRole.Admin)]
        [HttpGet]
        [Route("download")]
        public IActionResult GeneratePresignedDownloadUrl(string objectKey)
        {
            var response = new IPMSResponse<string>()
            {
                Data = _presignedUrlService.GeneratePresignedDownloadUrl(objectKey)
            }; // Default is success
        
            return Ok(response);
        }

        [EnumAuthorize(UserRole.Student, UserRole.Lecturer, UserRole.Admin)]
        [HttpGet]
        [Route("upload")]
        public IActionResult GeneratePresignedUploadUrl([FromQuery] string objectName)
        {
            var response = new IPMSResponse<string>()
            {
                Data = _presignedUrlService.GeneratePresignedUploadUrl(objectName)
            }; // Default is success

            return Ok(response);
        }

    }
}
