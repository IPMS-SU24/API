using IPMS.DataAccess.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPMS.DataAccess.Common.Models
{
    public abstract class BaseModel : IBaseModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool? IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
    }
}
