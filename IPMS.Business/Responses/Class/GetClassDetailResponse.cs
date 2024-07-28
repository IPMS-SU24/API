
using Amazon.S3.Model;

namespace IPMS.Business.Responses.Class
{
    public class GetClassDetailResponse
    {
        public Guid Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public Guid LecturerId { get; set; }
        public string Lecturer { get; set; }
        public Guid SemesterId { get; set; }
        public string Semester { get; set; }
        public int NumOfStudents { get; set; }
        public List<CommitteeResponse> Committees { get; set; } = new();
    }

    public class CommitteeResponse
    {
        public Guid CommitteeId { get; set; }
        public string Name { get; set; }
        public decimal Percentage { get; set; }
    }
}
