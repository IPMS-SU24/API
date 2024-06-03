using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using System;
using System.Collections.Generic;

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
        public async Task<(DateTime StartDate, DateTime EndDate)> GetAssessmentTime(Guid assessmentId, IUnitOfWork unitOfWork)
        {
            var assessmentModules = await unitOfWork.SubmissionModuleRepository.Get().Where(x => x.AssessmentId == assessmentId).ToListAsync();
            return new()
            {
                StartDate = assessmentModules.Max(x => x.StartDate),
                EndDate = assessmentModules.Max(x => x.StartDate)
            };
        }

        public async Task<Project?> GetProject(Guid currentUserId)
        {

            Guid currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester.Id;

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

        public async Task<Topic?> GetProjectTopic(Guid projectId)
        {
            return await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId == projectId)
                                                            .Include(x => x.Topic).Select(x => x.Topic).FirstOrDefaultAsync();
        }

        public async Task<AssessmentStatus> GetAssessmentStatus(Guid assessmentId, IEnumerable<ProjectSubmission> submissionList)
        {
            var now = DateTime.Now;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var modules = currentSemester!.Modules.Where(x => x.AssessmentId == assessmentId);
            var assessment = currentSemester.Syllabus!.Assessments.FirstOrDefault(x => x.Id == assessmentId);
            //Check Assessment Deadline, Start Date
            var deadline = modules.MaxBy(x => x.EndDate)!.EndDate;
            var start = modules.MinBy(x => x.StartDate)!.StartDate;
            //Case 1: Start Time in the future => status NotYet
            if (start > now)
            {
                return AssessmentStatus.NotYet;
            }
            //Case 2: Deadline in the future, Start time in the past => status InProgress
            if (start <= now && deadline > now)
            {
                return AssessmentStatus.InProgress;
            }
            //Case 3: Deadline in the past
            //Case 3.1: All module is submitted
            if (submissionList.Count() == assessment.Modules.Count)
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
            var isBorrowed = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents().Where(x => x.MasterId == projectId).AnyAsync();
            if (isBorrowed && @class.BorrowIoTComponentDeadline < now) return AssessmentStatus.Done;
            if (@class.BorrowIoTComponentDeadline > now && @class.ChangeTopicDeadline < now) return AssessmentStatus.InProgress;
            if (!isBorrowed && @class.BorrowIoTComponentDeadline > now) return AssessmentStatus.Expired;
            return AssessmentStatus.NotYet;
        }

        public async Task<int> GetRemainComponentQuantityOfLecturer(Guid lecturerId, Guid componentId)
        {
            //Get all project of lecturerId
            var allProject = await GetAllCurrentProjectsOfLecturer(lecturerId);
            //Get all quantity of component of Lecturer
            var quantity = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents().Where(x=>x.Status == BorrowedStatus.Approved && allProject.Contains(x.MasterId!.Value) && x.ComponentId == componentId).SumAsync(x=>x.Quantity);
            var lecturerQuantity = await _unitOfWork.ComponentsMasterRepository.GetLecturerOwnComponents().Where(x=>x.MasterId == lecturerId && x.ComponentId == componentId).SumAsync(x=>x.Quantity);
            return lecturerQuantity - quantity;
        }

        public async Task<List<Guid>> GetAllCurrentProjectsOfLecturer(Guid lecturerId)
        {
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            return await _unitOfWork.IPMSClassRepository.Get().Where(x => x.SemesterId == currentSemester.Id && x.LecturerId == lecturerId)
                                                                                .Join(_unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId != null),
                                                                                      @class => @class.Id,
                                                                                      classTopic => classTopic.ClassId,
                                                                                      (@class, classTopic) => classTopic.ProjectId.Value).ToListAsync();
        }
    }
}
