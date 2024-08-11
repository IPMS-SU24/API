using System.ComponentModel.DataAnnotations;

namespace IPMS.Business.Common.Models
{
    public class StudentDataRow
    {
        [Required]
        public string StudentId { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string StudentName { get; set; }
    }
}
