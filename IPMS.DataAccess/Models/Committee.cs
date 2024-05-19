using IPMS.DataAccess.CommonModels;
using System;
namespace IPMS.DataAccess.Models
{
    public class Committee : BaseModel
    {
        public decimal Percentage { get; set; }
        public Guid ClassId { get; set; }
        public Guid LecturerId { get; set; }
        public virtual IPMSClass Class { get; set; }
        public virtual IPMSUser Lecturer { get; set; }
        public virtual ICollection<LecturerGrade> Grades { get; set; } = new List<LecturerGrade>();

    }
}
