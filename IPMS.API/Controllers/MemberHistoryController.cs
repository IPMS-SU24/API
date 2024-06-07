using IPMS.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPMS.API.Controllers
{
    public class MemberHistoryController : ApiControllerBase
    {
        private readonly IMemberHistoryService _memberHistoryService;
        public MemberHistoryController(IMemberHistoryService MemberHistoryService)
        {
            _memberHistoryService = MemberHistoryService;
        }
    }
}
