using Microsoft.AspNetCore.Mvc;
using Protocol;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("")]
    public class DebugController : ControllerBase
    {

        private readonly ILogger<DebugController> _logger;

        public DebugController(ILogger<DebugController> logger)
        {
            _logger = logger;
        }

        //[HttpGet(DebugTestRequestPacket.NAME)]
        //public DebugTestResponsePacket Get()
        //{
        //    return new DebugTestResponsePacket();
        //}

        //[HttpPost(DebugTestRequestPacket.NAME)]
        //public DebugTestResponsePacket Get(DebugTestRequestPacket req)
        //{
        //    return new DebugTestResponsePacket();
        //}
    }
}