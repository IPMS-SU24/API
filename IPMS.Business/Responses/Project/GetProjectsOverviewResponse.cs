using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.Project
{
    public class GetProjectsOverviewResponse
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; }
        public int Members { get; set; }
        public string LeaderName { get; set; }
        public string TopicName { get; set; }
    }
}
