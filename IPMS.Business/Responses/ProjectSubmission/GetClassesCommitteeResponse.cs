using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.ProjectSubmission
{
    public class GetClassesCommitteeResponse
    {
        public Guid ClassId { get; set; }
        public int GroupNum { get; set; }
        public string ClassCode { get; set; }
        public string ClassName { get; set; }
        public int StudentNum { get; set; }
    }
}
