﻿using IPMS.DataAccess.Common.Models;

namespace IPMS.DataAccess.Models
{
    public partial class IPMSClass : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int MaxMember { get; set; } = int.MaxValue;
        public DateTime? CreateGroupDeadline { get; set; }
        public DateTime? ChangeGroupDeadline { get; set; }
        public DateTime? ChangeTopicDeadline { get; set; }
        public DateTime? BorrowIoTComponentDeadline { get; set; }
        public Guid? SemesterId { get; set; }
        public Guid? LecturerId { get; set; }
        public virtual Semester? Semester { get; set; }
        public virtual ICollection<Committee> Committees {get; set;} = new List<Committee>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<ClassTopic> Topics {get; set;} = new List<ClassTopic>();
        public int? JobImportId { get; set; }
    }
}
    