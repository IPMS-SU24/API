using IPMS.Business.Responses.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.ClassTopic
{
    public class AvailableClassTopicResponse
    {
        public string ProjectName { get; set; }
        public virtual ICollection<TopicIotComponentReponse> TopicComponents { get; set;} = new List<TopicIotComponentReponse>();
    }
}
