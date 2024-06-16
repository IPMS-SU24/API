namespace IPMS.Business.Requests.Class
{
    public class SetMaxMemberRequest
    {
        public IEnumerable<Guid> ClassIds { get; set; } = null!;
        public int MaxMember { get; set; }
    }
}
