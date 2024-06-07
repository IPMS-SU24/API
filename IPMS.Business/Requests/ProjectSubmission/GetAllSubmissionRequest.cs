using IPMS.Business.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IPMS.Business.Requests.ProjectSubmission
{
    public class GetAllSubmissionRequest: BasePaginationAutoFiltererRequest
    {
        public string searchValue { get; set; } = "";
        public Guid? submitterId { get; set; }
        public Guid? assessmentId { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

    }
}
