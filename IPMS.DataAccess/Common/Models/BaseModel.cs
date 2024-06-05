namespace IPMS.DataAccess.Common.Models
{
    public class BaseModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDeleted { get; set; } = false;
    }
}
