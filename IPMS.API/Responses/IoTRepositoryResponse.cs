using IPMS.Business.Responses.IoT;

namespace IPMS.API.Responses
{
    public class IoTRepositoryResponse : PaginationResponse<IEnumerable<GetIoTRepositoryResponse>>
    {
        public int TotalComponents { get; set; }
    }
}
