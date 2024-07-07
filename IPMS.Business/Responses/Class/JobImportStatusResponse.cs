using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.Class
{
    public class JobImportStatusResponse
    {
        public bool IsDone { get; set; } = false;
        public List<JobImportStatusRecord> States { get; set; } = new List<JobImportStatusRecord> { };
    }
    public class JobImportStatusRecord
    {
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string JobStatus { get; set; }
    }
}
