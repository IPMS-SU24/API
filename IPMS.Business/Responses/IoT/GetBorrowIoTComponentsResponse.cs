
using IPMS.Business.Responses.Project;
using System.Text.Json.Serialization;

namespace IPMS.Business.Responses.IoT
{
    public class GetBorrowIoTComponentsResponse 
    {
        public Guid? ProjectId { get; set; }
        public string ClassName { get; set; }
        public string GroupName { get; set; }
        public DateTime CreateAt { get; set; }
        public List<IotItem> Items { get; set; } = new();
    }

    public class ProjectInformation
    {
        public Guid? ProjectId { get; set; }
        public string ClassName { get; set; }
        public string GroupName { get; set; }
        [JsonIgnore]
        public Guid? TopicId { get; set; }
    }




}
