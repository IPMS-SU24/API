using Microsoft.AspNetCore.Identity;

namespace IPMS.DataAccess.Models
{
    public class IPMSUser : IdentityUser<Guid>
    {
       // public Guid Id { get; set; } = new Guid();
       // public virtual ICollection<IPMSClass> Classes { get; set; } = new List<IPMSClass>();
      //  public virtual ICollection<AccountComponent> BorrowedComponents { get; set; } = new List<AccountComponent>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        //public virtual ICollection<IoTComponent> IoTComponents { get; set; } = new List<IoTComponent>();
        public virtual ICollection<Project> OwnProjects { get; set; } = new List<Project>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<SubmissionModule> Modules { get; set; } = new List<SubmissionModule>();
        public virtual ICollection<Topic> OwnTopics { get; set; } = new List<Topic>();
        public string FullName {  get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
