using Microsoft.AspNetCore.Identity;

namespace IPMS.DataAccess.Models
{
    public class IPMSUser : IdentityUser<Guid>
    {
        public virtual ICollection<IPMSClass> IPMSClasses { get; set; } = new List<IPMSClass>();
        public virtual ICollection<AccountComponent> AccountComponents { get; set; } = new List<AccountComponent>();
        public virtual ICollection<ClassMember> ClassMembers { get; set; } = new List<ClassMember>();
        public virtual ICollection<Committee> Committees { get; set; } = new List<Committee>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<IoTComponent> IoTComponents { get; set; } = new List<IoTComponent>();
        public virtual ICollection<MemberProject> MemberProjects { get; set; } = new List<MemberProject>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<SubmissionModule> SubmissionModules { get; set; } = new List<SubmissionModule>();
        public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    }
}
