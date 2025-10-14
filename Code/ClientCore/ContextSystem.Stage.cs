using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientCore
{
    public partial class ContextSystem
    {
        public async Task RequestWorldFinishFirstStage(int worldNum, int order, int star)
        {
            var prtStage = APP.Prt.GetWorldStagePrtListByMk(worldNum).Where(x => x.Order == order).First();
            var prtRewardList = new List<ObjValue>();
            for (var i = 0; i <= star; i++)
            {
                prtRewardList.AddOrInc(new ObjValue(prtStage.FirstRewardTypeList[i], prtStage.FirstRewardNumList[i], prtStage.FirstRewardAmountList[i]));
            }

            var req = new WorldFinishStageFirstReqPacket(prtStage.WorldNum, prtStage.Num, star, prtRewardList);
            var res = await _rpcSystem.RequestAsync<WorldFinishStageFirstReqPacket, WorldFinishStageFirstResPacket>(req);

            SyncWorld(res.World);
            SyncWorldStage(res.WorldStage);
            SyncChgObjList(res.ChgObjList);
        }

        public async Task RequestWorldFinishRepeatStage(int worldNum, int order, int star)
        {
            var prtStage = APP.Prt.GetWorldStagePrtListByMk(worldNum).Where(x => x.Order == order).First();
            var pakStage = GetWorldStageForce(prtStage.Num);
            var prtRewardList = new List<ObjValue>();
            for (var i = pakStage.Star + 1; i <= star; i++)
            {
                prtRewardList.AddOrInc(new ObjValue(prtStage.FirstRewardTypeList[i], prtStage.FirstRewardNumList[i], prtStage.FirstRewardAmountList[i]));
            }

            var req = new WorldFinishStageRepeatReqPacket(prtStage.WorldNum, prtStage.Num, star, prtRewardList);
            var res = await _rpcSystem.RequestAsync<WorldFinishStageRepeatReqPacket, WorldFinishStageRepeatResPacket>(req);

            SyncWorld(res.World);
            SyncWorldStage(res.WorldStage);
            SyncChgObjList(res.ChgObjList);
        }

        public async Task RequestWorldRewardStar(int worldNum, int star)
        {
            var pakWorld = GetWorldForce(worldNum);
            var prtWorld = APP.Prt.GetWorldPrt(worldNum);
            var valTotalStar = _player.WorldStageList.Where(x => x.WorldNum == worldNum).Sum(x => x.Star);
            var prtReward = new ObjValue(Proto.EObjType.FREE_CASH, 0, 0);
            for (var i = pakWorld.RecvStarReward + 1; i <= star; i++)
            {
                prtReward.Value += prtWorld.RewardStarCashList[i];
            }

            var req = new WorldRewardStarReqPacket(worldNum, pakWorld.RecvStarReward, star, valTotalStar, prtReward);
            var res = await _rpcSystem.RequestAsync<WorldRewardStarReqPacket, WorldRewardStarResPacket>(req);

            SyncWorld(res.World);
            SyncChgObj(res.ChgObj);
        }

        public void PrintWorldList()
        {
            foreach(var pakWorld in _player.WorldList)
            {
                var prtWorld = APP.Prt.GetWorldPrt(pakWorld.Num);
                var valTotalStar = _player.WorldStageList.Where(x => x.WorldNum == pakWorld.Num).Sum(x => x.Star);
                var valRecvStarReward = pakWorld.RecvStarReward;
                var valStar = pakWorld.RecvStarReward;
                Console.WriteLine($"WorldNum:{pakWorld.Num}({prtWorld.Name}), RecvStar:{valStar}, TotalStar:{valTotalStar}, LastPlayNum({pakWorld.LastPlayStageNum}) TopFinishNum({pakWorld.TopFinishStageNum})");
            }
        }

        public void PrintWorldStageList()
        {
            foreach (var pakStage in _player.WorldStageList)
            {
                var prtWorld = APP.Prt.GetWorldStagePrt(pakStage.Num);
                Console.WriteLine($"StageNum:{prtWorld.Num}-{prtWorld.Num}({prtWorld.Name}), Star:{pakStage.Star}");
            }
        }

    }
}
