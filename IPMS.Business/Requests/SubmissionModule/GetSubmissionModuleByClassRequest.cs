using IPMS.Business.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Requests.SubmissionModule
{
    public class GetSubmissionModuleByClassRequest : BasePaginationRequest
    {
        public Guid classId { get; set; }
    }
}
