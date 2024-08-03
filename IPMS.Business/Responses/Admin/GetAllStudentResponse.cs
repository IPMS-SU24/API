
namespace IPMS.Business.Responses.Admin
{
    public class GetAllStudentResponse
    {
        public Guid Id { get; set; } 
        public string StudentId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public Guid? ClassId { get; set; }
        public string Class { get; set; }
        public Guid? ProjectId { get; set; }
        public string Project { get; set; }
    }
}
