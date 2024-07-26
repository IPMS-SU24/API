
namespace IPMS.Business.Responses.Class
{
    public class GetClassDetailResponse
    {
        public Guid Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Lecturer { get; set; }
        public string Semester { get; set; }
        public int NumOfStudents { get; set; }
    }
}
