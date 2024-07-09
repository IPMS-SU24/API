using Microsoft.AspNetCore.WebUtilities;
using static System.Net.WebRequestMethods;

namespace IPMS.Business.Common.Utils
{
    public static class PathUtils
    {
        private static string confirmEmailPath = "email-confirm";
        public static string GetConfirmURL(Guid userId, string token)
        {
            var url= $"https://portal.i-pma.click/{confirmEmailPath}";
            var queryParams = new Dictionary<string, string>()
            {
                {"userId",userId.ToString()},
                {"token",token},
            };
            return QueryHelpers.AddQueryString(url, queryParams);
        }
    }
}
