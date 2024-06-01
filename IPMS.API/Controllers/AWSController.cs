using IPMS.API.Common.Attributes;
using IPMS.API.Common.Enums;
using IPMS.API.Responses;
using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    
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
        public IActionResult GeneratePresignedUploadUrl(string objectKey)
        {
            var response = new IPMSResponse<string>()
            {
                Data = _presignedUrlService.GeneratePresignedUploadUrl(objectKey)
            }; // Default is success

            return Ok(response);
        }

    }
}
