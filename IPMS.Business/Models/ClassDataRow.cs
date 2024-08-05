using System.ComponentModel.DataAnnotations;
namespace IPMS.Business.Models
{
    public class ClassDataRow
    {
        [MaxLength(50)]
        public string ClassCode { get; set; } = null!;
        public string LecturerAccount { get; set; }
    }
}
