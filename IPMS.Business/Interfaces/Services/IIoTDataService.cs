using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.IoT;

namespace IPMS.Business.Interfaces.Services
{
    public interface IIoTDataService
    {
        IQueryable<GetIoTComponentResponse> GetAll(GetIoTComponentRequest request);
    }
}
