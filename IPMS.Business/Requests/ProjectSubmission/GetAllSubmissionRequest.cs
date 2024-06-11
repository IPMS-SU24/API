using IPMS.Business.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IPMS.Business.Requests.ProjectSubmission
{
    public class GetAllSubmissionRequest: BasePaginationAutoFiltererRequest
    {
        public string? SearchValue { get; set; } = "";
        public Guid? SubmitterId { get; set; }
        public Guid? AssessmentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

    }
}
