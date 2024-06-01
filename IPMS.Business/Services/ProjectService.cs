using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;

        public ProjectService(IUnitOfWork unitOfWork, ICommonServices commonServices)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
        }

        public async Task<string?> GetProjectName(Guid currentUserId)
        {
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester.Id;

            var studiesIn = (await _commonServices.GetStudiesIn(currentUserId)).ToList();

            if (studiesIn.Count() == 0 || studiesIn == null)
                return null;

            Guid? currentClassId = (await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemesterId))?.Id;
            
            if (currentClassId == null) // Check null current user did not enrolled any class this semester
                return null;

            var currentStudyIn = studiesIn.FirstOrDefault(s => s.ClassId.Equals(currentClassId)); // Get current studying
            if (currentStudyIn == null)
                return null;

            var project = await _unitOfWork.ProjectRepository.Get().FirstOrDefaultAsync(p => p.Id.Equals(currentStudyIn.ProjectId)); // get current project


            return project?.GroupName;
        }
    }
}
