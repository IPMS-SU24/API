namespace IPMS.Business.Responses.Semester
{
    public class GetAllSemestersResponse
    {
        public IEnumerable<SemesterInfo> Semesters { get; set; }
    }
    public class SemesterInfo
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsCurrent { get; set; }
    }
}
