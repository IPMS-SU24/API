namespace IPMS.Business.Responses.Class
{
    public class MemberInGroupResponse
    {
        public int TotalMember { get; set; }
        public IQueryable<MemberInGroupData> MemberInfo { get; set; }
    }
    public class MemberInGroupData
    {
        public Guid Id { get; set; }
        public string GroupName { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
    }
}
