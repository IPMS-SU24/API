using System.ComponentModel;

namespace IPMS.API.Common.Enums
{
    public enum ResponseStatus
    {
        [Description("Request Complete Successfully")]
        Success,
        [Description("Invalid Request")]
        BadRequest,
        [Description("Not Found Any Data")]
        DataNotFound,
        [Description("Unauthenticated")]
        Unauthorized,
        [Description("You don't have the permission to do the request")]
        Forbidden,
        [Description("Somethings wen't wrong")]
        Fail
    }
}
