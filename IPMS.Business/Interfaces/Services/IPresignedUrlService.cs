using IPMS.Business.Requests.AWS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface IPresignedUrlService
    {
        public string GeneratePresignedDownloadUrl(string objectKey);
        public string GeneratePresignedUploadUrl(string objectName);
    }
}
