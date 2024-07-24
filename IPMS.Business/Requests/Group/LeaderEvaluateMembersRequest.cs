namespace IPMS.Business.Requests.Group
{
    public class LeaderEvaluateMembersRequest
    {
        public IList<MemberContribute> Members { get; set; } = new List<MemberContribute>();
    }

    public class MemberContribute
    {
        public Guid MemberId { get; set; }
        public decimal Percentage { get; set; }
    }
}
