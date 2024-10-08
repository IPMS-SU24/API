﻿using AutoMapper;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Enums;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.Project;
using IPMS.Business.Requests.ProjectPreference;
using IPMS.Business.Responses.Project;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.Business.Responses.ProjectPreference;
using IPMS.Business.Responses.ProjectSubmission;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMapper _mapper;
        private readonly IPresignedUrlService _presignedUrlService;
        private readonly UserManager<IPMSUser> _userManager;

        public ProjectService(IUnitOfWork unitOfWork, ICommonServices commonServices, IMapper mapper, IPresignedUrlService presignedUrlService, UserManager<IPMSUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _mapper = mapper;
            _presignedUrlService = presignedUrlService;
            _userManager = userManager;
        }

        public async Task<string?> GetProjectName(Guid currentUserId)
        {
            return _commonServices.GetProject() != null ? $"Group {_commonServices.GetProject().GroupNum}" : null;
        }

        public async Task<IEnumerable<ProjectPreferenceResponse>> GetProjectPreferences(ProjectPreferenceRequest request)
        {
            var prjPref = new List<ProjectPreferenceResponse>();
            IEnumerable<Project> prjQueryable = await _unitOfWork.ProjectRepository.Get().Where(p => p.IsPublished == true)
                            .Include(p => p.AssessmentTopic).ThenInclude(t => t.Topic)
                            .Include(p => p.AssessmentTopic).ThenInclude(t => t.Class).ThenInclude(c => c.Semester)
                            .Include(p => p.Submissions).ToListAsync();

            if (request.SearchValue != null) // search value base on topic name or description
            {
                request.SearchValue = request.SearchValue.Trim().ToLower();
                prjQueryable = prjQueryable.Where(p => p.Topic != null && (p.Topic.Topic.Name.ToLower().Contains(request.SearchValue)
                                                        || p.Topic.Topic.Description.ToLower().Contains(request.SearchValue)));
            }

            if (request.LecturerId != null && request.LecturerId != Guid.Empty) // search base on lecturerId
            {
                prjQueryable = prjQueryable.Where(p => p.Topic != null && p.Topic.Class.LecturerId.Equals(request.LecturerId));
            }

            if (request.SemesterCode != null) // search base on semester code - semester shortname
            {
                request.SemesterCode = request.SemesterCode.Trim().ToLower();
                prjQueryable = prjQueryable.Where(p => p.Topic != null && p.Topic.Class.Semester.ShortName.ToLower().Contains(request.SemesterCode));

            }

            var projects = prjQueryable.ToList();
            var lastAssessmentId = await _unitOfWork.AssessmentRepository.Get().GroupBy(x => x.SyllabusId)
                .Select(x => x.OrderByDescending(a => a.Order).Select(a => a.Id).First()
                ).ToListAsync();
            var lastSubmissionModuleIds = await _unitOfWork.SubmissionModuleRepository.Get().Where(x => lastAssessmentId.Contains(x.AssessmentId)).Select(x => x.Id).ToListAsync();
            prjPref = projects.Select(p => new ProjectPreferenceResponse
            {
                TopicTitle = p.Topic.Topic.Name != null ? p.Topic.Topic.Name : "",
                LecturerId = p.Topic.Class.LecturerId != Guid.Empty ? p.Topic.Class.LecturerId : Guid.Empty,
                LecturerName = GetLecturerName(p.Topic.Class.LecturerId),
                Semester = p.Topic.Class.Semester.Name != null ? p.Topic.Class.Semester.Name : "",
                SemesterCode = p.Topic.Class.Semester.ShortName != null ? p.Topic.Class.Semester.ShortName : "",
                Description = p.Topic.Topic.Description != null ? p.Topic.Topic.Description : "",
                ProjectSubmissions = p.Submissions.Where(x => lastSubmissionModuleIds.Contains(x.SubmissionModuleId)).Select(ps => new ProjectSubmissionResponse
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    SubmitTime = ps.SubmissionDate,
                    Link = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, ps.Id, ps.Name)) ?? string.Empty //Get base on name on S3 
                }).ToList()
            }).ToList();

            return prjPref;
        }

        private string GetLecturerName(Guid? lecturerId)
        {
            if (lecturerId == null) return "";
            if (lecturerId == Guid.Empty) return "";

            var lecturer = _userManager.Users.FirstOrDefault(u => u.Id.Equals(lecturerId));
            if (lecturer == null)
            {
                return "";
            }
            return lecturer.FullName;
        }
        public async Task<ProjectProgressData> GetProjectProgressData(Guid currentUserId)
        {
            var project = _commonServices.GetProject();
            if (project == null) throw new DataNotFoundException("Not found Project");
            var topic = await _commonServices.GetProjectTopic(project.Id);
            var assessmentInfo = topic != null ? await _unitOfWork.ClassTopicRepository.Get()
                .Where(x => x.ProjectId == project.Id && x.TopicId == topic.Id)
                .Select(x => x.Assessment).FirstOrDefaultAsync() : null;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var @class = _commonServices.GetClass();
            var componentBorrowed = await _unitOfWork.ComponentsMasterRepository.GetBorrowComponents().Where(x => x.MasterId == project.Id).Include(x => x.Component).ToListAsync();
            var response = new ProjectProgressData
            {
                ProjectName = project != null ? $"Group {project.GroupNum}" : string.Empty,
                ProjectId = project!.Id,
                TopicInfo = new()
                {
                    TopicId = topic?.Id,
                    TopicName = topic?.Name ?? string.Empty,
                    Description = topic?.Description ?? string.Empty,
                    EndDate = @class!.ChangeTopicDeadline,
                    AssessmentStatus = _commonServices.GetChangeTopicStatus(topic, @class.ChangeTopicDeadline, @class.ChangeGroupDeadline!.Value),
                    AssessmentId = assessmentInfo?.Id,
                    AssessmentName = assessmentInfo?.Name
                },
                BorrowInfo = new()
                {
                    EndDate = @class.BorrowIoTComponentDeadline,
                    AssessmentStatus = await _commonServices.GetBorrowIoTStatus(currentUserId, @class),
                    IoTComponents = _mapper.Map<List<BorrowIoTComponentInformation>>(componentBorrowed)
                }
            };
            if (topic != null)
            {
                response.TopicInfo.FileLink = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Topic, topic.Id, topic.Detail)) ?? string.Empty;
                response.TopicInfo.Iots = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents().Include(x => x.Component).Where(x => x.MasterId == topic.Id).Select(x => new TopicIoTInfo
                {
                    Id = x.ComponentId,
                    Quantity = x.Quantity,
                    Name = x.Component.Name
                }).ToListAsync();
            }
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
                response.Assessments.Add(await MapAssessmentInformation(assessment, submissionsOfAssessment, currentSemester));
            }
            //response.Assessments.AddRange(await Task.WhenAll(mapAssessmentInformationTasks));
            return response;
        }
        private async Task<AssessmentInformation> MapAssessmentInformation(Assessment assessment, List<ProjectSubmission> submissionsOfAssessment, Semester semester)
        {
            var assessmentDeadline = await _commonServices.GetAssessmentTime(assessment.Id, _commonServices.GetClass()!.Id);
            return new AssessmentInformation
            {
                Name = assessment.Name ?? string.Empty,
                Description = assessment.Description ?? string.Empty,
                Order = assessment.Order,
                Percentage = assessment.Percentage,
                EndDate = assessmentDeadline.endDate,
                StartDate = assessmentDeadline.startDate,
                Id = assessment.Id,
                AssessmentStatus = await _commonServices.GetAssessmentStatus(assessment.Id, submissionsOfAssessment, semester)
            };
        }

        public async Task<IEnumerable<GetProjectsOverviewResponse>> GetProjectsOverview(GetProjectsOverviewRequest request, Guid currentUserId)
        {
            List<GetProjectsOverviewResponse> projectsOverview = new List<GetProjectsOverviewResponse>();
            if (request.ClassId == null || request.ClassId == Guid.Empty)
            {
                return projectsOverview;
            }
            IPMSClass? @class = null;
            if (request.IsCommittee.HasValue && request.IsCommittee.Value)
            {
                @class = await _unitOfWork.CommitteeRepository.Get().Include(x => x.Class)
                    .Where(x => x.LecturerId == currentUserId && x.Class.LecturerId != currentUserId && x.ClassId == request.ClassId).Select(x => x.Class).FirstOrDefaultAsync();
            }
            else
            {
                @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(currentUserId));
            }


            if (@class == null) // check class is existed
            {
                return projectsOverview;
            }

            var classTopicsQuery = _unitOfWork.ClassTopicRepository.Get()
                                                    .Include(ct => ct.Topic)
                                                    .Include(ct => ct.Project).ThenInclude(p => p.Students).ThenInclude(s => s.Information)
                                                    .Where(ct => ct.ClassId.Equals(request.ClassId) && ct.ProjectId != null);
            var semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.Id == @class.SemesterId);
            if (semester.IsMultipleTopic)
            {
                classTopicsQuery = classTopicsQuery.Where(x => x.AssessmentId.HasValue);
            }
            if (semester.IsMultipleTopic)
            {
                classTopicsQuery = classTopicsQuery.Where(x => !x.AssessmentId.HasValue);
            }
            List<ClassTopic> classTopics = await classTopicsQuery.ToListAsync();

            var allLeaders = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList(); // Find leader of project
            if(classTopics != null && classTopics.Count > 0)
            {
                foreach (var classTopic in classTopics.GroupBy(x => x.ProjectId)) // picked topic
                {
                    GetProjectsOverviewResponse prjOverview = new GetProjectsOverviewResponse
                    {
                        Id = (Guid)classTopic.Key!,
                        GroupName = $"Group {classTopic.First().Project!.GroupNum}",
                        Members = classTopic.First().Project.Students.Count(),
                        LeaderName = classTopic.First().Project.Students.FirstOrDefault(s => allLeaders.Contains(s.InformationId))!.Information.FullName,
                        TopicName = classTopic.First().Topic!.Name
                    };
                    projectsOverview.Add(prjOverview);
                }
            }

            var projectNotPick = await GetProjectNotPickedTopic(request.ClassId); // not picked topic
            foreach (var project in projectNotPick)
            {
                GetProjectsOverviewResponse prjOverview = new GetProjectsOverviewResponse
                {
                    Id = (Guid)project.Id!,
                    GroupName = $"Group {project.GroupNum}",
                    Members = project.Students.Count(),
                    LeaderName = project.Students.FirstOrDefault(s => allLeaders.Contains(s.InformationId))!.Information.FullName,
                    TopicName = ""
                };
                projectsOverview.Add(prjOverview);
            }

            return projectsOverview.OrderBy(x => x.GroupName);
        }
        private async Task<IEnumerable<Project>> GetProjectNotPickedTopic(Guid classId)
        {
            List<Project> projects = new List<Project>();
            var @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(x => x.Id == classId);
            var semester = await _unitOfWork.SemesterRepository.Get().FirstOrDefaultAsync(x => x.Id == @class.SemesterId);
            var prjPickedTopicQuery = _unitOfWork.ClassTopicRepository.Get().Where(ct => ct.ClassId.Equals(classId) // get project picked topic 
                                                        && (ct.ProjectId != null));
            if (semester.IsMultipleTopic)
            {
                prjPickedTopicQuery = prjPickedTopicQuery.Where(x => x.AssessmentId.HasValue);
            }
            if (semester.IsMultipleTopic)
            {
                prjPickedTopicQuery = prjPickedTopicQuery.Where(x => !x.AssessmentId.HasValue);
            }
            var prjPickedTopic = await prjPickedTopicQuery
                                                .Select(ct => ct.ProjectId).Distinct().ToListAsync();

            var projectsId = await _unitOfWork.StudentRepository.Get().Where(s => s.ClassId.Equals(classId) // get through project not in project picked topic
                                                    && prjPickedTopic.Contains(s.ProjectId) == false)
                                                .Select(s => s.ProjectId).Distinct()
                                                .ToListAsync();

            projects = await _unitOfWork.ProjectRepository.Get().Where(p => projectsId.Contains(p.Id)).Include(p => p.Students).ThenInclude(s => s.Information).ToListAsync();
            return projects;
        }
        public async Task<GetProjectDetailResponse> GetProjectDetail(GetProjectDetailRequest request, Guid currentUserId)
        {
            GetProjectDetailResponse prjDetail = null;
            if (request.ClassId == null || request.ClassId == Guid.Empty)
            {
                return prjDetail;
            }
            IPMSClass? @class = null;
            if (request.IsCommittee.HasValue && request.IsCommittee.Value)
            {
                @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.Committees.Any(x => x.LecturerId == currentUserId) && c.LecturerId != currentUserId);
            }
            else
            {
                @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(currentUserId));
            }
            if (@class == null) // check class is existed
            {
                return prjDetail;
            }

            if (request.GroupId == null || request.GroupId == Guid.Empty)
            {
                return prjDetail;

            }

            ClassTopic classTopic = await _unitOfWork.ClassTopicRepository.Get()
                                                    .Where(ct => ct.ClassId.Equals(request.ClassId) && ct.ProjectId.Equals(request.GroupId))
                                                    .Include(ct => ct.Topic)
                                                    .Include(ct => ct.Project).ThenInclude(p => p.Students).ThenInclude(s => s.Information)
                                                    .FirstOrDefaultAsync();

            var components = await _unitOfWork.ComponentsMasterRepository.Get()
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project && cm.MasterId.Equals(request.GroupId))
                                    .Include(cm => cm.Component).ToListAsync();

            var iotBorrows = components.GroupBy(cm => new { cm.CreatedAt.Year, cm.CreatedAt.Month, cm.CreatedAt.Day, cm.CreatedAt.Hour, cm.CreatedAt.Minute, cm.CreatedAt.Second})
                .Select(g => new IotBorrow
                {
                    CreateAt = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day, g.Key.Hour, g.Key.Minute, g.Key.Second),
                    Items = g.Select(g => new IotItem
                    {
                        Id = g.Id,
                        Name = g.Component!.Name,
                        Quantity = g.Quantity,
                        Status = g.Status
                    }).ToList()

                }).ToList();

            var allLeaders = (await _userManager.GetUsersInRoleAsync(UserRole.Leader.ToString())).Select(x => x.Id).ToList(); // Find leader of project

            if (classTopic == null) // Expect cho nay viet 1 cai ham de lay project chua pick topic --> Lay 1 list xong where de view group tong quan dung luon
            {
                var projectNotPick = await GetProjectNotPickedTopic(request.ClassId);
                var project = projectNotPick.FirstOrDefault(pnp => pnp.Id.Equals(request.GroupId));
                if (project == null)
                {
                    return prjDetail;

                }
                var memsProject = project.Students.Select(s => new MemberPrjDetail
                {
                    Id = s.InformationId,
                    StudentId = s.Information.UserName,
                    Name = s.Information.FullName,
                    isLeader = allLeaders.Any(l => l.Equals(s.InformationId))
                }).OrderByDescending(x => x.isLeader).ToList();
                prjDetail = new GetProjectDetailResponse
                {
                    GroupName = $"Group {project.GroupNum}",
                    TopicName = "",
                    TopicStatus = RequestStatus.Waiting,
                    Members = memsProject,
                    IotBorrows = iotBorrows
                };

                return prjDetail;
            }

            var members = classTopic.Project.Students.Select(s => new MemberPrjDetail
            {
                Id = s.InformationId,
                StudentId = s.Information.UserName,
                Name = s.Information.FullName,
                isLeader = allLeaders.Any(l => l.Equals(s.InformationId))

            }).OrderByDescending(x => x.isLeader).ToList();

            prjDetail = new GetProjectDetailResponse
            {
                GroupName = $"Group {classTopic.Project.GroupNum}",
                TopicName = classTopic.Topic != null ? classTopic.Topic.Name : "",
                TopicStatus = classTopic.Topic != null ? classTopic.Topic.Status : RequestStatus.Waiting,
                Members = members,
                IotBorrows = iotBorrows
            };

            return prjDetail;
        }
        public async Task<ValidationResultModel> UpdateProjectPreferencesStatusValidators(UpdateProjectPreferenceStatusRequest request, Guid currentUserId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };

            if (request.Projects.Count == 0)
            {
                result.Message = "Did not have any project to update";
                return result;
            }

            var projects = await _unitOfWork.ProjectRepository.Get().Where(p => request.Projects.Select(x => x.ProjectId).Contains(p.Id)
                                                            ).Include(p => p.AssessmentTopic).ThenInclude(t => t.Class).ToListAsync();

            projects = projects.Where(p => p.Topic != null && p.Topic.Class.LecturerId.Equals(currentUserId)).ToList();
            
            if (projects.Count != request.Projects.Count()) // check all request project need to be existed and have Id same lecturer Id updated
            {
                result.Message = "Have project invalid";
                return result;
            }

            var currentSemesterId = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester!.Id;

            var isInSemester = projects.Any(p => p.Topic.Class.SemesterId.Equals(currentSemesterId)); // if have project so that semester was worked or is working -- just check difference with current Semester
            if (isInSemester == true)
            {
                result.Message = "Cannot publish project in current semester";
                return result;
            }

            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task UpdateProjectPreferencesStatus(UpdateProjectPreferenceStatusRequest request, Guid currentUserId)
        {
            var projects = await _unitOfWork.ProjectRepository.Get().Where(p => request.Projects.Select(x => x.ProjectId).Contains(p.Id)).Include(p => p.AssessmentTopic).ThenInclude(x=>x.Class).ToListAsync();
            projects = projects.Where(p => p.Topic != null && p.Topic.Class.LecturerId.Equals(currentUserId)).ToList();
            foreach (var project in projects)
            {
                project.IsPublished = request.Projects.FirstOrDefault(p => p.ProjectId.Equals(project.Id)).IsPublished;
                _unitOfWork.ProjectRepository.Update(project);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProjectPreferenceResponse>> GetProjectPreferencesLecturer(ProjectPreferenceRequest request, Guid currentUserId)
        {
            var prjPref = new List<ProjectPreferenceResponse>();
            IEnumerable<Project> prjQueryable = await _unitOfWork.ProjectRepository.Get().Include(p => p.AssessmentTopic).ThenInclude(t => t.Topic)
                            .Include(p => p.AssessmentTopic).ThenInclude(t => t.Class).ThenInclude(c => c.Semester)
                            .Include(p => p.Submissions).ToListAsync();
            if (request.isPublished == true)
            {
                prjQueryable = prjQueryable.Where(p => p.Topic != null && p.IsPublished == true && p.Topic.Topic.Status == RequestStatus.Approved);
            }
            else
            {
                prjQueryable = prjQueryable.Where(p => p.Topic != null && p.Topic.Class.LecturerId.Equals(currentUserId) && p.IsPublished == false && p.Topic.Topic.Status == RequestStatus.Approved);
            }


            if (request.SearchValue != null) // search value base on topic name or description
            {
                request.SearchValue = request.SearchValue.Trim().ToLower();
                prjQueryable = prjQueryable.Where(p => p.Topic != null && (p.Topic.Topic.Name.ToLower().Contains(request.SearchValue)
                                                        || p.Topic.Topic.Description.ToLower().Contains(request.SearchValue)));
            }

            if (request.LecturerId != null && request.LecturerId != Guid.Empty) // search base on lecturerId
            {
                prjQueryable = prjQueryable.Where(p => p.Topic != null && p.Topic.Class.LecturerId.Equals(request.LecturerId));
            }

            if (request.SemesterCode != null) // search base on semester code - semester shortname
            {
                request.SemesterCode = request.SemesterCode.Trim().ToLower();
                prjQueryable = prjQueryable.Where(p => p.Topic != null && p.Topic.Class.Semester.ShortName.ToLower().Contains(request.SemesterCode));

            }

            var projects = prjQueryable.ToList();
            var lastAssessmentId = await _unitOfWork.AssessmentRepository.Get().Where(x => x.Modules.Any(m => m.LectureId == currentUserId && m.Semester.ShortName.ToLower().Contains(request.SemesterCode)))
                .OrderByDescending(x => x.Order).Select(x => x.Id).FirstOrDefaultAsync();
            var lastSubmissionModuleIds = await _unitOfWork.SubmissionModuleRepository.Get().Where(x => x.AssessmentId == lastAssessmentId).Select(x => x.Id).ToListAsync();
            prjPref = projects.Select(p => new ProjectPreferenceResponse
            {
                ProjectId = p.Id,
                TopicTitle = p.Topic.Topic.Name != null ? p.Topic.Topic.Name : "",
                LecturerId = p.Topic.Class.LecturerId != Guid.Empty ? p.Topic.Class.LecturerId : Guid.Empty,
                LecturerName = GetLecturerName(p.Topic.Class.LecturerId),
                Semester = p.Topic.Class.Semester.Name != null ? p.Topic.Class.Semester.Name : "",
                SemesterCode = p.Topic.Class.Semester.ShortName != null ? p.Topic.Class.Semester.ShortName : "",
                Description = p.Topic.Topic.Description != null ? p.Topic.Topic.Description : "",
                ProjectSubmissions = p.Submissions.Where(x => lastSubmissionModuleIds.Contains(x.SubmissionModuleId)).Select(ps => new ProjectSubmissionResponse
                {
                    Id = ps.Id,
                    Name = ps.Name,
                    SubmitTime = ps.SubmissionDate,
                    Link = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Submission, ps.Id, ps.Name)) ?? string.Empty //Get base on name on S3 
                }).ToList()
            }).ToList();

            return prjPref;
        }
    }
}
