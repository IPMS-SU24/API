using IPMS.DataAccess.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.DataAccess.Models
{
    [Owned]
    public class UserRefreshToken : BaseModel
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.Now >= Expires;
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Revoked { get; set; }
        public bool IsActive => Revoked == null && !IsExpired;
    }
}
