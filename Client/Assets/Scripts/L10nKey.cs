
using Proto;

public static class L10nKey
{
    public const string CookieGradeNamePrefix = "COOKIE_GRADE_NAME_";
    public const string CookieRollNamePrefix = "COOKIE_ROLL_NAME_";
    public const string CookieFormationNamePrefix = "COOKIE_FORMATION_NAME_";

    public static string GetCookieGradeText(EGradeType type)
    {
        var key = CookieGradeNamePrefix + type.ToString();
        return L10n.GetText(key);
    }

    public static string GetCookieRollText(ECookieRollType type)
    {
        var key = CookieRollNamePrefix + type.ToString();
        return L10n.GetText(key);
    }

    public static string GetCookieFormationText(EFormationPositionType type)
    {
        var key = CookieFormationNamePrefix + type.ToString();
        return L10n.GetText(key);
    }
}
