using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.DataAccess.Models;
using IPMS.Business.Common.Enums;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using IPMS.Business.Requests.IoTComponent;
using IPMS.Business.Responses.ProjectDashboard;
using IPMS.Business.Common.Exceptions;
using IPMS.DataAccess.Common.Enums;
using Microsoft.AspNetCore.Http;
using IPMS.Business.Common.Extensions;
using IPMS.Business.Responses.IoT;
using IPMS.Business.Responses.Project;
using IPMS.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.Business.Services
{
    public class BorrowIoTService : IBorrowIoTService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonServices _commonServices;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _context;

        public BorrowIoTService(IUnitOfWork unitOfWork, ICommonServices commonServices, IMapper mapper, IHttpContextAccessor context)
        {
            _unitOfWork = unitOfWork;
            _commonServices = commonServices;
            _mapper = mapper;
            _context = context;
        }

        public async Task<bool> CheckIoTValid(IoTModelRequest request, Guid leaderId)
        {
            if (request.Quantity <= 0) return false;
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var studiesIn = await _commonServices.GetStudiesIn(leaderId);
            var project = await _commonServices.GetProject(leaderId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemester.Id);
            //Check borrow Assessment Status => Not In Progress => Validate Fail
            var borrowAssessmentStatus = await _commonServices.GetBorrowIoTStatus(project.Id, @class);
            if (borrowAssessmentStatus != AssessmentStatus.InProgress) return false;
            //Check Exist
            var iot = await _unitOfWork.IoTComponentRepository.GetByIDAsync(request.ComponentId);
            if (iot == null) return false;
            //Check iot in iot list of Topic
            var topicId = await _unitOfWork.ClassTopicRepository.Get().Where(x => x.ProjectId == project.Id).Select(x => x.TopicId).FirstOrDefaultAsync(); //checked project get topic
            var isInTopicComponent = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents()
                                                                                    .Where(x => x.MasterId == topicId && x.ComponentId == request.ComponentId).AnyAsync();
            if (!isInTopicComponent) return false;
            //Check remain Quantity
            var remainQuantity = await _commonServices.GetRemainComponentQuantityOfLecturer(@class.LecturerId!.Value, request.ComponentId);
            if (remainQuantity < request.Quantity) return false;
            return true;
        }

        public async Task<IEnumerable<BorrowIoTComponentInformation>> GetAvailableIoTComponents(GetAvailableComponentRequest request, Guid leaderId)
        {
            var result = new List<BorrowIoTComponentInformation>();
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;
            var studiesIn = await _commonServices.GetStudiesIn(leaderId);
            var @class = await _commonServices.GetCurrentClass(studiesIn.Select(x => x.ClassId), currentSemester.Id);
            var componentTopics = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents().Where(x => x.MasterId == request.TopicId).Include(x => x.Component).ToListAsync();
            if (!componentTopics.Any()) throw new DataNotFoundException("Not Found Component for Topic");
            var mapComponentTasks = new List<Task<BorrowIoTComponentInformation>>();
            foreach (var component in componentTopics)
            {
                mapComponentTasks.Add(MapBorrowIoTComponentInformation(component, @class));
            }
            result.AddRange(await Task.WhenAll(mapComponentTasks));
            return result;
        }

        public async Task<IEnumerable<GetBorrowIoTComponentsResponse>> GetBorrowIoTComponents(GetBorrowIoTComponentsRequest request, Guid lecturerId)
        {
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;

            var classes = await _unitOfWork.IPMSClassRepository.Get().Where(c => c.LecturerId.Equals(lecturerId)
                                                && c.Semester.Id.Equals(currentSemester.Id))
                                            .Include(c => c.Topics)
                                            .ThenInclude(ct => ct.Project)
                                            .ToListAsync();

            if (request.ClassIds.Count != 0) // filter
            {
                classes = classes.Where(c => request.ClassIds.Contains(c.Id)).ToList();
            }


            var components = await _unitOfWork.ComponentsMasterRepository.Get() // get components
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project)
                                    .Include(cm => cm.Component).ToListAsync();


            List<ProjectInformation> projects = new List<ProjectInformation>();
            foreach (var @class in classes) // prepare information
            {
                projects.AddRange(@class.Topics.Where(ct => ct.ProjectId != null).Select(ct => new ProjectInformation
                {
                    ProjectId = ct.ProjectId,
                    GroupName = $"Group {ct.Project.GroupNum}",
                    ClassName = @class.ShortName
                }).ToList());
            }

            List<GetBorrowIoTComponentsResponse> borrowComponents = new();

            foreach (var prj in projects)
            {
                var prjComponents = components.Where(c => c.MasterId.Equals(prj.ProjectId)); // get borrow component base on projects

                borrowComponents.AddRange(prjComponents.GroupBy(cm => new { cm.CreatedAt.Year, cm.CreatedAt.Month, cm.CreatedAt.Day, cm.CreatedAt.Hour }) // add to response
                .Select(g => new GetBorrowIoTComponentsResponse
                {
                    ClassName = prj.ClassName,
                    GroupName = prj.GroupName,
                    CreateAt = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    Items = g.Select(g => new IotItem
                    {
                        Id = g.Id,
                        Name = g.Component!.Name,
                        Quantity = g.Quantity,
                        Status = g.Status

                    }).ToList()

                }).ToList());
            }
            if (request.OrderBy)
            {
                borrowComponents = borrowComponents.OrderByDescending(bC => bC.CreateAt).ToList();
            }
            else
            {
                borrowComponents = borrowComponents.OrderBy(bC => bC.CreateAt).ToList();
            }

            return borrowComponents;
        }

        public async Task<IEnumerable<ReportIoTComponentInformation>> GetGetReportIoTComponents()
        {
            IEnumerable<ReportIoTComponentInformation> report = new List<ReportIoTComponentInformation>();
            Project? project = _context.HttpContext.Session.GetObject<Project?>("Project");

            if (project == null)
            {
                return report;
            }
            var components = await _unitOfWork.ComponentsMasterRepository.Get()
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project && cm.MasterId.Equals(project.Id))
                                    .Include(cm => cm.Component).ToListAsync();


            report = components.GroupBy(cm => new { cm.CreatedAt.Year, cm.CreatedAt.Month, cm.CreatedAt.Day, cm.CreatedAt.Hour })
                .Select(g => new ReportIoTComponentInformation
                {
                    CreatedAt = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    IoTComponents = g.Select(g => new IotItem
                    {
                        Id = g.Id,
                        Name = g.Component!.Name,
                        Quantity = g.Quantity,
                        Status = g.Status
                    }).ToList()

                }).ToList();
            return report;
        }

        public async Task RegisterIoTForProject(Guid leaderId, IEnumerable<IoTModelRequest> borrowIoTModels)
        {
            var projectId = (await _commonServices.GetProject(leaderId)).Id;
            var componentMasters = _mapper.Map<IEnumerable<ComponentsMaster>>(borrowIoTModels, opts =>
            {
                opts.Items[nameof(ComponentsMaster.MasterId)] = projectId;
                opts.Items[nameof(ComponentsMaster.MasterType)] = ComponentsMasterType.Project;
            });

            await _unitOfWork.ComponentsMasterRepository.InsertRangeAsync(componentMasters); // create borrow Iot Components
            await _unitOfWork.SaveChangesAsync();


        }
        public async Task<ValidationResultModel> ReviewBorrowIoTComponentsValidators(ReviewBorrowIoTComponentsRequest request, Guid lecturerId)
        {
            var result = new ValidationResultModel
            {
                Message = "Operation did not successfully"
            };
            // check component quantity < 0
            var validReqComponts = request.IotComponents.Any(ic => ic.Quantity < 0);
            if (validReqComponts)
            {
                result.Message = "Cannot set Quantity < 0";
                return result;
            }

            var components = await _unitOfWork.ComponentsMasterRepository.Get()
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project && cm.MasterId.Equals(request.ProjectId) && cm.Status == BorrowedStatus.Pending) // check that cannot review approved || rejected component
                                    .Include(cm => cm.Component).ToListAsync();
            if (components.Count() == 0)
            {
                result.Message = "Does not have any request from Project";
                return result;
            }

            var groups = components.GroupBy(cm => new { cm.CreatedAt.Year, cm.CreatedAt.Month, cm.CreatedAt.Day, cm.CreatedAt.Hour })
                .Select(g => new GroupIotReview
                {
                    CreatedAt = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    IotComponents = g.Select(g => new IoTReview
                    {
                        Id = g.Id,
                        ComponentId = g.ComponentId, 
                        Quantity = g.Quantity,
                    }).ToList()
                }).ToList();

            var reqBorrow = groups.FirstOrDefault(g => g.CreatedAt == request.CreatedAt);
            if (reqBorrow == null)
            {
                result.Message = "Request cannot found";
                return result;
            }
            var currentSemester = (await CurrentSemesterUtils.GetCurrentSemester(_unitOfWork)).CurrentSemester;

            // get class topic (validate) --> get class, current semester (validate) --> different lecturer Id, check deadline
            var classTopic = await _unitOfWork.ClassTopicRepository.Get()
                                        .FirstOrDefaultAsync(ct => ct.ProjectId.Equals(request.ProjectId) 
                                                    && ct.Class.SemesterId.Equals(currentSemester.Id) 
                                                    && ct.Class.LecturerId.Equals(lecturerId));
                                                    
            if (classTopic == null)
            {
                result.Message = "Cannot found project";
                return result;
            }

            // get topic --> get iot component topic --> check count, check quantity of every components -> must match all

            var topicCompontns = await _unitOfWork.ComponentsMasterRepository.Get()
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Topic && cm.MasterId.Equals(classTopic.TopicId)).ToListAsync();

            // iot components -> count -> If different --> Do not enough

            // group this request, check is enough component in 1 request?
            if (reqBorrow.IotComponents.Count() != request.IotComponents.Count())
            {
                result.Message = "Please send full request";
                return result;
            }

            if (topicCompontns.Count() != request.IotComponents.Count())
            {
                result.Message = "Request not match with current topic";
                return result;
            }

            // check match all
            foreach (var compont in request.IotComponents)
            {
                var borrow = reqBorrow.IotComponents.FirstOrDefault(iot => iot.Id.Equals(compont.Id));
                if (borrow == null) // check with request
                {
                    result.Message = "Component does not match request";
                    return result;
                }
                borrow.Quantity = compont.Quantity;

                if (!topicCompontns.Any(iot => iot.ComponentId.Equals(borrow.ComponentId) && iot.Quantity >= compont.Quantity)) // check with topic must lower or equal quantity of request
                {
                    result.Message = "Component request does not match topic";
                    return result;
                }

            }

            // get borrowed

            var projects = await _commonServices.GetAllCurrentProjectsOfLecturer(lecturerId);

            //get from all request project accepted
            List<IoTReview> approvedComponnts = _unitOfWork.ComponentsMasterRepository.Get().Where(cm =>
                    cm.MasterType == ComponentsMasterType.Project
                    && projects.Contains((Guid)cm.MasterId)
                    && cm.Status == BorrowedStatus.Approved).GroupBy(cm => cm.ComponentId).Select(cm => new IoTReview
                    {
                        Id = (Guid)cm.Key,
                        Quantity = cm.Sum(g => g.Quantity)
                    }).ToList();

            //get from all lecturer
            List<IoTReview> lecComponnts = _unitOfWork.ComponentsMasterRepository.Get().Where(cm =>
                    cm.MasterType == ComponentsMasterType.Lecturer
                    && cm.MasterId.Equals(lecturerId)).GroupBy(cm => cm.ComponentId).Select(cm => new IoTReview
                    {
                        Id = (Guid)cm.Key,
                        Quantity = cm.Sum(g => g.Quantity)
                    }).ToList();

            foreach (var compont in reqBorrow.IotComponents)
            {

                var lecCompont = lecComponnts.FirstOrDefault(c => compont.ComponentId.Equals(c.Id));
                if (lecCompont == null)
                {
                    result.Message = "Lecturer does not has request Iot Component";
                    return result;
                }

                var approvedCompont = approvedComponnts.FirstOrDefault(c => compont.ComponentId.Equals(c.Id));

                if (approvedCompont != null)
                {
                    if (approvedCompont.Quantity + compont.Quantity > lecCompont.Quantity)
                    {
                        result.Message = "Lecturer does not has enough quantity";
                        return result;
                    }
                } else if (compont.Quantity > lecCompont.Quantity)
                {
                    result.Message = "Lecturer does not has enough quantity";
                    return result;
                }
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;

        }
        public async Task ReviewBorrowIoTComponents(ReviewBorrowIoTComponentsRequest request, Guid lecturerId)
        {
            var components = await _unitOfWork.ComponentsMasterRepository.Get()
                                    .Where(cm => cm.MasterType == ComponentsMasterType.Project && cm.MasterId.Equals(request.ProjectId) && cm.Status == BorrowedStatus.Pending)
                                    .Include(cm => cm.Component).ToListAsync();
           
            foreach (var compont in request.IotComponents)
            {

                var updtCompont = components.FirstOrDefault(c => c.Id.Equals(compont.Id));

                updtCompont.Quantity = compont.Quantity;

                if (updtCompont.Quantity == 0)
                {
                    updtCompont.Status = BorrowedStatus.Rejected;
                } else if (updtCompont.Quantity > 0)
                {
                    updtCompont.Status = BorrowedStatus.Approved;

                }
                _unitOfWork.ComponentsMasterRepository.Update(updtCompont);
            }

            await _unitOfWork.SaveChangesAsync();

        }



        private async Task<BorrowIoTComponentInformation> MapBorrowIoTComponentInformation(ComponentsMaster component, IPMSClass @class)
        {
            return new BorrowIoTComponentInformation
            {
                Id = component.ComponentId!.Value,
                Name = component.Component.Name,
                Quantity = await _commonServices.GetRemainComponentQuantityOfLecturer(@class.LecturerId!.Value, component.ComponentId!.Value)
            };
        }

    }
}
