using IPMS.Business.Pagination;
using IPMS.Business.Requests.Kit;
using IPMS.Business.Responses.Kit;

namespace IPMS.Business.Interfaces.Services
{
    public interface IKitService
    {
        Task<List<BasicIoTDeviceResponse>> GetAllBasicIoTDevice(GetAllBasicIoTDeviceRequest request);
        Task<BasicIoTDeviceResponse> GetBasicDetail(Guid Id);
        Task CreateBasicIoTDevice(CreateBasicIoTDeviceRequest request);
        Task UpdateBasicIoTDevice(UpdateBasicIoTDeviceRequest request);
        Task DeleteBasicIoTDevice(Guid Id);
        Task<List<KitResponse>> GetAllKit(GetAllKitRequest request);
        Task<KitResponse> GetKitDetail(Guid Id);
        Task CreateKit(CreateKitRequest request);
        Task UpdateKit(UpdateKitRequest request);


    }
}
