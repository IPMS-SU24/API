﻿using IPMS.Business.Models;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Responses.FavoriteTopic;

namespace IPMS.Business.Interfaces.Services
{
    public interface IFavoriteTopicListService
    {
        Task<CreateFavoriteTopicListResponse> Create(CreateFavoriteTopicListRequest request, Guid lecturerId);
        Task<ValidationResultModel> CheckValidCreate(Guid lecturerId, CreateFavoriteTopicListRequest request);
        Task UpdateAsync(UpdateFavoriteTopicListRequest request, Guid lecturerId);
        Task DeleteAsync(Guid favoriteId);
        Task<IList<GetAllFavoriteResponse>> GetAsync(Guid lecturerId);
        Task<IList<GetListTopicResponse>> GetListTopic(Guid lecturerId);
        Task<ValidationResultModel> AssignTopicListValidators(AssignTopicListRequest request, Guid lecturerId);
        Task AssignTopicList(AssignTopicListRequest request, Guid lecturerId);
        Task<IList<GetFavoriteTopicResponse>> GetInFavoriteAsync(Guid listId, bool isAdminRequest = false);
    }
}
