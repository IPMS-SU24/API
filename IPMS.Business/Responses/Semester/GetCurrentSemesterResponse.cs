namespace IPMS.Business.Responses.Semester
{
    public class GetCurrentSemesterResponse
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
