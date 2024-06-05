﻿using IPMS.Business.Interfaces.Repositories;
using IPMS.Business.Repository;
using IPMS.DataAccess;
using IPMS.DataAccess.Models;

namespace IPMS.Business.Repositories
{
    public class ReportTypeRepository : GenericRepository<ReportType>, IReportTypeRepository
    {
        public ReportTypeRepository(IPMSDbContext context) : base(context)
        {
        }
    }
}
