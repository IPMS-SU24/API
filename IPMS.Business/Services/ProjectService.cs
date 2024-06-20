using AutoMapper;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.ProjectPreference;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.Business.Responses.ProjectPreference;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace IPMS.Business.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMapper _mapper;
        private readonly IPresignedUrlService _presignedUrlService;
        private readonly UserManager<IPMSUser> _userManager;

        public ProjectService(IUnitOfWork unitOfWork, ICommonServices commonServices,IMapper mapper, IPresignedUrlService presignedUrlService, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _mapper = mapper;
            _presignedUrlService = presignedUrlService;
            _userManager = userManager;
        }

        public async Task<string?> GetProjectName(Guid currentUserId)
        {
            return (await _commonServices.GetProject(currentUserId))?.GroupName;
        }

        public async Task<IEnumerable<ProjectPreferenceResponse>> GetProjectPreferences(ProjectPreferenceRequest request)
        {
            var prjPref = new List<ProjectPreferenceResponse>();
            IQueryable<Project> prjQueryable = _unitOfWork.ProjectRepository.Get().Where(p => p.IsPublished == true)
                            .Include(p => p.Topic).ThenInclude(t => t.Topic)
                            .Include(p => p.Topic).ThenInclude(t => t.Class).ThenInclude(c => c.Semester)
                            .Include(p => p.Submissions);

            if (request.SearchValue != null) // search value base on topic name or description
            {
                request.SearchValue = request.SearchValue.Trim().ToLower();
                prjQueryable = prjQueryable.Where(p => p.Topic.Topic.Name.ToLower().Contains(request.SearchValue)
                                                        || p.Topic.Topic.Description.ToLower().Contains(request.SearchValue));
            }

            if (request.LecturerId != null && request.LecturerId != Guid.Empty) // search base on lecturerId
            {
                prjQueryable = prjQueryable.Where(p => p.Topic.Class.LecturerId.Equals(request.LecturerId));
            }

            if (request.SemesterCode != null) // search base on semester code - semester shortname
            {
                request.SemesterCode = request.SemesterCode.Trim().ToLower();
                prjQueryable = prjQueryable.Where(p => p.Topic.Class.Semester.ShortName.ToLower().Contains(request.SemesterCode));

            }

            var projects = await prjQueryable.ToListAsync();

            List<IPMSUser> users = _userManager.Users.ToList();

            prjPref = projects.Select(p => new ProjectPreferenceResponse
            {
                TopicTitle = p.Topic.Topic.Name != null ? p.Topic.Topic.Name : "",
                LecturerId = p.Topic.Class.LecturerId != Guid.Empty ?  p.Topic.Class.LecturerId : Guid.Empty,
                LecturerName = GetLecturerName(users, p.Topic.Class.LecturerId),
                Semester = p.Topic.Class.Semester.Name != null ? p.Topic.Class.Semester.Name : "",
                SemesterCode = p.Topic.Class.Semester.ShortName != null ? p.Topic.Class.Semester.ShortName : "",
                Description = p.Topic.Topic.Description != null ? p.Topic.Topic.Description : "",
                ProjectSubmissions = p.Submissions.Select(ps => new ProjectSubmissionResponse
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    SubmitTime = ps.SubmissionDate,
                    Link = _presignedUrlService.GeneratePresignedDownloadUrl("PS_" + ps.Id + "_" + ps.Name) //Get base on name on S3 
                }).ToList()
            }).ToList();

            return prjPref;
        }

        private string GetLecturerName(List<IPMSUser> users, Guid? lecturerId)
        {
            if (lecturerId == null) return "";
            if (lecturerId == Guid.Empty) return "";

            var lecturer = users.FirstOrDefault(u => u.Id.Equals(lecturerId));
            if (lecturer == null)
            {
                return "";
            }
            return lecturer.FullName;
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
                    AssessmentStatus = _commonServices.GetChangeTopicStatus(topic,@class.ChangeTopicDeadline.Value,@class.ChangeGroupDeadline.Value)
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
            var mapAssessmentInformationTasks = new List<Task<AssessmentInformation>>();
            foreach (var assessment in currentSemester.Syllabus!.Assessments)
            {

                var submissionsOfAssessment = new List<ProjectSubmission>();
                var isHaveSubmission = submissions.TryGetValue(assessment.Id, out var assessmentSubmissions);
                if (isHaveSubmission)
                {
                    submissionsOfAssessment = assessmentSubmissions!.ToList();
                }
                mapAssessmentInformationTasks.Add(MapAssessmentInformation(assessment, submissionsOfAssessment));
            }
            response.Assessments.AddRange(await Task.WhenAll(mapAssessmentInformationTasks));
            return response;
        }
        private async Task<AssessmentInformation> MapAssessmentInformation(Assessment assessment, List<ProjectSubmission> submissionsOfAssessment)
        {
            var assessmentDeadline = (await _commonServices.GetAssessmentTime(assessment.Id));
            return new AssessmentInformation
            {
                Name = assessment.Name ?? string.Empty,
                Description = assessment.Description ?? string.Empty,
                Order = assessment.Order,
                Percentage = assessment.Percentage,
                EndDate = assessmentDeadline.endDate,
                StartDate = assessmentDeadline.startDate,
                Id = assessment.Id,
                AssessmentStatus = await _commonServices.GetAssessmentStatus(assessment.Id, submissionsOfAssessment)
            };
        }
    }
}
