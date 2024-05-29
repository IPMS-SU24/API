using IPMS.Business.Interfaces.Services;
using IPMS.Business.Interfaces;
using IPMS.Business.Requests.Topic;
using IPMS.DataAccess.Models;
using AutoFilterer.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IPMS.Business.Services
{
    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _unitOfWork;
        public TopicService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IQueryable<Topic> GetTopics(GetTopicRequest request)
        {
            return _unitOfWork.TopicRepository.Get().ApplyFilter(request).AsNoTracking();
        }
    }
}
