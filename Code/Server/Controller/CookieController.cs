using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("cookie")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class CookieController : ControllerBase
    {
        public CookieController(GameService gameService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("enhance-star")]
        public ActionResult<CookieEnhanceStarResPacket> EnhanceStar(CookieEnhanceStarReqPacket req)
        {
            var res = _gameService.EnhanceCookieStar(req);
            return res;
        }


        [HttpPost("enhance-lv")]
        public ActionResult<CookieEnhanceLvResPacket> EnhanceLv(CookieEnhanceLvReqPacket req)
        {
            var res = _gameService.EnhanceCookieLv(req);
            return res;
        }

        private readonly GameService _gameService;
        private readonly ILogger _logger;
    }
}