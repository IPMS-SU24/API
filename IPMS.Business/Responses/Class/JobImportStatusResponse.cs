using IPMS.Business.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.Class
{
    public class JobImportStatusResponse<TState>
    {
        public bool IsDone { get; set; } = false;
        public List<TState> States { get; set; } = new List<TState>();
    }
    public class JobImportStudentStatusRecord : IJobImportStatusRecord
    {
        public string StudentName { get; set; }
        public string StudentEmail { get; set; }
        public string JobStatus { get; set; }
    }

    public class JobImportClassStatusRecord
    {
        public string ClassCode { get; set; }
        public string JobStatus
        {
            get
            {
                if(StudentStatus == null)
                {
                    return "Not Yet";
                }
                if (StudentStatus.IsDone)
                {
                    return "Succeeded";
                }
                if(StudentStatus.States.Count() > 0)
                {
                    return "Processing";
                }
                return "Not Yet";
            }
        }
        public JobImportStatusResponse<JobImportStudentStatusRecord>? StudentStatus { get; set; }
    }
}
