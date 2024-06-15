using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Semester;
using IPMS.Business.Common.Exceptions;
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

        public async Task<IEnumerable<GetAllSemestersResponse>> GetAllSemesters()
        {
            var response =  await _unitOfWork.SemesterRepository.Get().Select(x=>new GetAllSemestersResponse
            {
                Code = x.ShortName,
                Name = x.Name
            }).ToListAsync();
            if(response == null || !response.Any())
            {
                throw new DataNotFoundException();
            }
            return response;
        }
    }
}
