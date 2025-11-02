
using Proto;

public static class L10n
{
    public enum ELanguage
    {
        NONE,
        KO,
        EN
    }

    public static ELanguage Language { get; private set; } = ELanguage.KO; // TODO: 설정에서 컨트롤하고, 인게임에서 변경 가능하도록

    public static string GetText(string key)
    {
        if (!APP.Prt.TryGetLocalizationPrt(key, out var l10nPrt))
        {
            LOG.W($"Not Found L10n Key({key})");
            return key;
        }

        return GetLanTextFromL10nPrt(l10nPrt);
    }

    private static string GetLanTextFromL10nPrt(LocalizationProto prt)
    {
        switch (Language)
        {
            case ELanguage.KO:
                return prt.ko;
            case ELanguage.EN:
                return prt.en;
            default:
                LOG.W($"No Handling L10n Language({Language})");
                return prt.Key;
        }
    }
}
