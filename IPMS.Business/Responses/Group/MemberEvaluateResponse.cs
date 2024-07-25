namespace IPMS.Business.Responses.Group
{
    public class MemberEvaluateResponse
    {
        public Guid MemberId { get; set; }
        public decimal? LeaderSetPercentage { get; set; }
        public decimal? LecturerSetPercentage { get; set; }
        public string Name { get; set; }
    }
}
