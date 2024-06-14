using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Microsoft.Extensions.Primitives;

namespace IPMS.Business.Common.AWS
{
    public class AmazonSecretsManagerConfigurationProvider : ConfigurationProvider
    {
        private readonly string _region;
        private readonly string _secretName;

        public AmazonSecretsManagerConfigurationProvider(string region, string secretName)
        {
            _region = region;
            _secretName = secretName;
        }
        public AmazonSecretsManagerConfigurationProvider(
string region, string secretName, PeriodicWatcher watcher)
        {
            _region = region;
            _secretName = secretName;

            ChangeToken.OnChange(() => watcher.Watch(), Load);

        }
        public override void Load()
        {
            var secret = GetSecret();
            // Load JSON string into a MemoryStream
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(secret);
                writer.Flush();
                stream.Position = 0;
                Data = new ConfigurationBuilder().AddJsonStream(stream).Build().AsEnumerable().ToDictionary(x => x.Key, x => x.Value);
            }
        }

        private string GetSecret()
        {
            var request = new GetSecretValueRequest
            {
                SecretId = _secretName,
                VersionStage = "AWSCURRENT" // VersionStage defaults to AWSCURRENT if unspecified.
            };

            using var client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));
            var response = client.GetSecretValueAsync(request).GetAwaiter().GetResult();

            string secretString;
            if (response.SecretString != null)
            {
                secretString = response.SecretString;
            }
            else
            {
                var memoryStream = response.SecretBinary;
                var reader = new StreamReader(memoryStream);
                secretString =Encoding.UTF8.GetString(Convert.FromBase64String(reader.ReadToEnd()));
            }

            return secretString;
        }
    }
}
