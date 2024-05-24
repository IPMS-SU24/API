using System.ComponentModel;

namespace IPMS.API.Common.Enums
{
    public enum ResponseStatus
    {
        [Description("Request Complete Successfully")]
        Success = 200,
        [Description("Invalid Request")]
        BadRequest = 400,
        [Description("Not Found Any Data")]
        DataNotFound = 404,
        [Description("Unauthenticated")]
        Unauthorized = 401,
        [Description("You don't have the permission to do the request")]
        Forbidden = 403,
        [Description("Somethings wen't wrong")]
        Fail = 500
    }
}
