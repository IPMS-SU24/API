using IPMS.Business.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Requests.Topic
{
    public class GetSuggestedTopicsLecturerRequest : BasePaginationRequest
    {
        public Guid ClassId { get; set; }
    }
}
