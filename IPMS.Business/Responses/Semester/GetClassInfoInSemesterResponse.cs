namespace IPMS.Business.Responses.Semester
{
    public class GetClassInfoInSemesterResponse
    {
        public IEnumerable<ClassInSemesterInfo> Classes { get; set; } = null!;
    }
    public class ClassInSemesterInfo
    {
        public Guid ClassId { get; set; }
        public string ClassCode { get; set; } = null!;
        public string ClassName { get; set; } = null!;
        public int Total { get; set; }
        public int Enroll { get; set; }
        public int GroupNum { get; set; }
        public int MaxMembers { get; set; }


    }
}
