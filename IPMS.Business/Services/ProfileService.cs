using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.Profile;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IPMSUser> _userManager;
        private readonly ICommonServices _commonServices;

        public ProfileService(IUnitOfWork unitOfWork, UserManager<IPMSUser> userManager, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _commonServices = commonServices;
        }
        public async Task<ProfileResponse> GetProfile(Guid studentId)
        {
            var user = await _userManager.FindByIdAsync(studentId.ToString());
            var @class = _commonServices.GetClass();
            string? semesterName = null;
            if(@class != null)
            {
                semesterName = await _unitOfWork.IPMSClassRepository.Get().Include(x => x.Semester).Where(x => x.Id == @class.Id).Select(x => x.Semester!.ShortName).FirstOrDefaultAsync();
            }
            return new ProfileResponse
            {
                Id = studentId,
                Email = user.Email,
                ClassName = @class?.ShortName,
                SemesterName = semesterName,
                GroupName = $"{_commonServices.GetProject()?.GroupNum}"
            };
        }
    }
}
