﻿using IPMS.Business.Requests.IoTComponent;

namespace IPMS.Business.Requests.Topic
{
    public class LecturerRegisterTopicRequest
    {
        public Guid Id { get; set; }
        public string TopicName { get; set; } = null!;
        public string ShortName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? FileName { get; set; }
        public ICollection<IoTModelRequest> IoTComponents { get; set; } = null!;
    }
}
