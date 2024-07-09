using AutoFilterer.Extensions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.IoT;
using IPMS.DataAccess.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class IoTDataService : IIoTDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;

        public IoTDataService(IUnitOfWork unitOfWork, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
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
            var lecturerIoTQuery = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().ApplyFilter(request).Include(x=>x.Component).Where(x => x.MasterId == lecturerId).Select(x => new
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
                                            ClassCode = stu.Class.Name,
                                            GroupNumber = stu.Project!.GroupNum,
                                            BorrowNumber = com.Quantity
                                        }
                                    ).ToListAsync();
            var result = lecturerIoTQuery.Select(lec => new GetIoTRepositoryResponse
                                                {
                                                    Id = lec.Id.Value,
                                                    Name = lec.Name,
                                                    TotalQuantity = lec.TotalQuantity,
                                                    Components = groupBorrowQuery.Where(x=>x.Id.Value == lec.Id.Value).Select(x => new BorrowInGroup
                                                    {
                                                        ClassCode = x.ClassCode,
                                                        BorrowNumber = x.BorrowNumber,
                                                        GroupNumber = x.GroupNumber
                                                    })
                                                });
            var totalComponents = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().Where(x => x.MasterId == lecturerId).CountAsync();
            return ValueTuple.Create(totalComponents, result);
        }
    }
}
