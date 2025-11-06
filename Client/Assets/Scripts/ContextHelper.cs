using Proto;
using Protocol;
using System.Linq;
using UnityEngine.TerrainTools;

public static class ContextHelper
{
    public static CookiePacket GetCookie(int cookieNum)
    {
        var cookiePak = APP.Ctx.Player.CookieList.FirstOrDefault(x=>x.Num == cookieNum);
        if (cookiePak == null)
        {
            cookiePak = new CookiePacket
            {
                Num = cookieNum,
                Lv = 1,
                Star = 0,
                SoulStone = 0,
                AccSoulStone = 0,
                SkillLv = 1,
                State = Proto.ECookieState.NONE,
                Flag = 0,
            };
        }
        return cookiePak;
    }

    public static long GetPointAmount(int pointNum)
    {
        var pointPak = APP.Ctx.Player.PointList.FirstOrDefault(x => x.Num == pointNum);
        return pointPak == null ? 0 : (long)pointPak.Amount;
    }

    public static long GetTicketAmount(int ticketNum)
    {
        var ticketPak = APP.Ctx.Player.TicketList.FirstOrDefault(x => x.Num == ticketNum);
        return ticketPak == null ? 0 : (long)ticketPak.Amount;
    }

    public static long GetItemAmount(int itemNum)
    {
        var itemPak = APP.Ctx.Player.ItemList.FirstOrDefault(x => x.Num == itemNum);
        return itemPak == null ? 0 : (long)itemPak.Amount;
    }

    public static long GetObjAmount(EObjType objType)
    {
        switch (objType)
        {
            case EObjType.EXP:
                return (long)APP.Ctx.Player.Exp; // TODO: long으로 자료형 변경
            case EObjType.GOLD:
                return (long)APP.Ctx.Player.Gold;
            case EObjType.FREE_CASH:
                return (long)APP.Ctx.Player.FreeCash;
            case EObjType.REAL_CASH:
                return (long)APP.Ctx.Player.RealCash;
            case EObjType.TOTAL_CASH:
                return (long)APP.Ctx.Player.FreeCash + (long)APP.Ctx.Player.RealCash;
            case EObjType.POINT_MILEAGE:
            case EObjType.POINT_COOKIE_LV:
            case EObjType.POINT_C_GACHA_NORMAL:
            case EObjType.POINT_C_GACHA_SPECIAL:
            case EObjType.POINT_C_GACHA_DESTINY:
                return GetPointAmount((int)objType);
            case EObjType.TICKET_STAMINA:
                return GetTicketAmount((int)objType);
            default:
                LOG.E($"No Handling ObjType({objType}) Func({nameof(GetObjAmount)})");
                return 0;
        }
    }
}
