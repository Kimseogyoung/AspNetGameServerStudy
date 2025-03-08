using WebStudyServer.Service;
using WebStudyServer;
using Proto;
using WebStudyServer.Repo;
using WebStudyServer.Helper;
using Protocol;
using WebStudyServer.Model;
using WebStudyServer.GAME;
using AutoMapper;
using Server.Repo;

namespace Server.Service
{
    public class CheatService : ServiceBase
    {
        public CheatService( DbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<CheatService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public CheatRewardResPacket Reward(CheatRewardReqPacket req)
        {
            var mgrPlayerDetail = _dbRepo.OwnUser.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(req.RewardList, "CHEAT");
            return new CheatRewardResPacket
            {
                ChgObjList = chgObjList
            };
        }

        private readonly DbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
