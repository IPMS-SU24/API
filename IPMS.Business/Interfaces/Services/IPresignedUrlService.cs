﻿

namespace IPMS.Business.Interfaces.Services
{
    public interface IPresignedUrlService
    {
        public string GeneratePresignedDownloadUrl(string objectKey);
        public string GeneratePresignedUploadUrl(string objectName);
    }
}