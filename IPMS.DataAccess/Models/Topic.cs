﻿using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class Topic : BaseModel
    {
        public string? ShortName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid OwnerId { get; set; }
        public virtual IPMSUser Owner { get; set; }
    }
}
