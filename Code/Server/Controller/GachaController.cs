using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("gacha")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class GachaController : ControllerBase
    {
        public GachaController(GameService gameService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("normal")]
        public ActionResult<GachaNormalResPacket> GachaNormal(GachaNormalReqPacket req)
        {
            var res = _gameService.GachaNormal(req);
            return res;
        }


        [HttpPost("pickup")]
        public ActionResult<GachaNormalResPacket> GachaNormal2(GachaNormalReqPacket req)
        {
            var res = _gameService.GachaNormal(req);
            return res;
        }

        private readonly GameService _gameService;
        private readonly ILogger _logger;
    }
}