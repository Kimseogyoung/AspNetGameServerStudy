using Microsoft.AspNetCore.Mvc;
using Protocol;
using WebStudyServer.Filter;
using WebStudyServer.Service;

namespace WebStudyServer.Controllers
{
    [ApiController]
    [Route("auth")]
    [ServiceFilter(typeof(LogFilter))]
    [ServiceFilter(typeof(AuthTransactionFilter))]
    public class AuthController : ControllerBase
    {
        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("sign-up")]
        public ActionResult<AuthSignUpResPacket> SignUp(AuthSignUpReqPacket req)
        {
            var res = _authService.SignUp(req.DeviceKey);
            return res;
        }

        [HttpPost("sign-in")]
        public ActionResult<AuthSignInResPacket> SignIn(AuthSignInReqPacket req)
        {
            var res = _authService.SignIn(req.ChannelId);
            return res;
        }

        private readonly AuthService _authService;
        private readonly ILogger _logger;
    }
}