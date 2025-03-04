using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("schedule")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class ScheduleController : ControllerBase
    {
        public ScheduleController(GameService gameService, ILogger<GameController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpPost("load")]
        public ActionResult<ScheduleLoadResPacket> Load(ScheduleLoadReqPacket req)
        {
            var res = _gameService.LoadSchedule(req);
            return res;
        }

        private readonly GameService _gameService;
        private readonly ILogger _logger;
    }
}