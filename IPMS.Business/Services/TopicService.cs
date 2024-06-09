using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.Topic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;
using IPMS.Business.Models;
using AutoMapper;
using IPMS.DataAccess.Common.Enums;

namespace IPMS.Business.Services
{
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonService;
        private readonly IMapper _mapper;
        private readonly IMessageService _messageService;
        public TopicService(IUnitOfWork unitOfWork, ICommonServices commonServices, IMapper mapper, IMessageService messageService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonServices;
            _mapper = mapper;
            _messageService = messageService;
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
            var project = await _commonService.GetProject(leaderId);
            if (project == null)
            {
                result.Message = "Student is not in project";
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

        public async Task RegisterTopic(RegisterTopicRequest request, Guid leaderId)
        {
            //Create new Topic
            var newTopic = _mapper.Map<Topic>(request);
            var studiesIn = await _commonService.GetStudiesIn(leaderId);
            var @class = await _commonService.GetCurrentClass(studiesIn.Select(x => x.ClassId));
            newTopic.OwnerId = @class.LecturerId;
            await _unitOfWork.TopicRepository.InsertAsync(newTopic);
            var project = await _commonService.GetProject(leaderId);
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
                AccountId = @class.LecturerId!.Value,
                Title = "New Topic Registration",
                Message = $"A New Topic has been suggested to you from {project.GroupName}"
            };
            _messageService.SendMessage(notificationMessageToLecturer);
        }
    }
}
