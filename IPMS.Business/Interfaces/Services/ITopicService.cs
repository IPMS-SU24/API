using IPMS.Business.Models;
using IPMS.Business.Requests.Topic;
using IPMS.Business.Responses.Topic;
using IPMS.DataAccess.Models;
using System.Threading.Tasks;

namespace IPMS.Business.Interfaces.Services
{
    public interface ITopicService
    {
        IQueryable<Topic> GetAllTopics();
        Task<IEnumerable<SuggestedTopicsResponse>> GetSuggestedTopicsLecturer(GetSuggestedTopicsLecturerRequest request, Guid lecturerId);
        Task<SuggestedTopicsResponse> GetSuggestedTopicDetailLecturer(GetSugTopicDetailLecRequest request, Guid lecturerId);
        Task<IEnumerable<SuggestedTopicsResponse>> GetSuggestedTopics();
        IQueryable<Topic> GetApprovedTopics(GetTopicRequest request);
        Task RegisterTopic(RegisterTopicRequest request, Guid leaderId);
        Task<ValidationResultModel> LecturerRegisterNewTopicValidator(LecturerRegisterTopicRequest request);
        Task LecturerRegisterNewTopic(LecturerRegisterTopicRequest request, Guid lecturerId);
        Task<ValidationResultModel> CheckRegisterValid(RegisterTopicRequest request, Guid leaderId);
        Task ReviewSuggestedTopic(ReviewSuggestedTopicRequest request, Guid lecturerId);
        Task<ValidationResultModel> ReviewSuggestedTopicValidators(ReviewSuggestedTopicRequest request, Guid lecturerId);
        Task ChangeVisible(ChangeVisibleTopicRequest request);
    }
}
