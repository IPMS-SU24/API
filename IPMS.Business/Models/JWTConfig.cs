using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Models
{
    public class JWTConfig
    {
        public string ValidAudience { get; set; } = string.Empty;
        public string ValidIssuer {  get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public double TokenExpiryTimeInHour { get; set; } = 0.25;
        public double RefreshTokenValidityInDays { get; set; } = 7;
    }
}
