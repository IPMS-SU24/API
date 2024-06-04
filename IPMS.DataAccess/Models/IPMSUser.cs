using Microsoft.AspNetCore.Identity;

namespace IPMS.DataAccess.Models
{
    public class IPMSUser : IdentityUser<Guid>
    {
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Project> OwnProjects { get; set; } = new List<Project>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<SubmissionModule> Modules { get; set; } = new List<SubmissionModule>();
        public virtual ICollection<Topic> OwnTopics { get; set; } = new List<Topic>();
        public virtual ICollection<ProjectSubmission> ProjectSubmissions { get; set; } = new List<ProjectSubmission>();
        public string FullName {  get; set; }
        public virtual ICollection<UserRefreshToken> RefreshTokens { get; set; } = new List<UserRefreshToken>();
        public bool IsDeleted { get; set; } = false;
    }
}
