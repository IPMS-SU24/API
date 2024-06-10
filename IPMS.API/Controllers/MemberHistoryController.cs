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

        [HttpGet]
        public async Task<IActionResult> GetLoggedInUserHistory()
        {
            var response = await _memberHistoryService.GetLoggedInUserHistories(new Guid("9c7d4c9e-6a23-4c13-af6d-e85b83705b2e"));
            return Ok();
        }
    }
}
