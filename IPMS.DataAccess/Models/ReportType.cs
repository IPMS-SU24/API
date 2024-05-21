using IPMS.DataAccess.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.DataAccess.Models
{
    public class ReportType : BaseModel
    {
        public string Name { get; set; }
        public virtual ICollection<Report> Reports { get; set;} = new List<Report>();
    }
}
