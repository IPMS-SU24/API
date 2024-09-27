using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Kit;
using IPMS.Business.Responses.Kit;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class KitService : IKitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        public KitService(IUnitOfWork unitOfWork, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
        }

        public async Task CreateBasicIoTDevice(CreateBasicIoTDeviceRequest request)
        {
            BasicIoTDevice newIot = new BasicIoTDevice
            {
                Name = request.Name,
                Description = request.Description
            };
            await _unitOfWork.BasicIoTDeviceRepository.InsertAsync(newIot);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteBasicIoTDevice(Guid Id)
        {
            var isExisted = await _unitOfWork.BasicIoTDeviceRepository.Get().Where(x => x.Id.Equals(Id)).FirstOrDefaultAsync();
            if (isExisted == null)
            {
                throw new DataNotFoundException("Iot Device not found");
            }
            var isInKit = await _unitOfWork.KitDeviceRepository.Get().Where(x => x.KitId.Equals(Id)).CountAsync();
            if (isInKit > 0)
            {
                throw new BaseBadRequestException("Iot Device is in Kit");
            }
            _unitOfWork.BasicIoTDeviceRepository.Delete(isExisted);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<BasicIoTDeviceResponse>> GetAllBasicIoTDevice(GetAllBasicIoTDeviceRequest request)
        {
            return await _unitOfWork.BasicIoTDeviceRepository.Get().Select(x => new BasicIoTDeviceResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }).ToListAsync();
        }

        public async Task<List<KitResponse>> GetAllKit(GetAllKitRequest request)
        {
            var kitsRaw = _unitOfWork.IoTKitRepository.Get().Include(x => x.Devices).ThenInclude(x => x.Device);
            var kits = await kitsRaw.Select(x => new KitResponse
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Devices = x.Devices.Select(x => new DeviceInformation
                {
                    Id = x.DeviceId,
                    Name = x.Device.Name,
                    Description = x.Device.Description
                }).ToList() 
            }).ToListAsync();
            return kits;
        }
        public async Task<KitResponse> GetKitDetail(Guid Id)
        {
            var kitRaw = await _unitOfWork.IoTKitRepository.Get().Where(x => x.Id.Equals(Id)).Include(x => x.Devices).ThenInclude(x => x.Device).FirstOrDefaultAsync();
            if (kitRaw != null)
            {
                return new KitResponse
                {
                    Id = kitRaw.Id,
                    Name = kitRaw.Name,
                    Description = kitRaw.Description,
                    Devices = kitRaw.Devices.Select(x => new DeviceInformation
                    {
                        Id = x.DeviceId,
                        Name = x.Device.Name,
                        Description = x.Device.Description,
                        Quantity = x.Quantity
                    }).ToList()
                };
            }
            return null;
          
        }
        public async Task CreateKit(CreateKitRequest request)
        {
            foreach (var device in request.Devices)
            {
                var isExisted = await _unitOfWork.BasicIoTDeviceRepository.Get().FirstOrDefaultAsync(x => x.Id.Equals(device.DeviceId));
                if (isExisted == null)
                {
                    throw new DataNotFoundException("Device does not exist!");
                }
            }
            var newKit = new IoTKit
            {
                Name = request.Name,
                Description = request.Description
            };
            await _unitOfWork.IoTKitRepository.InsertAsync(newKit);
            await _unitOfWork.SaveChangesAsync();
            var kitDevices = request.Devices.Select(x => new KitDevice
            {
                KitId = newKit.Id,
                DeviceId = x.DeviceId,
                Quantity = Int32.Parse(x.Quantity)
            });
            await _unitOfWork.KitDeviceRepository.InsertRangeAsync(kitDevices);
            await _unitOfWork.SaveChangesAsync();
        }
        
        public async Task UpdateKit(UpdateKitRequest request)
        {
           
            var kit = await _unitOfWork.IoTKitRepository.Get().Where(x => x.Id.Equals(request.Id)).Include(x => x.Devices).FirstOrDefaultAsync();
            if (kit == null)
            {
                throw new DataNotFoundException("Kit does not exist!");
            }

            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            var isKitBorrowed = await _unitOfWork.KitProjectRepository.Get().FirstOrDefaultAsync(kp => kp.KitId.Equals(request.Id));
            if (isKitBorrowed != null)
            {
                throw new DataNotFoundException("Cannot update kit is borrowed");

            }

            foreach (var device in request.Devices)
            {
                var isExisted = await _unitOfWork.BasicIoTDeviceRepository.Get().FirstOrDefaultAsync(x => x.Id.Equals(device.DeviceId));
                if (isExisted == null)
                {
                    throw new DataNotFoundException("Device does not exist!");
                }
            }

            kit.Name = request.Name;
            kit.Description = request.Description;
            _unitOfWork.IoTKitRepository.Update(kit);
            _unitOfWork.KitDeviceRepository.DeleteRange(kit.Devices);
            await _unitOfWork.SaveChangesAsync();
            var kitDevices = request.Devices.Select(x => new KitDevice
            {
                KitId = kit.Id,
                DeviceId = x.DeviceId,
                Quantity = Int32.Parse(x.Quantity)
            });
            await _unitOfWork.KitDeviceRepository.InsertRangeAsync(kitDevices);
            await _unitOfWork.SaveChangesAsync();

        }
        public async Task UpdateBasicIoTDevice(UpdateBasicIoTDeviceRequest request)
        {
            var device = await _unitOfWork.BasicIoTDeviceRepository.Get().Where(x => x.Id.Equals(request.Id)).FirstOrDefaultAsync();
            if (device == null) {
                throw new DataNotFoundException("Device does not exist!");
            }
            device.Name = request.Name;
            device.Description = request.Description;
            _unitOfWork.BasicIoTDeviceRepository.Update(device);
            await _unitOfWork.SaveChangesAsync();

        }
        public async Task<BasicIoTDeviceResponse> GetBasicDetail(Guid Id)
        {
            var detail = await _unitOfWork.BasicIoTDeviceRepository.Get().Where(bd => bd.Id.Equals(Id)).FirstOrDefaultAsync();
            if (detail != null)
            {
                return new BasicIoTDeviceResponse
                {
                    Id = detail.Id,
                    Name = detail.Name,
                    Description = detail.Description
                };
            }
            return null;
        }
    }
}
