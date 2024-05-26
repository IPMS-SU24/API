namespace IPMS.DataAccess.Common.Models
{
    public class BaseModel
    {
        public Guid Id { get; set; } = new Guid();
        public bool IsDeleted { get; set; } = false;
    }
}
