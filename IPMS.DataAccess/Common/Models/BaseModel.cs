using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.DataAccess.Common.Models
{
    public class BaseModel
    {
        public Guid Id { get; set; } = new Guid();
        public bool IsDeleted { get; set; } = false;
    }
}
