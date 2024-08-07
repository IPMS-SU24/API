using System.ComponentModel.DataAnnotations;
namespace IPMS.Business.Models
{
    public class ClassDataRow
    {
        [Required]
        [MaxLength(50)]
        public string ClassCode { get; set; } = null!;
        [Required]
        [EmailAddress]
        public string LecturerEmail { get; set; } = null!;
    }
}
