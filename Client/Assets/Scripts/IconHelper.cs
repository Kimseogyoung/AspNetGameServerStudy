using Proto;
using Protocol;
using System.Collections.Generic;
using UnityEngine;

public static class IconHelper
{
    private static Dictionary<ObjKey, Sprite> _cachedFullImageDict = new Dictionary<ObjKey, Sprite>();
    private static Dictionary<ObjKey, Sprite> _cachedIconDict = new Dictionary<ObjKey, Sprite>();
    private static Dictionary<EGradeType, Sprite> _cachedGradeIconDict = new Dictionary<EGradeType, Sprite>();
    public static Sprite GetIconImage(ObjKey objKey)
    {
        if (_cachedIconDict.TryGetValue(objKey, out var cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        var spritePath = "";
        switch (objKey.Type)
        {
            case EObjType.EXP:
            case EObjType.GOLD:
            case EObjType.FREE_CASH:
            case EObjType.REAL_CASH:
            case EObjType.TOTAL_CASH:

            case EObjType.POINT_MILEAGE:
            case EObjType.POINT_COOKIE_LV:
            case EObjType.POINT_C_GACHA_NORMAL:
            case EObjType.POINT_C_GACHA_SPECIAL:
            case EObjType.POINT_C_GACHA_DESTINY:

            case EObjType.TICKET_STAMINA:

            case EObjType.SOUL_STONE:
                return GetFullImage(objKey);
            case EObjType.COOKIE:
                var prtCookie = APP.Prt.GetCookiePrt(objKey.Num);
                spritePath = prtCookie.IconSprite;
                break;
            case EObjType.ITEM:
                // Icon? 필요할지 추후 검토
                var prtItem = APP.Prt.GetItemPrt(objKey.Num);
                spritePath = prtItem.Sprite;
                break;
            case EObjType.KINGDOM_ITEM:
                // Icon? 필요할지 추후 검토
                var prtKingdomItem = APP.Prt.GetKingdomItemPrt(objKey.Num);
                spritePath = prtKingdomItem.Sprite;
                break;
            default:
                LOG.E($"No Handling ObjType({objKey.Type})");
                break;
        }


        var loadedSprite = UTIL.LoadSprite(spritePath);
        if (loadedSprite == null)
        {
            LOG.E($"Failed to load Icon Sprite ObjKey({objKey})");
            return null;
        }

        _cachedIconDict.Add(objKey, loadedSprite);
        return loadedSprite;
    }

    public static Sprite GetFullImage(ObjKey objKey)
    {
        if (_cachedFullImageDict.TryGetValue(objKey, out var cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        var spritePath = "";
        switch (objKey.Type)
        {
            case EObjType.EXP:
                spritePath = "Icon1#Icon1_18";
                break;
            case EObjType.GOLD:
                spritePath = "Icon1#Icon1_0";
                break;
            case EObjType.FREE_CASH:
            case EObjType.REAL_CASH:
            case EObjType.TOTAL_CASH:
                spritePath = "Icon1#Icon1_12";
                break;
            case EObjType.POINT_MILEAGE:
            case EObjType.POINT_COOKIE_LV:
            case EObjType.POINT_C_GACHA_NORMAL:
            case EObjType.POINT_C_GACHA_SPECIAL:
            case EObjType.POINT_C_GACHA_DESTINY:
                var prtPoint = APP.Prt.GetPointPrt(objKey.Type);
                spritePath = prtPoint.Sprite;
                break;
            case EObjType.TICKET_STAMINA:
                var prtTicket = APP.Prt.GetTicketPrt(objKey.Type);
                spritePath = prtTicket.Sprite;
                break;
            case EObjType.COOKIE:
                var prtCookie = APP.Prt.GetCookiePrt(objKey.Num);
                spritePath = prtCookie.Sprite;
                break;
            case EObjType.SOUL_STONE:
                var prtCookieSoulStone = APP.Prt.GetCookieSoulStonePrt(objKey.Num);
                spritePath = prtCookieSoulStone.Sprite;
                break;
            case EObjType.ITEM:
                var prtItem = APP.Prt.GetItemPrt(objKey.Num);
                spritePath = prtItem.Sprite;
                break;
            case EObjType.KINGDOM_ITEM:
                var prtKingdomItem = APP.Prt.GetKingdomItemPrt(objKey.Num);
                spritePath = prtKingdomItem.Sprite;
                break;
            default:
                LOG.E($"No Handling ObjType({objKey.Type})");
                break;
        }


        var loadedSprite = UTIL.LoadSprite(spritePath);
        if (loadedSprite == null)
        {
            LOG.E($"Failed to load Icon Sprite ObjKey({objKey})");
            return null;
        }

        _cachedIconDict.Add(objKey, loadedSprite);
        return loadedSprite;
    }

    public static Sprite GetGradeIconImage(EGradeType gradeType)
    {
        if (_cachedGradeIconDict.TryGetValue(gradeType, out var cachedSprite) && cachedSprite != null)
        {
            return cachedSprite;
        }

        var spritePath = "Icon1#Icon1_13";
        switch (gradeType)
        {
            case EGradeType.COMMON:
                break;
            case EGradeType.RARE:
                break;
            case EGradeType.EPIC:
                break;
            case EGradeType.SUPER_EPIC:
                break;
            case EGradeType.ANCIENT:
                break;
            case EGradeType.LEGENDARY:
                break;
            default:
                LOG.E($"No Handling Grade({gradeType})");
                break;
        }

        var loadedSprite = UTIL.LoadSprite(spritePath);
        if (loadedSprite == null)
        {
            LOG.E($"Failed to load Icon Sprite GradeType({gradeType})");
            return null;
        }

        _cachedGradeIconDict.Add(gradeType, loadedSprite);
        return loadedSprite;
    }
}
