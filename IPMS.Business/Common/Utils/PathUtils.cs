using Microsoft.AspNetCore.WebUtilities;
using static System.Net.WebRequestMethods;

namespace IPMS.Business.Common.Utils
{
    public static class PathUtils
    {
        private static string confirmEmailPath = "email-confirm";
        private static string resetPasswordPath = "reset-password";
        private static string clientURL = "https://portal.i-pma.click/";
        public static string GetConfirmURL(Guid userId, string token)
        {
            var url= $"{clientURL}{confirmEmailPath}";
            var queryParams = new Dictionary<string, string>()
            {
                {"userId",userId.ToString()},
                {"token",token},
            };
            return QueryHelpers.AddQueryString(url, queryParams);
        }
        public static string GetResetPasswordURL(Guid userId, string token)
        {
            var url= $"{clientURL}{resetPasswordPath}";
            var queryParams = new Dictionary<string, string>()
            {
                {"userId",userId.ToString()},
                {"token",token},
            };
            return QueryHelpers.AddQueryString(url, queryParams);
        }
    }
}
