using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("kingdom-item")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class KindomItemController : ControllerBase
    {
        public KindomItemController(GameService gameService, IMapper mapper, ILogger<KindomItemController> logger)
        {
            _gameService = gameService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("buy-structure")]
        public ActionResult<KingdomBuyStructureResPacket> BuyStructure(KingdomBuyStructureReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomBuyStructureResPacket
            {
            };
        }

        [HttpPost("buy-deco")]
        public ActionResult<KingdomBuyDecoResPacket> BuyDeco(KingdomBuyDecoReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomBuyDecoResPacket
            {
            };
        }

        [HttpPost("construct-structure")]
        public ActionResult<KingdomConstructStructureResPacket> ConstructStructure(KingdomConstructStructureReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomConstructStructureResPacket
            {
            };
        }

        [HttpPost("store")]
        public ActionResult<KingdomStoreResPacket> Store(KingdomStoreReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomStoreResPacket
            {
            };
        }

        [HttpPost("dec-time-structure")]
        public ActionResult<KingdomDecTimeStructureResPacket> DecTime(KingdomDecTimeStructureReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomDecTimeStructureResPacket
            {
            };
        }

        private readonly GameService _gameService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
    }
}