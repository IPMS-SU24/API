using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.DataAccess.Common.Enums
{
    public enum RequestStatus
    {
        Pending = 0,
        Waiting = 1,
        Rejected = 2, 
        Approved = 3,
    }
}
