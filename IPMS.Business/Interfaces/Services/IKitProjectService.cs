using IPMS.Business.Requests.Kit;
using IPMS.Business.Requests.KitProject;
using IPMS.Business.Requests.ProjectKit;
using IPMS.Business.Responses.Kit;

namespace IPMS.Business.Interfaces.Services
{
    public interface IKitProjectService
    {
        Task<List<GetAllKitProjectResponse>> GetAllKitProject(GetAllKitProjectRequest request);
        Task<List<GetKitProjectStudentResponse>> GetAllKitProjectStudent(GetKitProjectStudentRequest request);
        Task CreateKitProject(CreateKitProjectRequest request);
        Task UpdateKitProject(UpdateKitProjectRequest request);
    }
}
