using System;
using IPMS.DataAccess.CommonModels;

namespace IPMS.DataAccess.Models
{
    public class ClassMember : BaseModel
    {
        public Guid StudentId { get; set; }
        public Guid ClassId { get; set; }
        public virtual IPMSUser Student { get; set; }
        public virtual IPMSClass Class { get; set; }
    }
}
