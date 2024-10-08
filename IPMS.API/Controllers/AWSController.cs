﻿using IPMS.API.Common.Attributes;
using IPMS.API.Responses;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    [Route("api/v1/[controller]/")]
    [Authorize]
    public class AWSController : ApiControllerBase
    {
        private readonly IPresignedUrlService _presignedUrlService;
        public AWSController(IPresignedUrlService PresignedUrlService)
        {
            _presignedUrlService = PresignedUrlService;
        }

        [HttpGet]
        [Route("download")]
        public IActionResult GeneratePresignedDownloadUrl(string objectKey)
        {
            var response = new IPMSResponse<string?>()
            {
                Data = _presignedUrlService.GeneratePresignedDownloadUrl(objectKey)
            }; // Default is success
            if (response.Data == null) throw new DataNotFoundException();
            return GetActionResponse(response);
        }

        [HttpGet]
        [Route("upload")]
        public IActionResult GeneratePresignedUploadUrl([FromQuery] string objectName)
        {
            var response = new IPMSResponse<string>()
            {
                Data = _presignedUrlService.GeneratePresignedUploadUrl(objectName)
            }; // Default is success

            return GetActionResponse(response);
        }

    }
}
