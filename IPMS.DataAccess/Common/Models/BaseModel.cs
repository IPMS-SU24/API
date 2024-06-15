﻿using System.ComponentModel.DataAnnotations.Schema;

namespace IPMS.DataAccess.Common.Models
{
    public class BaseModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDeleted { get; set; } = false;
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }
    }
}
