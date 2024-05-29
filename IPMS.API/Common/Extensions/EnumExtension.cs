using IPMS.API.Common.Enums;
using System.ComponentModel;

namespace IPMS.API.Common.Extensions
{
    public static class EnumExtension
    {
        public static string GetEnumDescription(this Enum responseStatus)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])responseStatus
           .GetType()
           .GetField(responseStatus.ToString())
           !.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }
    }
}
