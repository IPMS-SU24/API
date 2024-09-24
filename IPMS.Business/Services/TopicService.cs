using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.Topic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;
using IPMS.Business.Models;
using AutoMapper;
using IPMS.DataAccess.Common.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Responses.Topic;
using System.Runtime.InteropServices;
using IPMS.Business.Responses.Group;
using MathNet.Numerics.Distributions;
using ZstdSharp.Unsafe;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Utils;
using IPMS.Business.Common.Exceptions;

namespace IPMS.Business.Services
{
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonService;
        private readonly IMapper _mapper;
        private readonly IMessageService _messageService;
        private readonly IHttpContextAccessor _context;
        private readonly IPresignedUrlService _presignedUrlService;

        public TopicService(IUnitOfWork unitOfWork, ICommonServices commonServices,
                    IMapper mapper, IMessageService messageService, IHttpContextAccessor context,
                    IPresignedUrlService presignedUrlService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonServices;
            _mapper = mapper;
            _messageService = messageService;
            _context = context;
            _presignedUrlService = presignedUrlService;
        }

        public async Task<ValidationResultModel> CheckRegisterValid(RegisterTopicRequest request, Guid leaderId)
        {
            var result = new ValidationResultModel
            {
                Message = "Not found IoT Component"
            };
            //Check leader in studying
            var studiesIn = await _commonService.GetStudiesIn(leaderId);
            var @class = await _commonService.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            if (@class == null)
            {
                result.Message = "Student is not studying";
                return result;
            }

            if (@class.ChangeTopicDeadline < DateTime.Now)
            {
                result.Message = "Change Topic Deadline is expired";
                return result;
            }
            //Check leader in project
            var project = _commonService.GetProject();
            if (project == null)
            {
                result.Message = "Student is not in project";
                return result;
            }
            //Check Project have topic
            await _unitOfWork.ProjectRepository.LoadExplicitProperty(project, nameof(Project.Topic));
            if(project.Topic != null)
            {
                result.Message = "Project already has topic";
                return result;
            }
            //Check Short Name exist
            var isShortNameExist = await _unitOfWork.TopicRepository.Get().AnyAsync(x => x.ShortName == request.ShortName);
            if (isShortNameExist)
            {
                result.Message = "Short Name is already exist";
                return result;
            }
            //Check must have IoT
            if (!request.IoTComponents.Any())
            {
                result.Message = "Must have IoT in Topic";
                return result;
            }
            //Check IoT Exist
            var IoTComponents = await _unitOfWork.IoTComponentRepository.Get().Select(x => x.Id).ToListAsync();
            if (IoTComponents != null && IoTComponents.Any())
            {
                foreach (var component in request.IoTComponents)
                {
                    if (!IoTComponents.Contains(component.ComponentId))
                    {
                        return result;
                    }
                    if (component.Quantity <= 0)
                    {
                        result.Message = "IoT Component Quantity Must be Greater Than 0";
                        return result;
                    }
                    if (request.IoTComponents.Count(x => x.ComponentId == component.ComponentId) > 1)
                    {
                        result.Message = "Duplicate IoT Component";
                        return result;
                    }
                }
                result.Message = string.Empty;
                result.Result = true;
                return result;
            }
            return result;
        }

        public IQueryable<Topic> GetAllTopics()
        {
            return _unitOfWork.TopicRepository.GetApprovedTopics();
        }

        public IQueryable<Topic> GetApprovedTopics(GetTopicRequest request)
        {
            return GetAllTopics().ApplyFilter(request).AsNoTracking();
        }

