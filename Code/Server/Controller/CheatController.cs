using Microsoft.AspNetCore.Mvc;
using Protocol;
using Server.Service;
using WebStudyServer.Filter;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("cheat")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthFilter))]
    [ServiceFilter(typeof(UserTransactionFilter))]
    public class CheatController : ControllerBase
    {
        public CheatController(CheatService cheatService, ILogger<CheatController> logger)
        {
            _cheatService = cheatService;
            _logger = logger;
        }

        [HttpPost("reward")]
        public ActionResult<CheatRewardResPacket> BuyStructure(CheatRewardReqPacket req)
        {
            var result = _cheatService.Reward(req);
            return result;
        }

        private readonly ILogger _logger;
        private readonly CheatService _cheatService;
    }
}