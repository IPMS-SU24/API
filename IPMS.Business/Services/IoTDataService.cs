using AutoFilterer.Extensions;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.IoT;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class IoTDataService : IIoTDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly UserManager<IPMSUser> _userManager;

        public IoTDataService(IUnitOfWork unitOfWork, ICommonServices commonServices, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _userManager = userManager;
        }

        public async Task<ValidationResultModel> AddIoTDeviceValidators(AddIoTDeviceRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task AddIoTDevice(AddIoTDeviceRequest request)
        {
            await _unitOfWork.IoTComponentRepository.InsertAsync(new IoTComponent { Name = request.Name, Description = request.Description == null ? "" : request.Description });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ValidationResultModel> CheckLecturerUpdateIoTValid(UpdateIoTQuantityRequest request, Guid lectuerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Fail to Update"
            };
            var ioTMasters = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().Where(x => x.ComponentId == request.Id && x.MasterId == lectuerId).FirstOrDefaultAsync();
            if (ioTMasters == null) throw new DataNotFoundException();
            var projects = await _commonServices.GetAllCurrentProjectsOfLecturer(lectuerId);
            var ioTBorrowed = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents()
                                                                            .Where(x => projects.Contains(x.MasterId) && x.ComponentId == request.Id)
                                                                            .GroupBy(x => x.ComponentId, x => x.Id,
                                                                            (master, com) =>
                                                                                 com.Count()
                                                                            ).FirstOrDefaultAsync();
            if (ioTBorrowed > request.Quantity)
            {
                result.Message = "Quantity borrowed greater than quantity you want to request";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public IQueryable<GetIoTComponentResponse> GetAll(GetIoTComponentRequest request)
        {
            return _unitOfWork.IoTComponentRepository.Get().ApplyFilter(request).Select(x => new GetIoTComponentResponse
            {
                Description = x.Description,
                Id = x.Id,
                Name = x.Name
            });
        }
        public async Task<(int TotalComponents, IEnumerable<GetIoTRepositoryResponse> info)> GetIoTRepsitoryAsync(GetIoTRepositoryRequest request, Guid lecturerId)
        {
            var lecturerIoTQuery = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().ApplyFilter(request).Include(x => x.Component).Where(x => x.MasterId == lecturerId).Select(x => new
            {
                Id = x.ComponentId,
                TotalQuantity = x.Quantity,
                x.Component.Name
            }).ToListAsync();

            var projectIds = await _commonServices.GetAllCurrentProjectsOfLecturer(lecturerId);
            var groupBorrowQuery = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents().Where(x => x.Status == BorrowedStatus.Approved)
                                .Join(_unitOfWork.StudentRepository.Get()
                                    .Include(x => x.Class).Include(x => x.Project)
                                    .Where(x => x.ProjectId != null && projectIds.Contains(x.ProjectId.Value)),
                                    com => com.MasterId,
                                    stu => stu.ProjectId,
                                    (com, stu) => new
                                    {
                                        Id = com.ComponentId,
                                        ClassCode = stu.Class.ShortName,
                                        GroupNumber = stu.Project!.GroupNum,
                                        BorrowNumber = com.Quantity
                                    }
                                    ).ToListAsync();
            var result = lecturerIoTQuery.Select(lec => new GetIoTRepositoryResponse
            {
                Id = lec.Id,
                Name = lec.Name,
                TotalQuantity = lec.TotalQuantity,
                Components = groupBorrowQuery.Where(x => x.Id == lec.Id).Select(x => new BorrowInGroup
                {
                    ClassCode = x.ClassCode,
                    BorrowNumber = x.BorrowNumber,
                    GroupNumber = x.GroupNumber
                })
            });
            var totalComponents = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().Where(x => x.MasterId == lecturerId).CountAsync();
            return ValueTuple.Create(totalComponents, result);
        }

        public async Task UpdateIoTQuantity(UpdateIoTQuantityRequest request, Guid lecturerId)
        {
            var iotMaster = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents()
                                                    .Where(x => x.MasterId == lecturerId && x.ComponentId == request.Id)
                                                    .FirstOrDefaultAsync();
            iotMaster.Quantity = request.Quantity;
            _unitOfWork.ComponentsMasterRepository.Update(iotMaster);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ValidationResultModel> UpdateIoTDeviceValidators(UpdateIoTDeviceRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            var device = await _unitOfWork.IoTComponentRepository.GetByIDAsync(request.Id);
            if (device == null)
            {
                result.Message = "Device does not exist";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task UpdateIoTDevice(UpdateIoTDeviceRequest request)
        {
            var device = await _unitOfWork.IoTComponentRepository.GetByIDAsync(request.Id);
            device!.Name = request.Name;
            device!.Description = request.Description == null ? "" : request.Description;
            _unitOfWork.IoTComponentRepository.Update(device);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ValidationResultModel> DeleteIoTDeviceValidators(DeleteIoTDeviceRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            var device = await _unitOfWork.IoTComponentRepository.GetByIDAsync(request.Id);
            if (device == null)
            {
                result.Message = "Device does not exist";
                return result;
            }

            var isInTopic = await _unitOfWork.ComponentsMasterRepository.Get().AnyAsync(cm => cm.ComponentId.Equals(request.Id) && cm.MasterType.Equals(ComponentsMasterType.Topic));
            if (isInTopic)
            {
                result.Message = "Cannot delete device in topic";
                return result;

            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task DeleteIoTDevice(DeleteIoTDeviceRequest request)
        {
            var device = await _unitOfWork.IoTComponentRepository.GetByIDAsync(request.Id);
            _unitOfWork.IoTComponentRepository.Delete(device);
            await _unitOfWork.SaveChangesAsync();

        }


    }
}
