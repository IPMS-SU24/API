using IPMS.DataAccess.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Requests.MemberHistory
{
    public class UpdateRequestStatusRequest
    {
        public Guid Id { get; set; }
        public Guid ReviewId { get; set; }
        public RequestStatus Status { get; set; }
        public string Type { get; set; }
    }
}
