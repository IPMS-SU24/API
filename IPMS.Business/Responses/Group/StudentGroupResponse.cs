namespace IPMS.Business.Responses.Group
{
    public class StudentGroupResponse
    {
        public ClassInfo Class { get; set; } = null!;
        public Guid? GroupJoinRequest { get; set; }
        public Guid? MemberSwapRequest { get; set; }

        public ClassLecturerInfo Lecturer { get; set; } = null!;

        public ICollection<ClassGroupInfo> Groups { get; set; } = new List<ClassGroupInfo>();
        public GroupDeadlineInfo GroupDeadline { get; set; }
    }

    public class ClassInfo
    {
        public string Semester { get; set; } = null!;
        public string ClassCode { get; set; } = null!;
        public int MaxMembers { get; set; }
    }
    public class ClassLecturerInfo
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
    }
    public class ClassGroupInfo
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; } = null!;
        public ICollection<GroupMemberInfo> Members { get; set; } = null!;
        public bool? IsYourGroup { get; set; }
    }
    public class GroupMemberInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool? IsLeader { get; set; }
    }

    public class GroupDeadlineInfo
    {
        public DateTime? CreateGroupDeadline { get; set; }
        public DateTime? ChangeGroupDeadline { get; set; }
    }
}
