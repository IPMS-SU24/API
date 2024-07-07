namespace IPMS.Business.Responses.Profile
{
    public class ProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? SemesterName { get; set; }
        public string? ClassName { get; set; }
        public string? GroupName { get; set; }
    }
}
