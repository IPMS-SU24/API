using IPMS.Business.Responses.Profile;

namespace IPMS.Business.Interfaces.Services
{
    public interface IProfileService
    {
        Task<ProfileResponse> GetProfile(Guid studentId);
    }
}
