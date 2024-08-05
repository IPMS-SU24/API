using IPMS.Business.Pagination;
using IPMS.DataAccess.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Requests.Topic
{
    public class GetSuggestedTopicsLecturerRequest : BasePaginationRequest
    {
        public Guid? ClassId { get; set; }
        public List<RequestStatus> Statuses { get; set; } = new();
       
    }
}
