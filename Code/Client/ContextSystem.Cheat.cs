using Proto;
using Protocol;
namespace Client
{
    public partial class ContextSystem
    {
        public async Task RequestCheatReward(string objTypeStr, int objNum, int objAmount)
        {
            var upperCashObjTypeStr = objTypeStr.ToUpper();
            var objTypeArr = Enum.GetValues<EObjType>();
            var canParse = Enum.TryParse(typeof(EObjType), upperCashObjTypeStr, out var parseObjType);

            if (canParse)
            {
                objTypeArr = new EObjType[] { (EObjType)parseObjType };
            }
        
            var reqRewardList = new List<ObjPacket>();
            foreach (var objType in objTypeArr)
            {
                switch (objType)
                {
                    case EObjType.EXP:
                    case EObjType.GOLD:
                    case EObjType.FREE_CASH:
                    case EObjType.POINT_MILEAGE:
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
            var res = await _rpcSystem.RequestAsync<CheatRewardReqPacket, CheatRewardResPacket>(req);

            SyncChgObjList(res.ChgObjList);
        }
    }
}
