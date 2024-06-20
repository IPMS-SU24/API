using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.ProjectSubmission
{
    public class ProjectSubmissionResponse
    {
        public Guid Id { get; set; }
        public DateTime? SubmitTime { get; set; }
        public string Link { get; set; }
        public string Name { get; set; }

    }
}
