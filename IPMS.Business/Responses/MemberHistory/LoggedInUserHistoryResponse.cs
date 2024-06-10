using IPMS.DataAccess.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.MemberHistory
{
    public class LoggedInUserHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid LeaderId { get; set; }
        public string Type { get; set; }
        public GeneralObjectInformation Requester { get; set; }
        public GeneralObjectInformation? MemberSwap { get; set; }
        public GeneralObjectInformation? ProjectFrom { get; set; }
        public GeneralObjectInformation ProjectTo { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Approved;
        public DateTime CreateAt { get; set; }
    }
}
