using Microsoft.AspNetCore.WebUtilities;

namespace IPMS.Business.Common.Utils
{
    public static class PathUtils
    {
        private static string confirmEmailPath = "api/v1/authentication/email-confirmation";
        public static string GetConfirmURL(string serverDomain, Guid userId, string token)
        {
            var url= $"https://{serverDomain}/{confirmEmailPath}";
            var queryParams = new Dictionary<string, string>()
            {
                {"userId",userId.ToString()},
                {"token",token},
            };
            return QueryHelpers.AddQueryString(url, queryParams);
        }
    }
}
