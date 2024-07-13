namespace IPMS.Business.Responses.Semester
{
    public class GetLecturerInSemesterResponse
    {
        public IEnumerable<LecturerInfo> Lecturers { get; set; } = new List<LecturerInfo>();
    }

    public class LecturerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
