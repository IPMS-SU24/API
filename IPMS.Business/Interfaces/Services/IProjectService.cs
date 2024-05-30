using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProjectService
    {
        public Task<string?> GetProjectName(Guid currentUserId);
    }
}
