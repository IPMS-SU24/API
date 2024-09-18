using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Kit;
using IPMS.Business.Requests.KitProject;
using IPMS.Business.Requests.ProjectKit;
using IPMS.Business.Responses.Kit;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class KitProjectService : IKitProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        public KitProjectService(IUnitOfWork unitOfWork, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
        }

        public async Task CreateKitProject(CreateKitProjectRequest request)
        {
            var kit = await _unitOfWork.IoTKitRepository.Get().Where(x => x.Id.Equals(request.KitId)).Include(x => x.Devices).FirstOrDefaultAsync();
            if (kit == null)
            {
                throw new DataNotFoundException("Kit does not exist!");
            }
            bool isProjectExisted = false;
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            //get project is in semester?
            var classes = await _unitOfWork.IPMSClassRepository.Get().Where(x => x.SemesterId.Equals(currentSemesterId)).Include(x => x.Students).ToListAsync();
            foreach (var @class in classes)
            {
                var projectsId = @class.Students.Select(x => x.ProjectId).Distinct().ToList();
                isProjectExisted = projectsId.Any(x => x.Equals(request.ProjectId));
            }
            if (isProjectExisted == false)
            {
                throw new DataNotFoundException("Project does not exist!");

            }
            await _unitOfWork.KitProjectRepository.InsertAsync(new KitProject
            {
                ProjectId = request.ProjectId,
                KitId = request.KitId,
                BorrowedDate = DateTime.Now,
                Comment = request.Comment
            });
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task<List<GetAllKitProjectResponse>> GetAllKitProject(GetAllKitProjectRequest request)
        {
            var kitProjectsRaw = _unitOfWork.KitProjectRepository.Get().Include(x => x.Project).Include(x => x.Kit);
            return await kitProjectsRaw.Select(kp => new GetAllKitProjectResponse
            {
                Id = kp.Id,
                BorrowedDate = kp.BorrowedDate,
                ReturnedDate = kp.ReturnedDate,
                Comment = kp.Comment,
                ProjectId = kp.ProjectId,
                KitId = kp.KitId,
                KitName = kp.Kit.Name,
                ProjectNum = kp.Project.GroupNum
            }).ToListAsync();
        }

        public async Task UpdateKitProject(UpdateKitProjectRequest request)
        {
            var kitProject = await _unitOfWork.KitProjectRepository.Get().FirstOrDefaultAsync(x => x.Id.Equals(request.Id));
            if (kitProject == null)
            {
                throw new DataNotFoundException("Request borrow not found");
            }
            kitProject.Comment = request.Comment;
            kitProject.ReturnedDate = DateTime.Now;
            _unitOfWork.KitProjectRepository.Update(kitProject);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
