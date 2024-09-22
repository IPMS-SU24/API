namespace IPMS.Business.Responses.Semester
{
    public class GetAllSemestersResponse
    {
        public IEnumerable<SemesterInfo> Semesters { get; set; }
    }
    public class SemesterInfo
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public bool IsCurrent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsMultipleTopic { get; set; }

    }
}
