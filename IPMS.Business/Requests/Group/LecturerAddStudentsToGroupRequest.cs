namespace IPMS.Business.Requests.Group
{
    public class LecturerAddStudentsToGroupRequest
    {
        public Guid ClassId { get; set; }
        public Guid GroupId { get; set; }
        public IEnumerable<Guid> Students { get; set; } = new List<Guid>();
}
}
