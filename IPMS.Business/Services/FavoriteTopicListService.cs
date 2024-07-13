using IPMS.Business.Common.Exceptions;
using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Responses.FavoriteTopic;
using IPMS.DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class FavoriteTopicListService : IFavoriteTopicListService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FavoriteTopicListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

        public async Task Update(UpdateFavoriteTopicListRequest request, Guid lecturerId)
        {
            var favorite = await _unitOfWork.FavoriteRepository.Get().Include(x=>x.Topics).FirstOrDefaultAsync(x=>x.Id == request.ListId);
            if (favorite == null) throw new DataNotFoundException();
            var topics = await _unitOfWork.TopicRepository.GetApprovedTopics().Where(x=>request.TopicIds.Contains(x.Id)).Select(x=>x.Id).CountAsync();
            if(topics != request.TopicIds.Count) throw new DataNotFoundException();
            favorite.Topics.Clear();
            foreach(var topicId in request.TopicIds)
            {
                favorite.Topics.Add(new TopicFavorite
                {
                    TopicId = topicId,
                    FavoriteId = favorite.Id,
                });
            }
            _unitOfWork.FavoriteRepository.Update(favorite);
            await _unitOfWork.SaveChangesAsync();

        }
    }
}
