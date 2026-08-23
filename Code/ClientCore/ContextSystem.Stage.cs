using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proto;
using Protocol;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public async Task<WorldFinishStageFirstResponsePacket> RequestWorldFinishFirstStage(int worldNum, int order, int star)
        {
            var prtStage = ProtoDb.GetByMk<WorldStageProto>(worldNum).Where(x => x.Order == order).First();
            var prtRewardList = new List<ObjValue>();
            for (var i = 0; i <= star; i++)
            {
                prtRewardList.AddOrInc(new ObjValue(prtStage.FirstRewardTypeList[i], prtStage.FirstRewardNumList[i], prtStage.FirstRewardAmountList[i]));
            }

            var req = new WorldFinishStageFirstRequestPacket(prtStage.WorldNum, prtStage.Num, star, prtRewardList);
            var res = await RpcSystem.RequestAsync<WorldFinishStageFirstRequestPacket, WorldFinishStageFirstResponsePacket>(req);

            SyncWorld(res.World);
            SyncWorldStage(res.WorldStage);
            SyncChgObjList(res.ChgObjList);
            return res;
        }

        public async Task<WorldFinishStageRepeatResponsePacket> RequestWorldFinishRepeatStage(int worldNum, int order, int star)
        {
            var prtStage = ProtoDb.GetByMk<WorldStageProto>(worldNum).Where(x => x.Order == order).First();
            var pakStage = GetWorldStageForce(prtStage.Num);
            var prtRewardList = new List<ObjValue>();
            for (var i = pakStage.Star + 1; i <= star; i++)
            {
                prtRewardList.AddOrInc(new ObjValue(prtStage.FirstRewardTypeList[i], prtStage.FirstRewardNumList[i], prtStage.FirstRewardAmountList[i]));
            }

            var req = new WorldFinishStageRepeatRequestPacket(prtStage.WorldNum, prtStage.Num, star, prtRewardList);
            var res = await RpcSystem.RequestAsync<WorldFinishStageRepeatRequestPacket, WorldFinishStageRepeatResponsePacket>(req);

            SyncWorld(res.World);
            SyncWorldStage(res.WorldStage);
            SyncChgObjList(res.ChgObjList);
            return res;
        }

        public async Task<WorldRewardStarResponsePacket> RequestWorldRewardStar(int worldNum, int star)
        {
            var pakWorld = GetWorldForce(worldNum);
            var prtWorld = ProtoDb.Get<WorldProto>(worldNum);
            var valTotalStar = Player.WorldStageList.Where(x => x.WorldNum == worldNum).Sum(x => x.Star);
            var prtReward = new ObjValue(Proto.EObjType.FREE_CASH, 0, 0);
            for (var i = pakWorld.RecvStarReward + 1; i <= star; i++)
            {
                prtReward.Value += prtWorld.RewardStarCashList[i];
            }

            var req = new WorldRewardStarRequestPacket(worldNum, pakWorld.RecvStarReward, star, valTotalStar, prtReward);
            var res = await RpcSystem.RequestAsync<WorldRewardStarRequestPacket, WorldRewardStarResponsePacket>(req);

            SyncWorld(res.World);
            SyncChgObjList(res.ChgObjList);
            return res;
        }

        public void PrintWorldList()
        {
            foreach (var pakWorld in Player.WorldList)
            {
                var prtWorld = ProtoDb.Get<WorldProto>(pakWorld.Num);
                var valTotalStar = Player.WorldStageList.Where(x => x.WorldNum == pakWorld.Num).Sum(x => x.Star);
                var valRecvStarReward = pakWorld.RecvStarReward;
                var valStar = pakWorld.RecvStarReward;
                Console.WriteLine($"WorldNum:{pakWorld.Num}({prtWorld.Name}), RecvStar:{valStar}, TotalStar:{valTotalStar}, LastPlayNum({pakWorld.LastPlayStageNum}) TopFinishNum({pakWorld.TopFinishStageNum})");
            }
        }

        public void PrintWorldStageList()
        {
            foreach (var pakStage in Player.WorldStageList)
            {
                var prtWorld = ProtoDb.Get<WorldStageProto>(pakStage.Num);
                Console.WriteLine($"StageNum:{prtWorld.Num}-{prtWorld.Num}({prtWorld.Name}), Star:{pakStage.Star}");
            }
        }

    }
}
