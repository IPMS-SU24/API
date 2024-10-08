﻿using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public class ClassTopic : BaseModel
    {
        public Guid ClassId { get; set; }
        public Guid TopicId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AssessmentId { get; set; }
        public virtual IPMSClass Class { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual Project? Project { get; set; }
        
        public virtual Assessment? Assessment { get; set; }
    }
}
