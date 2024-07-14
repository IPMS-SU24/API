using Azure;
using IPMS.Business.Common.Constants;
using IPMS.Business.Common.Exceptions;
using IPMS.Business.Common.Utils;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Models;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Responses.FavoriteTopic;
using IPMS.DataAccess.Common.Enums;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class FavoriteTopicListService : IFavoriteTopicListService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPresignedUrlService _presignedUrlService;

        public FavoriteTopicListService(IUnitOfWork unitOfWork, IPresignedUrlService presignedUrlService)
        {
            _unitOfWork = unitOfWork;
            _presignedUrlService = presignedUrlService;
        }

        public async Task<ValidationResultModel> CheckValidCreate(Guid lecturerId, CreateFavoriteTopicListRequest request)
        {
            var result = new ValidationResultModel()
            {
                Message = "Cannot Create"
            };
            var isExistName = await _unitOfWork.FavoriteRepository.Get().AnyAsync(x => x.LecturerId == lecturerId && request.ListName == x.Name);
            if (isExistName)
            {
                result.Message = "List Name is existed";
                return result;
            }
            result.Message = string.Empty;
            result.Result = true;
            return result;
        }

        public async Task<CreateFavoriteTopicListResponse> Create(CreateFavoriteTopicListRequest request, Guid lecturerId)
        {
            var newFavorite = new Favorite
            {
                LecturerId = lecturerId,
                Name = request.ListName
            };
            await _unitOfWork.FavoriteRepository.InsertAsync(newFavorite);
            await _unitOfWork.SaveChangesAsync();
            return new CreateFavoriteTopicListResponse
            {
                ListId = newFavorite.Id
            };
        }

        public async Task UpdateAsync(UpdateFavoriteTopicListRequest request, Guid lecturerId)
        {
            var favorite = await _unitOfWork.FavoriteRepository.Get().AnyAsync(x => x.Id == request.ListId);
            if (!favorite) throw new DataNotFoundException();
            var topics = await _unitOfWork.TopicRepository.GetApprovedTopics().Where(x => request.TopicIds.Contains(x.Id)).Select(x => x.Id).CountAsync();
            if (topics != request.TopicIds.Count) throw new DataNotFoundException();
            var existingTopic = await _unitOfWork.TopicFavoriteRepository.Get().Where(x => x.FavoriteId == request.ListId).ToListAsync();
            if (existingTopic != null && existingTopic.Any())
            {
                _unitOfWork.TopicFavoriteRepository.DeleteRange(existingTopic);
            }
            var newTopicList = request.TopicIds.Select(x => new TopicFavorite
            {
                FavoriteId = request.ListId,
                TopicId = x
            });
            await _unitOfWork.TopicFavoriteRepository.InsertRangeAsync(newTopicList);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid favoriteId)
        {
            var existingFavorite = await _unitOfWork.FavoriteRepository.Get().Include(x => x.Topics).Where(x => x.Id == favoriteId).FirstOrDefaultAsync();
            if (existingFavorite == null) throw new DataNotFoundException();
            _unitOfWork.FavoriteRepository.Delete(existingFavorite);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IList<GetAllFavoriteResponse>> GetAsync(Guid lecturerId)
        {
            var response = await _unitOfWork.FavoriteRepository.Get().Where(x => x.LecturerId == lecturerId).Select(x => new GetAllFavoriteResponse
            {
                ListId = x.Id,
                ListName = x.Name
            }).ToListAsync();
            if (response == null || !response.Any()) throw new DataNotFoundException();
            return response;
        }

        public async Task<IList<GetFavoriteTopicResponse>> GetInFavoriteAsync(Guid listId)
        {
            var response = await _unitOfWork.TopicRepository.GetApprovedTopics().Include(x => x.Favorites).Select(x => new GetFavoriteTopicResponse
            {
                Description = x.Description,
                TopicId = x.Id,
                TopicName = x.Name,
                DetailLink = _presignedUrlService.GeneratePresignedDownloadUrl(S3KeyUtils.GetS3Key(S3KeyPrefix.Topic, x.Id, x.Detail)),
                IsBelongToList = x.Favorites.Any(x => x.FavoriteId == listId)
            }).ToListAsync();
            if (response == null || !response.Any()) throw new DataNotFoundException();
            var allTopicIoT = await _unitOfWork.ComponentsMasterRepository.GetTopicComponents().Include(x => x.Component).Where(x => response.Select(x => x.TopicId).ToList().Contains(x.MasterId.Value)).Select(x => new
            {
                Title = x.Component.Name,
                x.Component.Description,
                Id = x.ComponentId,
                TopicId = x.MasterId
            }).ToListAsync();

            foreach (var topic in response)
            {
                topic.IoTComponents.AddRange(allTopicIoT.Where(x => x.TopicId == topic.TopicId).Select(x => new FavoriteIoTInfo
                {
                    Description = x.Description,
                    Id = x.Id,
                    Title = x.Title
                }));

            }
            return response;
        }

        public async Task<IList<GetListTopicResponse>> GetListTopic(Guid lecturerId)
        {
            List<GetListTopicResponse> favTopics = new List<GetListTopicResponse>();
            favTopics = await _unitOfWork.FavoriteRepository.Get().Where(f => f.LecturerId.Equals(lecturerId))
                            .Include(f => f.Topics.Where(t => t.Topic.Status == RequestStatus.Approved))
                            .ThenInclude(t => t.Topic).Select(f => new GetListTopicResponse
                            {
                                Id = f.Id,
                                Name = f.Name,
                                favTopicInfos = f.Topics.Select(t => new FavTopicInfo
                                {
                                    TopicId = t.TopicId,
                                    TopicName = t.Topic.Name
                                }).ToList(),
                            }).ToListAsync();

            return favTopics;
        }
    }
}
