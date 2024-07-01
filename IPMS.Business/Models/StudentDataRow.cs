using System.ComponentModel.DataAnnotations;

namespace IPMS.Business.Common.Models
{
    public class StudentDataRow
    {
        public string StudentId { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string StudentName { get; set; }
    }
}
