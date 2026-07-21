using AutoMapper;
using Proto;
using Protocol;
using Server.Repo;
using WebStudyServer;
using WebStudyServer.Helper;
using WebStudyServer.Model;
using WebStudyServer.Repo;
using WebStudyServer.Service;

namespace Server.Service
{
    public class CheatService : ServiceBase
    {
        public CheatService(GlobalDbRepo dbRepo, IMapper mapper, RpcContext rpcContext, ILogger<CheatService> logger) : base(rpcContext, logger)
        {
            _dbRepo = dbRepo;
            _mapper = mapper;
        }

        public CheatRewardResponsePacket Reward(CheatRewardRequestPacket req)
        {
            var mgrPlayerDetail = _dbRepo.OwnUser.PlayerDetail.Touch();
            var chgObjList = mgrPlayerDetail.IncRewardList(req.RewardList, "CHEAT");
            return new CheatRewardResponsePacket
            {
                ChgObjList = chgObjList
            };
        }

        private readonly GlobalDbRepo _dbRepo;
        private readonly IMapper _mapper;
    }
}
