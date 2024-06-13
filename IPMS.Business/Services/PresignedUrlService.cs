using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using IPMS.Business.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Security.AccessControl;

namespace IPMS.Business.Services
{
    public class PresignedUrlService : IPresignedUrlService
    {
        private readonly string BUCKET_NAME = "BucketName";
        private readonly string EXPIRY = "UrlExpiryTimeInHours";
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        public PresignedUrlService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
     
        }
        public string GeneratePresignedDownloadUrl(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return null;
            string urlString = string.Empty;
            try
            {
                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = _configuration[BUCKET_NAME],
                    Key = objectName,
                    Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration[EXPIRY])),

                };
                urlString = _s3Client.GetPreSignedURL(request);
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error:'{ex.Message}'");
            }

            return urlString;
        }

        public  string GeneratePresignedUploadUrl(string objectName)
        {
            if (string.IsNullOrEmpty(objectName))
                return null;
            AWSConfigsS3.UseSignatureVersion4 = true;
            var requestS3 = new GetPreSignedUrlRequest
            {
                BucketName = _configuration[BUCKET_NAME],
                Key = objectName,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration[EXPIRY])),
            };

            string url = _s3Client.GetPreSignedURL(requestS3);
            return url;
        }
    }
}
