using Amazon.S3;
using Amazon.S3.Model;
using IPMS.Business.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace IPMS.Business.Services
{
    public class PresignedUrlService : IPresignedUrlService
    {
        private readonly string BUCKET_NAME = "IPMS_BucketName";
        private readonly string EXPIRY = "IPMS_UrlExpiryTimeInHours";
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        public PresignedUrlService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
     
        }
        public string GeneratePresignedDownloadUrl(string objectKey)
        {
            string urlString = string.Empty;
            try
            {
                var request = new GetPreSignedUrlRequest()
                {
                    BucketName = _configuration[BUCKET_NAME],
                    Key = objectKey,
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

        public  string GeneratePresignedUploadUrl(string objectKey)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _configuration[BUCKET_NAME],
                Key = objectKey,
                Verb = HttpVerb.PUT,
                Expires = DateTime.UtcNow.AddHours(double.Parse(_configuration[EXPIRY])),
            };

            string url = _s3Client.GetPreSignedURL(request);
            return url;
        }
    }
}
