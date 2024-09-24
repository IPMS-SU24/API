using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.Kit;
using IPMS.Business.Requests.KitProject;
using IPMS.Business.Requests.ProjectKit;
using IPMS.Business.Responses.Kit;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class KitProjectService : IKitProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _context;
        public KitProjectService(IUnitOfWork unitOfWork, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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
                if (isProjectExisted == true)
                {
                    var isInProject = await _unitOfWork.StudentRepository.Get()
                        .Where(x => x.Id.Equals(request.StudentId)
                                && x.ProjectId.Equals(request.ProjectId))
                        .FirstOrDefaultAsync();
                    if (isInProject != null)
                    {
                        await _unitOfWork.KitProjectRepository.InsertAsync(new KitProject
                        {
                            ProjectId = request.ProjectId,
                            KitId = request.KitId,
                            BorrowedDate = DateTime.Now,
                            Comment = request.Comment,
                            BorrowerId = request.StudentId
                        });
                    }
                    else
                    {
                        throw new DataNotFoundException("Student not found!");
                    }
                    await _unitOfWork.SaveChangesAsync();
                    break;
                }
            }
        }

        public async Task<List<GetAllKitProjectResponse>> GetAllKitProject(GetAllKitProjectRequest request)
        {
            var kitProjectsRaw = _unitOfWork.KitProjectRepository.Get()
                        .Include(x => x.Borrower)
                        .ThenInclude(y => y.Information)
                        .Include(x => x.Returner)
                        .ThenInclude(y => y.Information)
                        .Include(x => x.Project)
                        .ThenInclude(x => x.Students)
                        .ThenInclude(x => x.Class)
                        .Include(x => x.Kit).AsQueryable();
            if (request.SemesterId != null)
            {
                var classes = await _unitOfWork.IPMSClassRepository.Get().Where(x => x.SemesterId.Equals(request.SemesterId)).Include(x => x.Students).ToListAsync();

                var projectsId = classes.SelectMany(x => x.Students.Where(y => y.ProjectId != null).Select(y => y.ProjectId!.Value)).ToList();
                kitProjectsRaw = kitProjectsRaw.Where(x => projectsId.Contains(x.ProjectId));
            }

            if (request.ClassId != null)
            {
                var classes = await _unitOfWork.IPMSClassRepository.Get().Where(x => x.Id.Equals(request.ClassId)).Include(x => x.Students).ToListAsync();

                var projectsId = classes.SelectMany(x => x.Students.Where(y => y.ProjectId != null).Select(y => y.ProjectId!.Value)).ToList();
                kitProjectsRaw = kitProjectsRaw.Where(x => projectsId.Contains(x.ProjectId));
            }

            if (request.ProjectId != null)
            {
                kitProjectsRaw = kitProjectsRaw.Where(x => x.ProjectId.Equals(request.ProjectId));
            }
            return await kitProjectsRaw.Select(kp => new GetAllKitProjectResponse
            {
                Id = kp.Id,
                BorrowedDate = kp.BorrowedDate,
                ReturnedDate = kp.ReturnedDate,
                Comment = kp.Comment,
                ProjectId = kp.ProjectId,
                KitId = kp.KitId,
                KitName = kp.Kit.Name,
                ProjectNum = kp.Project.GroupNum,
                ClassId = kp.Project.Students.First().ClassId,
                ClassName = kp.Project.Students.First().Class.ShortName,
                BorrowerId = kp.BorrowerId,
                BorrowerName = kp.Borrower.Information.FullName,
                BorrowerEmail = kp.Borrower.Information.Email,
                ReturnerName = kp.ReturnerId != null ? kp.Returner.Information.FullName : null,
                ReturnerEmail = kp.ReturnerId != null ? kp.Returner.Information.Email : null
                
            }).ToListAsync();
        }

        public async Task<List<GetKitProjectStudentResponse>> GetAllKitProjectStudent(GetKitProjectStudentRequest request)
        {
            var kitsProject = new List<GetKitProjectStudentResponse>();
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");
            if (project == null)
            {
                return kitsProject;
            }
            kitsProject = await _unitOfWork.KitProjectRepository.Get()
                .Where(x => x.ProjectId.Equals(project.Id))
                .Include(x => x.Kit).ThenInclude(y => y.Devices).ThenInclude(z => z.Device)
                .Include(x => x.Borrower).ThenInclude(y => y.Information)
                .Include(x => x.Returner).ThenInclude(y => y.Information)
                .Select(x => new GetKitProjectStudentResponse
                {
                    BorrowedDate = x.BorrowedDate,
                    ReturnedDate = x.ReturnedDate,
                    BorrowerName = x.Borrower.Information.FullName,
                    ReturnerName = x.ReturnerId != null ? x.Returner.Information.FullName : null,
                    Comment = x.Comment,
                    KitName = x.Kit.Name,
                    KitDescription = x.Kit.Description,
                    Devices = x.Kit.Devices.Select(y => new KitDeviceResponse
                    {
                        Name = y.Device.Name,
                        Quantity = y.Quantity
                    }).ToList()
                }).ToListAsync();
            return kitsProject;
        }

        public async Task UpdateKitProject(UpdateKitProjectRequest request)
        {
            var kitProject = await _unitOfWork.KitProjectRepository.Get().FirstOrDefaultAsync(x => x.Id.Equals(request.Id));
            if (kitProject == null)
            {
                throw new DataNotFoundException("Request borrow not found");
            }
            var isInProject = await _unitOfWork.StudentRepository.Get()
                        .Where(x => x.Id.Equals(request.StudentId)
                                && x.ProjectId.Equals(kitProject.ProjectId)).FirstOrDefaultAsync();

            if (isInProject == null)
            {
                throw new DataNotFoundException("Student not found!");
            }

            kitProject.Comment = request.Comment;
            kitProject.ReturnedDate = DateTime.Now;
            kitProject.ReturnerId = request.StudentId;
            _unitOfWork.KitProjectRepository.Update(kitProject);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
