using IPMS.API.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace IPMS.API.Common.Attributes
{
    public class IPMSAuthorizeAttribute : AuthorizeAttribute
    {
        public IPMSAuthorizeAttribute(params UserRole[] roles) : base()
        {
            if (roles.Any(r => r.GetType().BaseType != typeof(Enum)))
                throw new ArgumentException("roles");

            this.Roles = string.Join(",", roles.Select(r => Enum.GetName(r.GetType(), r)));
        }
    }
}
