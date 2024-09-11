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
        [Display(Name = "Student Name")]
        public string StudentName { get; set; }
    }
}
