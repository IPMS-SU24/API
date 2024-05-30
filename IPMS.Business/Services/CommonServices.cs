using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPMS.Business.Services
{
    public class CommonServices : ICommonServices
    {
        private readonly IUnitOfWork _unitOfWork;
        public CommonServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<Student>> GetStudiesIn(Guid currentUserId)
        {
            return await _unitOfWork.StudentRepository.Get() // Find Student from current User 
                                                       .Where(s => s.InformationId.Equals(currentUserId)).ToListAsync();
        }

        public async Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn, Guid currentSemesterId)
        {
            return await _unitOfWork.IPMSClassRepository.Get() // Get class that student learned and find in current semester
                                                                      .FirstOrDefaultAsync(c => studiesIn.Contains(c.Id)
                                                                      && c.SemesterId.Equals(currentSemesterId));
        }
    }
}
