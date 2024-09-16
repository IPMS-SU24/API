using IPMS.Business.Requests.Kit;
using IPMS.Business.Responses.Kit;

namespace IPMS.Business.Interfaces.Services
{
    public interface IKitService
    {
        Task<List<BasicIoTDeviceResponse>> GetAllBasicIoTDevice();
        Task CreateBasicIoTDevice(CreateBasicIoTDeviceRequest request);
        //  Task UpdateBasicIoTDevice(CreateBasicIoTDeviceRequest request);
        Task DeleteBasicIoTDevice(Guid Id);
        Task<List<KitResponse>> GetAllKit();
        Task CreateKit(CreateKitRequest request);
        Task UpdateKit(UpdateKitRequest request);


    }
}
