using IPMS.Business.Requests.KitProject;
using IPMS.Business.Requests.ProjectKit;

namespace IPMS.Business.Interfaces.Services
{
    public interface IKitProjectService
    {
        Task GetAllProjectKit();
        Task CreateKitProject(CreateKitProjectRequest request);
        Task UpdateKitProject(UpdateKitProjectRequest request);
    }
}
