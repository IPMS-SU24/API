using IPMS.API.Common.Enums;
using IPMS.Business.Common.Enums;
using Microsoft.AspNetCore.Authorization;

namespace IPMS.API.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class EnumAuthorizeAttribute : AuthorizeAttribute
    {
        public EnumAuthorizeAttribute(params UserRole[] roles)
        {
            this.Roles = string.Join(",", roles.Distinct().Select(r => Enum.GetName(r.GetType(), r)));
        }
    }
}
