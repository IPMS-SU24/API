using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Responses.MemberHistory
{
    public class GeneralObjectInformation
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public RequestStatus? Status { get; set; } // for member swap, project from || to
    }
}
