
using IPMS.Business.Common.AWS;
using Microsoft.Extensions.Configuration;

namespace IPMS.Business.Common.Extensions
{
    public static class AWSExtensions
    {
        public static void AddAmazonSecretsManager(this IConfigurationBuilder configurationBuilder,
                        string region,
                        string secretName)
        {
            var configurationSource =
                    new AmazonSecretsManagerConfigurationSource(region, secretName);

            configurationBuilder.Add(configurationSource);
        }
    }
}
