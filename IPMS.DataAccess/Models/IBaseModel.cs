namespace IPMS.DataAccess.Models
{
    public interface IBaseModel
    {
        Guid Id { get; set; }
        bool IsDeleted { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime LastModified { get; set; }
    }
}
