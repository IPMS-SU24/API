﻿using IPMS.Business.Interfaces;
using IPMS.Business.Interfaces.Services;
using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Responses.FavoriteTopic;
using IPMS.DataAccess.Models;

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
    }
}
