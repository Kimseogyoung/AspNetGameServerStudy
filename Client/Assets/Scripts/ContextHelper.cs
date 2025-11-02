using Protocol;
using System.Linq;

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
}
