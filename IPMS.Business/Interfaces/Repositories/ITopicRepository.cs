﻿using IPMS.DataAccess.Models;

namespace IPMS.Business.Interfaces.Repositories
{
    public interface ITopicRepository : IGenericRepository<Topic>
    {
        IQueryable<Topic> GetApprovedTopics();
    }
}
