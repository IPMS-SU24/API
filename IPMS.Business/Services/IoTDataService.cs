using AutoFilterer.Extensions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.IoT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Services
{
    public class IoTDataService : IIoTDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        public IoTDataService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<GetIoTComponentResponse> GetAll(GetIoTComponentRequest request)
        {
            return _unitOfWork.IoTComponentRepository.Get().ApplyFilter(request).Select(x=> new GetIoTComponentResponse
            {
                Description = x.Description,
                Id = x.Id,
                Name = x.Name
            });
        }
    }
}
