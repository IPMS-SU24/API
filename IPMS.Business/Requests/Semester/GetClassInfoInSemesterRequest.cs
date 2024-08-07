namespace IPMS.Business.Requests.Semester
{
    public class GetClassInfoInSemesterRequest
    {
        public string SemesterCode { get; set; } = null!;
        public bool? IsCommittee { get; set; } = null!;
    }
}
