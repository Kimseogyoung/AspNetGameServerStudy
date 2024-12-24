using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer.Controllers
{
    // 인증 필요 X
    [ApiController]
    [Route("title")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class TitleController : ControllerBase
    {
        public TitleController(GameService gameService, IMapper mapper, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpPost("enter")]
        public ActionResult<GameEnterResPacket> Enter(GameEnterReqPacket req)
        {
            var result = _gameService.Enter();

            return new GameEnterResPacket
            {
                Player = _mapper.Map<PlayerPacket>(result.Player)
            };
        }

        private readonly GameService _gameService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
    }
}