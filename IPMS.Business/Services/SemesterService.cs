using AutoFilterer.Extensions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Semester;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SemesterService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IQueryable<Semester> GetSemesters(GetSemesterRequest request)
        {
            return _unitOfWork.SemesterRepository.Get().ApplyFilter(request).AsNoTracking();
        }
    }
}
