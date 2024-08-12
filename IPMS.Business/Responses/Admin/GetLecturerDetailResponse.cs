
namespace IPMS.Business.Responses.Admin
{
    public class GetLecturerDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string Phone{ get; set; }
        public List<ClassInfoLecDetail> Classes { get; set; } = new();
    }
    public class ClassInfoLecDetail
    {
        public Guid ClassId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }
}
