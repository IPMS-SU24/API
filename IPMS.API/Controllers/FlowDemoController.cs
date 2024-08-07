using IPMS.API.Responses;
using IPMS.Business.Common.Singleton;
using IPMS.Business.Common.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace IPMS.API.Controllers
{
    public class FlowDemoController : ApiControllerBase
    {
        private readonly IDistributedCache _cache;

        public FlowDemoController(IDistributedCache cache)
        {
            _cache = cache;
        }

        [HttpPost]
        public async Task<IActionResult> ClearAllCacheAsync()
        {
            if (_cache is MemoryDistributedCache memoryCache)
            {
                // Remove all entries from the cache
            }
            await _cache.RemoveAsync("Project");
            await _cache.RemoveAsync("Class");
            CurrentSemesterInfo.Instance.CurrentSemester = null;
            return GetActionResponse(new IPMSResponse<object>());
        }
    }
}
