using IPMS.Business.Requests.FavoriteTopic;
using IPMS.Business.Responses.FavoriteTopic;

namespace IPMS.Business.Interfaces.Services
{
    public interface IFavoriteTopicListService
    {
        Task<CreateFavoriteTopicListResponse> Create(CreateFavoriteTopicListRequest request, Guid lecturerId);
    }
}
