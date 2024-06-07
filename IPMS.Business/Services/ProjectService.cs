using AutoMapper;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMapper _mapper;

        public ProjectService(IUnitOfWork unitOfWork, ICommonServices commonServices,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _mapper = mapper;
        }

        public async Task<string?> GetProjectName(Guid currentUserId)
        {
            return (await _commonServices.GetProject(currentUserId))?.GroupName;
        }

        public async Task<ProjectProgressData> GetProjectProgressData(Guid currentUserId)
        {
            var project = await _commonServices.GetProject(currentUserId);
            if (project == null) throw new DataNotFoundException("Not found Project");
            var topic = await _commonServices.GetProjectTopic(project.Id);
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var studiesIn = await _commonServices.GetStudiesIn(currentUserId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x=>x.ClassId), currentSemester.Id);
            var componentBorrowed = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents().Where(x => x.MasterId == project.Id).Include(x => x.Component).ToListAsync();
            var response = new ProjectProgressData
            {
                ProjectName = project!.GroupName ?? string.Empty,
                ProjectId = project!.Id,
                TopicInfo = new()
                {
                    TopicId = topic?.Id,
                    TopicName = topic?.Name ?? string.Empty,
                    Description = topic?.Description ?? string.Empty,
                    EndDate = @class.ChangeTopicDeadline,
                    AssessmentStatus = _commonServices.GetChangeTopicStatus(topic,@class.ChangeTopicDeadline,@class.ChangeGroupDeadline)
                },
                BorrowInfo = new()
                {
                    EndDate = @class.BorrowIoTComponentDeadline,
                    AssessmentStatus = await _commonServices.GetBorrowIoTStatus(currentUserId, @class),
                    IoTComponents = _mapper.Map<List<BorrowIoTComponentInformation>>(componentBorrowed)
                }
            };
            var projectSubmissions = await _commonServices.GetProjectSubmissions(project.Id);
            var submissions = projectSubmissions.GroupBy(x => (Guid)x.SubmissionModule!.AssessmentId!).ToDictionary(x => x.Key);
            foreach (var assessment in currentSemester.Syllabus!.Assessments)
            {
                var assessmentDeadline = (await _commonServices.GetAssessmentTime(assessment.Id));
                var assessmentInfo = new AssessmentInformation
                {
                    Name = assessment.Name ?? string.Empty,
                    Description = assessment.Description ?? string.Empty,
                    Order = assessment.Order,
                    Percentage = assessment.Percentage,
                    EndDate = assessmentDeadline.endDate,
                    StartDate = assessmentDeadline.startDate,
                    Id = assessment.Id,
                };

                var submissionsOfAssessment = new List<ProjectSubmission>();
                var isHaveSubmission = submissions.TryGetValue(assessment.Id, out var assessmentSubmissions);
                if (isHaveSubmission)
                {
                    submissionsOfAssessment = assessmentSubmissions!.ToList();
                }
                assessmentInfo.AssessmentStatus = await _commonServices.GetAssessmentStatus(assessment.Id, submissionsOfAssessment);
                response.Assessments.Add(assessmentInfo);
            }
            return response;
        }
    }
}