        public async Task<IEnumerable<SuggestedTopicsResponse>> GetSuggestedTopics()
        {
            List<SuggestedTopicsResponse> topics = new List<SuggestedTopicsResponse>();
            List<Topic> preTopics = new List<Topic>();
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");
            if (project == null)
            {
                return topics;
            }
            preTopics = await _unitOfWork.TopicRepository.Get().Where(t => t.SuggesterId.Equals(project.Id)).ToListAsync();
            var components = await _unitOfWork.ComponentsMasterRepository.Get().Where(cm => cm.MasterType == ComponentsMasterType.Topic).Include(cm => cm.Component).ToListAsync();
            foreach (var pre in preTopics)
            {
                var topic = new SuggestedTopicsResponse
                {
                    Id = pre.Id,
                    Title = pre.Name,
                    Description = pre.Description,
                    Detail = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Topic, pre.Id, pre.Detail)),
                    Status = pre.Status,
                    Iots = components.Where(c => c.MasterId.Equals(pre.Id)).Select(c => new TopicIoT
                    {
                        Name = c.Component.Name,
                        Quantity = c.Quantity

                    }).ToList(),
                    CreateAt = pre.CreatedAt,
                    GroupNum = project.GroupNum
                };
                topics.Add(topic);
            }
            return topics;
        }
        public async Task<ValidationResultModel> LecturerRegisterNewTopicValidator(LecturerRegisterTopicRequest request)
        {
            var result = new ValidationResultModel
            {
                Message = "Not found IoT Component"
            };
            //Check must have IoT
            if (!request.IoTComponents.Any())
            {
                result.Message = "Must have IoT in Topic";
                return result;
            }
            //Check IoT Exist
            var IoTComponents = await _unitOfWork.IoTComponentRepository.Get().Select(x => x.Id).ToListAsync();
            if (IoTComponents != null && IoTComponents.Any())
            {
                foreach (var component in request.IoTComponents)
                {
                    if (!IoTComponents.Contains(component.ComponentId))
                    {
                        return result;
                    }
                    if (component.Quantity <= 0)
                    {
                        result.Message = "IoT Component Quantity Must be Greater Than 0";
                        return result;
                    }
                    if (request.IoTComponents.Count(x => x.ComponentId == component.ComponentId) > 1)
                    {
                        result.Message = "Duplicate IoT Component";
                        return result;
                    }
                }
                result.Message = string.Empty;
                result.Result = true;
                return result;
            }
            return result;
        }
        public async Task LecturerRegisterNewTopic(LecturerRegisterTopicRequest request, Guid lecturerId)
        {
            //Create new Topic
            var newTopic = _mapper.Map<Topic>(request);
            newTopic.OwnerId = lecturerId;
            await _unitOfWork.TopicRepository.InsertAsync(newTopic);
            //Add IoT Component to ComponentMaster
            var componentMasters = _mapper.Map<List<ComponentsMaster>>(request.IoTComponents, opts =>
            {
                opts.Items[nameof(ComponentsMaster.MasterId)] = newTopic.Id;
                opts.Items[nameof(ComponentsMaster.MasterType)] = ComponentsMasterType.Topic;
            });
            await _unitOfWork.ComponentsMasterRepository.InsertRangeAsync(componentMasters);

            await _unitOfWork.SaveChangesAsync();

        }

        public async Task RegisterTopic(RegisterTopicRequest request, Guid leaderId)
        {
            //Create new Topic
            var newTopic = _mapper.Map<Topic>(request);
            var @class = _commonService.GetClass();
            newTopic.OwnerId = @class.LecturerId;
            var project = _commonService.GetProject();
            newTopic.SuggesterId = project!.Id;
            await _unitOfWork.TopicRepository.InsertAsync(newTopic);
            //Add IoT Component to ComponentMaster
            var componentMasters = _mapper.Map<List<ComponentsMaster>>(request.IoTComponents, opts =>
            {
                opts.Items[nameof(ComponentsMaster.MasterId)] = newTopic.Id;
                opts.Items[nameof(ComponentsMaster.MasterType)] = ComponentsMasterType.Topic;
            });
            await _unitOfWork.ComponentsMasterRepository.InsertRangeAsync(componentMasters);
            //Release ClassTopic
            var existingClassTopic = await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId == project.Id && x.ClassId == @class.Id).FirstOrDefaultAsync();
            if (existingClassTopic != null)
            {
                existingClassTopic.ProjectId = null;
                _unitOfWork.ClassTopicRepository.Update(existingClassTopic);
            }
            //Create Class Topic
            var newClassTopic = new ClassTopic
            {
                TopicId = newTopic.Id,
                ProjectId = project.Id,
                ClassId = @class.Id
            };
            await _unitOfWork.ClassTopicRepository.InsertAsync(newClassTopic);
            await _unitOfWork.SaveChangesAsync();
            var notificationMessageToLecturer = new NotificationMessage
            {
                AccountId = @class.LecturerId,
                Title = "New Topic Registration",
                Message = $"A New Topic has been suggested to you from group {project.GroupNum} of Class {@class.ShortName}"
            };
            await _messageService.SendMessage(notificationMessageToLecturer);
        }

        public async Task<IEnumerable<SuggestedTopicsResponse>> GetSuggestedTopicsLecturer(GetSuggestedTopicsLecturerRequest request, Guid lecturerId)
        {
            List<SuggestedTopicsResponse> topics = new List<SuggestedTopicsResponse>();
            var groupQuery = _unitOfWork.StudentRepository.Get();
            IPMSClass? @class = null;
            if (request.ClassId.HasValue)
            {
                @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId));
                if (@class == null)
                {
                    return topics;
                }
                groupQuery = groupQuery.Where(x => x.ClassId.Equals(request.ClassId));
            }
            // Find distinct project in class
            List<GroupInformation> groups = await groupQuery.Where(x => x.ProjectId != null)
                                                                        .Include(x => x.Project)
                                                                        .GroupBy(x => new { x.Project!.Id, x.Project!.GroupNum })
                                                                            .Select(x => new GroupInformation
                                                                            {
                                                                                Id = x.Key.Id,
                                                                                Num = x.Key.GroupNum,
                                                                                ClassId = x.First().ClassId
                                                                            }).ToListAsync();

            if (groups.Count == 0)
            {
                return topics;
            }
            List<Guid> groupsIds = groups.Select(g => g.Id).ToList();

            List<Topic> preTopics = new List<Topic>();

            preTopics = await _unitOfWork.TopicRepository.Get().Where(t => groupsIds.Contains(t.SuggesterId.Value)).ToListAsync();
            if (request.Statuses.Count > 0) // filter status
            {
                preTopics = preTopics.Where(pT => request.Statuses.Contains(pT.Status)).ToList();
            }
            if (@class != null && @class.ChangeTopicDeadline < DateTime.Now)
            {
                await UpdateStatusTopicExpired(preTopics);
            }
            var components = await _unitOfWork.ComponentsMasterRepository.Get().Where(cm => cm.MasterType == ComponentsMasterType.Topic).Include(cm => cm.Component).ToListAsync();
            foreach (var pre in preTopics)
            {
                var topic = new SuggestedTopicsResponse
                {
                    Id = pre.Id,
                    Title = pre.Name,
                    Description = pre.Description,
                    GroupNum = groups.FirstOrDefault(g => g.Id.Equals(pre.SuggesterId)).Num,
                    Detail = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Topic, pre.Id, pre.Detail)),
                    Status = pre.Status,
                    Iots = components.Where(c => c.MasterId.Equals(pre.Id)).Select(c => new TopicIoT
                    {
                        Name = c.Component.Name,
                        Quantity = c.Quantity

                    }).ToList(),
                    CreateAt = pre.CreatedAt,
                    ClassId = groups.FirstOrDefault(g => g.Id.Equals(pre.SuggesterId)).ClassId

                };
                topics.Add(topic);
            }
            return topics;
        }

        public async Task<SuggestedTopicsResponse> GetSuggestedTopicDetailLecturer(GetSugTopicDetailLecRequest request, Guid lecturerId)
        {
            SuggestedTopicsResponse topic = new SuggestedTopicsResponse();
            var groupQuery = _unitOfWork.StudentRepository.Get();
            if (request.ClassId.HasValue)
            {
                var isExistClass = await _unitOfWork.IPMSClassRepository.Get().AnyAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId));
                if (!isExistClass)
                {
                    return topic;
                }
                groupQuery = groupQuery.Where(x => x.ClassId.Equals(request.ClassId));
            }
            // Find distinct project in class
            List<GroupInformation> groups = await groupQuery.Where(x => x.ProjectId != null)
                                                                        .Include(x => x.Project)
                                                                        .GroupBy(x => new { x.Project!.Id, x.Project!.GroupNum })
                                                                            .Select(x => new GroupInformation
                                                                            {
                                                                                Id = x.Key.Id,
                                                                                Num = x.Key.GroupNum,
                                                                            }).ToListAsync();

            if (groups.Count == 0)
            {
                return topic;
            }
            List<Guid> groupsIds = groups.Select(g => g.Id).ToList();
            
            Topic preTopic = await _unitOfWork.TopicRepository.Get().FirstOrDefaultAsync(t => groupsIds.Contains((Guid)t.SuggesterId) // validate that topic get belong to class and project lecturer teach!
                                                                         && t.Id.Equals(request.TopicId));

            var components = await _unitOfWork.ComponentsMasterRepository.Get().Where(cm => cm.MasterType == ComponentsMasterType.Topic 
                                                                            && cm.MasterId.Equals(preTopic.Id))
                                                                        .Include(cm => cm.Component)
                                                                        .ToListAsync();

            if (preTopic == null)
            {
                return topic;
            }

            topic = new SuggestedTopicsResponse
            {
                Id = preTopic.Id,
                Title = preTopic.Name,
                Description = preTopic.Description,
                GroupNum = groups.FirstOrDefault(g => g.Id.Equals(preTopic.SuggesterId)).Num,
                Detail = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Topic, preTopic.Id, preTopic.Detail)),
                Status = preTopic.Status,
                Iots = components.Where(c => c.MasterId.Equals(preTopic.Id)).Select(c => new TopicIoT
                {
                    Name = c.Component.Name,
                    Quantity = c.Quantity

                }).ToList()

            };
            return topic;
        }

        private async Task UpdateStatusTopicExpired(List<Topic> topics)
        {
            foreach (var topic in topics)
            {
                if (topic.Status == RequestStatus.Waiting)
                {
                    topic.Status = RequestStatus.Rejected;
                }
                _unitOfWork.TopicRepository.Update(topic);
            }
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<ValidationResultModel> ReviewSuggestedTopicValidators(ReviewSuggestedTopicRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operators did not successfully!"
            };
            IPMSClass @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId));

            if (@class == null)
            {
                result.Message = "Class does not exist";
                return result;
            }

            if (@class.ChangeTopicDeadline < DateTime.Now)
            {
                result.Message = "Review Topic is expired";
                return result;
            }

            // Find distinct project in class
            List<Guid> groupsIds = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == @class.Id && x.ProjectId != null)
                                                                        .Include(x => x.Project)
                                                                        .GroupBy(x => new { x.Project!.Id })
                                                                            .Select(x => x.Key.Id).ToListAsync();
            if (groupsIds.Count == 0)
            {
                result.Message = "Group does not exist";
                return result;

            }

            Topic preTopic = await _unitOfWork.TopicRepository.Get().FirstOrDefaultAsync(t => groupsIds.Contains((Guid)t.SuggesterId) // validate that topic get belong to class and project lecturer teach!
                                                                         && t.Id.Equals(request.TopicId));
            if (preTopic == null)
            {
                result.Message = "Topic does not exist";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }
        public async Task ReviewSuggestedTopic(ReviewSuggestedTopicRequest request, Guid lecturerId)
        {
            SuggestedTopicsResponse topic = new SuggestedTopicsResponse();
            IPMSClass @class = await _unitOfWork.IPMSClassRepository.Get().FirstOrDefaultAsync(c => c.Id.Equals(request.ClassId) && c.LecturerId.Equals(lecturerId));
            

            // Find distinct project in class
            List<Guid> groupsIds = await _unitOfWork.StudentRepository.Get().Where(x => x.ClassId == @class.Id && x.ProjectId != null)
                                                                        .Include(x => x.Project)
                                                                        .GroupBy(x => new { x.Project.Id })
                                                                            .Select(x => x.Key.Id).ToListAsync();
            //List<Guid> groupsIds = groups.Select(g => g.Id).ToList();

            Topic preTopic = await _unitOfWork.TopicRepository.Get().FirstOrDefaultAsync(t => groupsIds.Contains((Guid)t.SuggesterId) // validate that topic get belong to class and project lecturer teach!
                                                                         && t.Id.Equals(request.TopicId));
            if (request.IsApproved == true)
            {
                preTopic.Status = RequestStatus.Approved;
            } else if (request.IsApproved == false)
            {
                preTopic.Status = RequestStatus.Rejected;
                //Reset topic of project
                var classTopicOfProject = await _unitOfWork.ClassTopicRepository.Get().FirstOrDefaultAsync(x => x.ProjectId == preTopic.SuggesterId);
                if (classTopicOfProject != null)
                {
                    _unitOfWork.ClassTopicRepository.Delete(classTopicOfProject);
                }
            }
            _unitOfWork.TopicRepository.Update(preTopic);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task ChangeVisible(ChangeVisibleTopicRequest request)
        {
            var topic = await _unitOfWork.TopicRepository.Get().FirstOrDefaultAsync(x => x.Id == request.Id && x.Status == RequestStatus.Approved || x.Status == RequestStatus.Hidden);
            if(topic == null)
            {
                throw new DataNotFoundException();
            }
            topic.Status = request.IsPublic ? RequestStatus.Approved : RequestStatus.Hidden;
            _unitOfWork.TopicRepository.Update(topic);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
