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

        [HttpPost("buy")]
        public ActionResult<KingdomItemBuyResPacket> Buy(KingdomItemBuyReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomItemBuyResPacket
            {
            };
        }

        [HttpPost("construct")]
        public ActionResult<KingdomItemConstructResPacket> Construct(KingdomItemConstructReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomItemConstructResPacket
            {
            };
        }

        [HttpPost("cancel")]
        public ActionResult<KingdomItemCancelResPacket> Cancel(KingdomItemCancelReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomItemCancelResPacket
            {
            };
        }

        [HttpPost("dec-time")]
        public ActionResult<KingdomItemDecTimeResPacket> Enter(KingdomItemDecTimeReqPacket req)
        {
            var result = _gameService.Enter();

            return new KingdomItemDecTimeResPacket
            {
            };
        }

        private readonly GameService _gameService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
    }
}