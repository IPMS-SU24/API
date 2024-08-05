using IPMS.DataAccess.Common.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace IPMS.DataAccess.Models
{
    public class ClassModuleDeadline : BaseModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid SubmissionModuleId { get; set; }
        public SubmissionModule SubmissionModule { get; set; }
        public Guid ClassId { get; set; }
        public IPMSClass Class { get; set; }
    }
}
