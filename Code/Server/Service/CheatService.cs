using WebStudyServer.Service;
using WebStudyServer;
using Proto;
using WebStudyServer.Repo;
using WebStudyServer.Helper;
using Protocol;
using WebStudyServer.Model;
using WebStudyServer.GAME;
using AutoMapper;

namespace Server.Service
{
    public class CheatService : ServiceBase
    {
        public CheatService(AllUserRepo allUserRepo, AuthRepo authRepo, UserRepo userRepo, IMapper mapper, RpcContext rpcContext, ILogger<GameService> logger) : base(rpcContext, logger)
        {
            _userRepo = userRepo;
            _mapper = mapper;
        }

        public CheatRewardResPacket Reward(CheatRewardReqPacket req)
        {
            var mgrPlayerDetail = _userRepo.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(req.RewardList, "CHEAT");
            return new CheatRewardResPacket
            {
                ChgObjList = chgObjList
            };
        }

        private readonly UserRepo _userRepo;
        private readonly IMapper _mapper;
    }
}
