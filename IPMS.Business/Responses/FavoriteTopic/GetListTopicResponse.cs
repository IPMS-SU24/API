using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Responses.FavoriteTopic
{
    public class GetListTopicResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<FavTopicInfo> favTopicInfos { get; set; } = new();
    }

    public class FavTopicInfo
    {
        public Guid? TopicId { get; set; }
        public string TopicName { get; set; }
    }
}
