using IPMS.Business.Interfaces.Services;

namespace IPMS.API.Controllers
{
    public class KitController : ApiControllerBase
    {
        private readonly IKitService _kitService;

        public KitController(IKitService kitService) 
        {
            _kitService = kitService;
        }
       
    }
}
