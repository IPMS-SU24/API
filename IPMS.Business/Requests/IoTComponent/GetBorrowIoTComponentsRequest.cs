using IPMS.Business.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Requests.IoTComponent
{
    public class GetBorrowIoTComponentsRequest : BasePaginationRequest
    {
        public bool OrderBy { get; set; } = true;
        public List<Guid> ClassIds { get; set; } = new();
    }
}
