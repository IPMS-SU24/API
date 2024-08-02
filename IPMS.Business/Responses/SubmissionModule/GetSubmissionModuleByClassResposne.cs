using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.SubmissionModule
{
    public class GetAssessmentSubmissionModuleByClassResponse
    {
        public Guid AssessmentId { get; set; }
        public string Title { get; set; }
        /*public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }*/
        public decimal Percentage { get; set; }
        public List<ProjectSubmissionModule> Modules { get; set; } = new();
    }

    public class ProjectSubmissionModule
    {
        public Guid ModuleId { get; set; }
        public string Title { get; set; }
        public int Graded { get; set; }
        public int Submissions { get; set; }
        public int Total { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Percentage { get; set; }
        public string Description { get; set; }
    }
}
