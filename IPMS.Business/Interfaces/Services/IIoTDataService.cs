﻿using IPMS.Business.Models;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.IoT;

namespace IPMS.Business.Interfaces.Services
{
    public interface IIoTDataService
    {
        IQueryable<GetIoTComponentResponse> GetAll(GetIoTComponentRequest request);
        Task<(int TotalComponents, IEnumerable<GetIoTRepositoryResponse> info)> GetIoTRepsitoryAsync(GetIoTRepositoryRequest request, Guid lecturerId);
        Task<ValidationResultModel> CheckLecturerUpdateIoTValid(UpdateIoTQuantityRequest request, Guid lectuerId);
        Task UpdateIoTQuantity(UpdateIoTQuantityRequest request, Guid lecturerId);
    }
}
