using Proto;
using Protocol;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ClientCore
{
    public partial class ContextSystem
    {
        public async Task<CheatRewardResPacket> RequestCheatReward(string objTypeStr, int objNum, int objAmount)
        {
            var upperCashObjTypeStr = objTypeStr.ToUpper();
            var objTypeList = new List<EObjType>();
            var canParse = Enum.TryParse(typeof(EObjType), upperCashObjTypeStr, out var parseObjType);

            if (canParse)
            {
                objTypeList.Add((EObjType)parseObjType);
            }
            else if (string.IsNullOrEmpty(objTypeStr))
            {
                foreach (var type in Enum.GetValues(typeof(EObjType)))
                {
                    objTypeList.Add((EObjType)type);
                }
            }
            else
            {
                switch (upperCashObjTypeStr)
                {
                    case "POINT":
                        for (var i = EObjType.POINT_START + 1; i < EObjType.POINT_END; i++)
                        {
                            objTypeList.Add(i);
                        }
                        break;
                    case "TICKET":
                        for (var i = EObjType.TICKET_START + 1; i < EObjType.TICKET_END; i++)
                        {
                            objTypeList.Add(i);
                        }
                        break;
                }
            }

            var reqRewardList = new List<ObjValue>();
            foreach (var objType in objTypeList)
            {
                switch (objType)
                {
                    case EObjType.EXP:
                    case EObjType.GOLD:
                    case EObjType.FREE_CASH:
                    case EObjType.POINT_MILEAGE:
                    case EObjType.POINT_COOKIE_LV:
                    case EObjType.POINT_C_GACHA_NORMAL:
                    case EObjType.POINT_C_GACHA_SPECIAL:
                    case EObjType.POINT_C_GACHA_DESTINY:
                    case EObjType.TICKET_STAMINA:
                        reqRewardList.Add(new ObjValue(objType, 0, objAmount));
                        break;
                    case EObjType.COOKIE:
                        foreach (var cookiePrt in APP.Prt.GetCookiePrts())
                        {
                            reqRewardList.Add(new ObjValue(objType, cookiePrt.Num, objAmount));
                        }
                        break;
                    case EObjType.ITEM:
                        foreach (var itemPrt in APP.Prt.GetItemPrts())
                        {
                            reqRewardList.Add(new ObjValue(objType, itemPrt.Num, objAmount));
                        }
                        break;
                }
            }

            return await RequestCheatReward(reqRewardList);
        }

        public async Task<CheatRewardResPacket> RequestCheatReward(ObjValue objValue)
        {
            return await RequestCheatReward(new List<ObjValue>() { objValue });
        }

        public async Task<CheatRewardResPacket> RequestCheatReward(List<ObjValue> objValueList)
        {
            var req = new CheatRewardReqPacket { RewardList = objValueList };
            var res = await RpcSystem.RequestAsync<CheatRewardReqPacket, CheatRewardResPacket>(req);

            SyncChgObjList(res.ChgObjList);
            return res;
        }

    }
}
