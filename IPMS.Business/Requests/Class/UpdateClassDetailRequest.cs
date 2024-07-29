
namespace IPMS.Business.Requests.Class
{
    public class UpdateClassDetailRequest
    {
        public Guid Id { get; set; }
        public Guid LecturerId { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Guid SemesterId { get; set; }
        public List<CommitteeInfo> Committees { get; set; } = new();
    }
    public class CommitteeInfo
    {
        public Guid Id { get; set; }
        public decimal Percentage { get; set; }
    }
}
