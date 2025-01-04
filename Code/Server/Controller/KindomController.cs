using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("kingdom")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class KindomController : ControllerBase
    {
        public KindomController(GameService gameService, ILogger<KindomController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("buy-structure")]
        public ActionResult<KingdomBuyStructureResPacket> BuyStructure(KingdomBuyStructureReqPacket req)
        {
            var result = _gameService.KingdomStructureBuy(req);
            return result;
        }

        [HttpPost("buy-deco")]
        public ActionResult<KingdomBuyDecoResPacket> BuyDeco(KingdomBuyDecoReqPacket req)
        {
            var result = _gameService.KingdomDecoBuy(req);
            return result;
        }

        [HttpPost("construct-structure")]
        public ActionResult<KingdomConstructStructureResPacket> ConstructStructure(KingdomConstructStructureReqPacket req)
        {
            var result = _gameService.KingdomConstructStructure(req);
            return result;
        }

        [HttpPost("construct-deco")]
        public ActionResult<KingdomConstructDecoResPacket> ConstructDeco(KingdomConstructDecoReqPacket req)
        {
            var result = _gameService.KingdomConstructDeco(req);
            return result;
        }

        [HttpPost("store")]
        public ActionResult<KingdomStoreResPacket> Store(KingdomStoreReqPacket req)
        {
            var result = _gameService.KingdomItemStore(req);
            return result;  
        }

        [HttpPost("dec-time-structure")]
        public ActionResult<KingdomDecTimeStructureResPacket> DecTime(KingdomDecTimeStructureReqPacket req)
        {
            var result = _gameService.KingdomItemDecTime(req);
            return result;
        }

        private readonly GameService _gameService;
        private readonly ILogger _logger;
    }
}