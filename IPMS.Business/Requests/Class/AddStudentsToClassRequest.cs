namespace IPMS.Business.Requests.Class
{
    public class AddStudentsToClassRequest
    {
        public Guid ClassId { get; set; }
        public string FileName { get; set; }
        public bool? IsOverwrite { get; set; } = false;
    }
}
