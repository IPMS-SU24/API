using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class CommonServices : ICommonServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _context;
        public CommonServices(IUnitOfWork unitOfWork, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
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
        public async Task<IPMSClass?> GetCurrentClass(IEnumerable<Guid> studiesIn)
        {
            var currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            return await GetCurrentClass(studiesIn, currentSemesterId);
        }

        public async Task<Project?> GetProject(Guid currentUserId)
        {
            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            var studiesIn = (await GetStudiesIn(currentUserId)).ToList();

            if (studiesIn.Count() == 0 || studiesIn == null)
                return null;

            Guid? currentClassId = (await GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemesterId))?.Id;

            if (currentClassId == null) // Check null current user did not enrolled any class this semester
                return null;

            var currentStudyIn = studiesIn.FirstOrDefault(s => s.ClassId.Equals(currentClassId)); // Get current studying
            if (currentStudyIn == null)
                return null;

            var project = await _unitOfWork.ProjectRepository.Get().FirstOrDefaultAsync(p => p.Id.Equals(currentStudyIn.ProjectId)); // get current project

            return project;
        }

        public async Task<Project?> GetProject(Guid currentUserId, Guid currentClassId)
        {
            var currentStudyIn = await _unitOfWork.StudentRepository.Get()
                        .Where(s => s.ClassId.Equals(currentClassId) && s.InformationId.Equals(currentUserId))
                        .Include(s => s.Project)
                        .FirstOrDefaultAsync(); // Get current studying

            if (currentStudyIn == null)
                return null;

            return currentStudyIn.Project;
        }

        public async Task<Topic?> GetProjectTopic(Guid projectId)
        {
            
            var result = await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId == projectId)
                                                        .Include(x => x.Topic).ToListAsync();
            if (result == null || !result.Any()) return null;

            //single topic
            if(result.Count == 1 && !result.First().AssessmentId.HasValue)
            {
                return result.First().Topic;
            }

            // multiple topic -> get current assessment topic
            if(result.All(x => x.AssessmentId.HasValue))
            {
                var classId = result.First().ClassId;
                var now = DateTime.Now;
                var currentAssessment = await _unitOfWork.ClassModuleDeadlineRepository.Get()
                                                                .Where(x => result.Select(x => x.AssessmentId).Contains(x.SubmissionModule.AssessmentId) && x.ClassId == classId)
                                                                .GroupBy(x=>x.SubmissionModule.AssessmentId)
                                                                .Select(x=> new
                                                                {
                                                                    StartDate = x.Min(x=>x.StartDate),
                                                                    EndDate = x.Max(x=>x.EndDate),
                                                                    x.First().SubmissionModule.AssessmentId,
                                                                })
                                                                .FirstOrDefaultAsync(x=>x.StartDate < now && x.EndDate > now);

                return result.First(x=>x.AssessmentId!.Value == currentAssessment!.AssessmentId).Topic;
            }
            return null;
        }

        public async Task<AssessmentStatus> GetAssessmentStatus(Guid assessmentId, IEnumerable<ProjectSubmission> submissionList)
        {
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            return await GetAssessmentStatus(assessmentId, submissionList, currentSemester!);
        }
        public async Task<AssessmentStatus> GetAssessmentStatus(Guid assessmentId, IEnumerable<ProjectSubmission> submissionList, Semester semester)
        {
            var now = DateTime.Now;
            var @class = GetClass();
            var time = await GetAssessmentTime(assessmentId, @class!.Id);
            //Case 1: Start Time in the future => status NotYet
            if (time.startDate > now)
            {
                return AssessmentStatus.NotYet;
            }
            //Case 2: Deadline in the future, Start time in the past => status InProgress
            if (time.startDate <= now && time.endDate > now)
            {
                return AssessmentStatus.InProgress;
            }
            //Case 3: Deadline in the past
            //Case 3.1: All module is submitted
            var submissionModuleCount = await _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.AssessmentId == assessmentId
                                                                                    && x.SemesterId == semester.Id
                                                                                    && x.LectureId == @class.LecturerId).CountAsync();
            if (submissionList.Count() == submissionModuleCount)
            {
                return AssessmentStatus.Done;
            }
            //Case 3.2: At least one module have not submitted yet => status Expired
            return AssessmentStatus.Expired;
        }

        public async Task<IEnumerable<ProjectSubmission>> GetProjectSubmissions(Guid projectId)
        {
            return await _unitOfWork.ProjectSubmissionRepository
                                                    .Get().Where(x => x.ProjectId == projectId)
                                                    .Include(x => x.SubmissionModule).ToListAsync();
        }

        public async Task<AssessmentStatus> GetBorrowIoTStatus(Guid projectId, IPMSClass @class)
        {
            var now = DateTime.Now;
            if (!@class.BorrowIoTComponentDeadline.HasValue) return AssessmentStatus.InProgress;
            var isBorrowed = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents().Where(x => x.MasterId == projectId).AnyAsync();
            if (isBorrowed && @class.BorrowIoTComponentDeadline < now) return AssessmentStatus.Done;
            if (@class.BorrowIoTComponentDeadline > now && @class.ChangeTopicDeadline < now) return AssessmentStatus.InProgress;
            if (!isBorrowed && @class.BorrowIoTComponentDeadline < now) return AssessmentStatus.Expired;
            return AssessmentStatus.NotYet;
        }

        public async Task<int> GetRemainComponentQuantityOfLecturer(Guid lecturerId, Guid componentId)
        {
            //Get all quantity of component of Lecturer
            var lecturerQuantity = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().Where(x => x.MasterId == lecturerId && x.ComponentId == componentId).SumAsync(x => x.Quantity);
            return lecturerQuantity;
        }

        public async Task<List<Guid>> GetAllCurrentProjectsOfLecturer(Guid lecturerId)
        {
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            return await _unitOfWork.IPMSClassRepository.Get().Where(x => x.SemesterId == currentSemester!.Id && x.LecturerId == lecturerId)
                                                                                .Join(_unitOfWork.StudentRepository.Get().Where(x => x.ProjectId != null),
                                                                                      @class => @class.Id,
                                                                                      stu => stu.ClassId,
                                                                                      (@class, stu) => stu.ProjectId.Value).Distinct().ToListAsync();
        }

        public async Task<(DateTime startDate, DateTime endDate)> GetAssessmentTime(Guid assessmentId, Guid classId)
        {
            var modules = _unitOfWork.ClassModuleDeadlineRepository.Get()
                                                                .Where(x => x.SubmissionModule.AssessmentId == assessmentId && x.ClassId == classId);

            //Check Assessment Deadline, Start Date
            if (!await modules.AnyAsync()) throw new DataNotFoundException("Not Found Module for Assessment");
            var deadline = await modules.MaxAsync(x => x.EndDate);
            var start = await modules.MinAsync(x => x.StartDate);
            return new()
            {
                startDate = start,
                endDate = deadline
            };
        }

        public AssessmentStatus GetChangeTopicStatus(Topic? topic, DateTime? changeTopicDeadline, DateTime changeGroupDeadline)
        {
            
            var now = DateTime.Now;
            if (!changeTopicDeadline.HasValue) return AssessmentStatus.NotAvailable;
            //changeGroupDeadline be startDate
            //changeTopicDeadline be endDate
            if (changeGroupDeadline > now) return AssessmentStatus.NotYet;
            //changeGroupDeadline <= now implicit
            if (changeTopicDeadline > now) return AssessmentStatus.InProgress;
            //changeTopicDeadline <= now implicit
            if (topic != null) return AssessmentStatus.Done;
            return AssessmentStatus.Expired;
        }

        public async Task<IPMSClass?> GetCurrentClass(Guid studentId)
        {
            var studiesIn = await GetStudiesIn(studentId);
            return await GetCurrentClass(studiesIn.Select(x => x.ClassId));
        }

        public async Task SetCommonSessionUserEntity(Guid currentUserId)
        {
            Guid currentSemesterId = Guid.Empty;
            try
            {
                currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;
            }
            catch (NoCurrentSemesterException)
            {
                return;
            }
            var studiesIn = await _unitOfWork.StudentRepository.Get() // Find Student from current User 
                                                       .Where(s => s.InformationId.Equals(currentUserId)).ToListAsync();

            if (studiesIn.Count() != 0)
            {
                IPMSClass? currentClass = await _unitOfWork.IPMSClassRepository.Get() // Get class that student learned and find in current semester
                                                                      .FirstOrDefaultAsync(c => studiesIn.Select(si => si.ClassId).Contains(c.Id)
                                                                      && c.SemesterId.Equals(currentSemesterId));
                if (currentClass != null)
                {
                    _context.HttpContext.Session.SetObject("Class", currentClass);
                    var currentStudyIn = studiesIn.FirstOrDefault(s => s.ClassId.Equals(currentClass.Id)); // Get current studying
                    if (currentStudyIn != null)
                    {
                        var project = await _unitOfWork.ProjectRepository.Get().FirstOrDefaultAsync(p => p.Id.Equals(currentStudyIn.ProjectId)); // get current project
                        if (project != null)
                        {
                            _context.HttpContext.Session.SetObject("Project", project);
                        }
                    }
                }
            }
        }

        public IPMSClass? GetClass()
        {
            return _context.HttpContext.Session.GetObject<IPMSClass?>("Class");
        }

        public Project? GetProject()
        {
            return _context.HttpContext.Session.GetObject<Project?>("Project");
        }

        public async Task<List<IPMSClass>> GetAllCurrentClassesOfLecturer(Guid lecturerId)
        {
            return (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Classes.Where(x => x.LecturerId == lecturerId).ToList();
        }

        public async Task<List<KitProject>> GetKitProjectSemester(Guid semesterId, Guid kitId)
        {
            List<KitProject> projects = new List<KitProject>();
            var kitProjectsRaw = await _unitOfWork.KitProjectRepository.Get().ToListAsync();
            var classes = await _unitOfWork.IPMSClassRepository.Get().Where(x => x.SemesterId.Equals(semesterId)).Include(x => x.Students).ToListAsync();
            foreach (var @class in classes)
            {
                var projectsId = @class.Students.Select(x => x.ProjectId).Distinct().ToList();
                projects.AddRange(kitProjectsRaw.Where(x => projectsId.Contains(x.ProjectId) && x.Id.Equals(kitId)).ToList());
            }
            return projects;
        }
    }
}
