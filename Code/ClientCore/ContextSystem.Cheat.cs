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
                foreach(var type in Enum.GetValues(typeof(EObjType)))
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
        
            var reqRewardList = new List<ObjPacket>();
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
                        reqRewardList.Add(new ObjPacket { Type = objType, Num = 0, Amount = objAmount });
                        break;
                    case EObjType.COOKIE:
                        foreach (var cookiePrt in APP.Prt.GetCookiePrts())
                        {
                            reqRewardList.Add(new ObjPacket { Type = objType, Num = cookiePrt.Num, Amount = objAmount });
                        }
                        break;
                    case EObjType.ITEM:
                        foreach (var itemPrt in APP.Prt.GetItemPrts())
                        {
                            reqRewardList.Add(new ObjPacket { Type = objType, Num = itemPrt.Num, Amount = objAmount });
                        }
                        break;
                }
            }

            var req = new CheatRewardReqPacket { RewardList = reqRewardList };
            var res = await RpcSystem.RequestAsync<CheatRewardReqPacket, CheatRewardResPacket>(req);

            SyncChgObjList(res.ChgObjList);
            return res;
        }
    }
}
