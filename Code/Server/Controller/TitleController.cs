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
        public TitleController(GameService gameService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("enter")]
        public ActionResult<GameEnterResPacket> Enter(GameEnterReqPacket req)
        {
            var result = _gameService.Enter(req);
            return result;
        }

        private readonly GameService _gameService;
        private readonly ILogger _logger;
    }
}