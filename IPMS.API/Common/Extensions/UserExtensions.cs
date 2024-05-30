using System.Security.Claims;

namespace IPMS.API.Common.Extensions
{
    public static class UserExtensions
    {
        public static Guid GetUserId(this IEnumerable<Claim> Claims)
        {
            return new Guid(Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
        }
    }
}
