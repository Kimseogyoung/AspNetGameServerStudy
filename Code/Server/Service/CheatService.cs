using Protocol;
using Server.Extension;
using WebStudyServer;
using WebStudyServer.Data;

namespace Server.Service
{
    public class CheatService : ServiceBase
    {
        public CheatService(GameDb db, RpcContext rpcContext, ILogger<CheatService> logger) : base(db, rpcContext, logger)
        {
        }

        public async Task<CheatRewardResponsePacket> RewardAsync(CheatRewardRequestPacket req)
        {
            var changeList = await RewardService.GrantListAsync(OwnScope, req.RewardList, "CHEAT");
            return new CheatRewardResponsePacket
            {
                ChgObjList = changeList.ToPacketList()
            };
        }
    }
}
