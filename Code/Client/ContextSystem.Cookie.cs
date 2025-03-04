using NLog.LayoutRenderers.Wrappers;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public partial class ContextSystem
    {
        public async Task RequestLoadSchedule()
        {
            var req = new ScheduleLoadReqPacket();
            var res = await _rpcSystem.RequestAsync<ScheduleLoadReqPacket, ScheduleLoadResPacket>(req);

            SyncScheduleList(res.ScheduleList);
        }

        public async Task RequestGachaNormal(int scheduleNum, int cnt)
        {
            var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(scheduleNum);

            var costIdx = prtGachaSchedule.CntList.FindIndex(x => x == cnt);
            if(costIdx == -1)
            {
                Console.WriteLine($"INVALID_GACHA_CNT({cnt})");
                return;
            }

            var req = new GachaNormalReqPacket
            {
                ScheduleNum = scheduleNum,
                Cnt = cnt,
                CostObj = new CostObjPacket { Type = prtGachaSchedule.CostTypeList[costIdx], Num = 0, Amount = prtGachaSchedule.CostAmountList[costIdx] * cnt },
            };

            var res = await _rpcSystem.RequestAsync<GachaNormalReqPacket, GachaNormalResPacket>(req);

            SyncChgObjList(res.GachaResultChgObjList);
            SyncChgObj(res.CostChgObj);
        }
    }
}
