using System;
namespace IPMS.DataAccess.Models
{
    public class Committee
    {
        public decimal Percentage { get; set; }
        public Guid ClassId { get; set; }
        public Guid LecturerId { get; set; }
        public virtual IPMSClass Class { get; set; }
        public virtual IPMSUser Lecturer { get; set; }

    }
}
