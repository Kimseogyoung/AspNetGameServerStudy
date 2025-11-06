using Proto;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CookiePopup : UI_Popup
{
    private GameObject _cookieSelectGroupGO;
    private GameObject _cookieMainGO;
    private Vector3 _cookieMainOriginPos;
    private Dictionary<int, CookieSlot> _cookieSlotDict = new();
    private UI_Button _cookieMainExitButton;

    private Image _cookieMainImage;
    private TMP_Text _cookieMainNameTxt;
    private TMP_Text _cookieMainLvTxt;
    private TMP_Text _cookieMainStarTxt;
    private TMP_Text _cookieMainSoulStoneTxt;
    private TMP_Text _cookieMainRollTxt;
    private TMP_Text _cookieMainFormationTxt;
    private TMP_Text _cookieMainGradeTxt;
    private UI_Button _cookieMainLvUpButton;
    private UI_Button _cookieMainStarUpButton;

    protected override void InitImp()
    {
        base.InitImp();

        _cookieSelectGroupGO = Bind<GameObject>(UI.CookieSelectGroup.ToString());
        UTIL.DestoryChildren(_cookieSelectGroupGO);

        var cookieSelectSlotPrefab = UTIL.LoadRes<GameObject>(AppPath.GetPrefabPath(UI.CookieSelectSlot.ToString()));
        foreach (var cookiePrt in APP.Prt.GetCookiePrts())
        {
            var cookieSelectSlot = UTIL.Instantiate(cookieSelectSlotPrefab, _cookieSelectGroupGO);
            cookieSelectSlot.transform.localScale = Vector3.one;

            var button = UTIL.FindChild<Button>(cookieSelectSlot, UI.CookieSelectProfile.ToString());
            var image = UTIL.FindChild<Image>(cookieSelectSlot, UI.CookieSelectProfile.ToString());
            var starGroup = UTIL.FindChild<UI_StarGroup>(cookieSelectSlot);
            var gradeText = UTIL.FindChild<TMP_Text>(cookieSelectSlot);

            starGroup.Init();
            gradeText.text = cookiePrt.GradeType.ToString();

            starGroup.SetMaxStarCnt(5);
            image.sprite = UTIL.LoadSprite(cookiePrt.Sprite);

            button.onClick.AddListener(() => { UpMainCookiePanel(cookiePrt); });

            _cookieSlotDict.Add(cookiePrt.Num,
                new CookieSlot
                {
                    Button = button,
                    CookieProfileImage = image,
                    StarGroup = starGroup,
                    GradeText = gradeText,
                    GradeImage = null // TODO: 
                });
        }

        // Main
        _cookieMainGO = Bind<GameObject>(UI.CookieMainGO.ToString());
        _cookieMainOriginPos = _cookieMainGO.transform.localPosition;

        _cookieMainImage = Bind<Image>(UI.CookieFullImage.ToString());
        _cookieMainNameTxt = Bind<TMP_Text>(UI.CookieNameTxt.ToString());
        _cookieMainLvTxt = Bind<TMP_Text>(UI.CookieLvTxt.ToString());
        _cookieMainStarTxt = Bind<TMP_Text>(UI.CookieStarTxt.ToString());
        _cookieMainSoulStoneTxt = Bind<TMP_Text>(UI.CookieSoulStoneTxt.ToString());
        _cookieMainRollTxt = Bind<TMP_Text>(UI.CookieRollTxt.ToString());
        _cookieMainFormationTxt = Bind<TMP_Text>(UI.CookieFormationTxt.ToString());
        _cookieMainGradeTxt = Bind<TMP_Text>(UI.CookieGradeTxt.ToString());
        _cookieMainLvUpButton = Bind<UI_Button>(UI.CookieLvUpButton.ToString());
        _cookieMainStarUpButton = Bind<UI_Button>(UI.CookieStarUpButton.ToString());

        _cookieMainExitButton = Bind<UI_Button>(UI.CookieMainExitButton.ToString());
        _cookieMainExitButton.AddEvent(() => { DownMainCookiePanel(); });
    }

    public override void OnClose()
    {
    }

    public override void OnOpen()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (var (cookieNum, cookieSlot) in _cookieSlotDict)
        {
            var cookieCtx = APP.Ctx.Player.CookieList.FirstOrDefault(x=>x.Num == cookieNum);
            var hasCookie = cookieCtx?.AccSoulStone == 0;
            cookieSlot.StarGroup.SetStarCnt(cookieCtx?.Star ?? 0);
            cookieSlot.CookieProfileImage.color = hasCookie ? DEF.C_WHITE : DEF.C_CLEAR_BLOCK;
        }
    }

    private void UpMainCookiePanel(CookieProto cookiePrt)
    {
        var cookieCtx = ContextHelper.GetCookie(cookiePrt.Num);
        _cookieMainGO.transform.localPosition = new Vector3(0, 0, 0);

        _cookieMainImage.sprite = UTIL.LoadSprite(cookiePrt.Sprite);
        _cookieMainNameTxt.text = L10n.GetText(cookiePrt.NameKey);
        _cookieMainLvTxt.text = $"Lv {cookieCtx.Lv}";
        _cookieMainStarTxt.text = $"Star {cookieCtx.Star}";
        _cookieMainSoulStoneTxt.text = $"SoulStone {cookieCtx.SoulStone}";
        _cookieMainRollTxt.text = L10nKey.GetCookieRollText(cookiePrt.RollType);
        _cookieMainFormationTxt.text = L10nKey.GetCookieFormationText(cookiePrt.FormationPosType);
        _cookieMainGradeTxt.text = L10nKey.GetCookieGradeText(cookiePrt.GradeType);

        _cookieMainLvUpButton.AddEvent(() => { OnClickCookieLvUpButton(cookiePrt); });
        _cookieMainStarUpButton.AddEvent(() => { OnClickCookieStarUpButton(cookiePrt); });
    }

    private void DownMainCookiePanel()
    {
        _cookieMainGO.transform.localPosition = _cookieMainOriginPos;

        _cookieMainLvUpButton.RemoveAllEvent();
        _cookieMainStarUpButton.RemoveAllEvent();
    }
   
    private void OnClickCookieLvUpButton(CookieProto cookiePrt)
    {
        LOG.I($"OnClickCookieLvUpButton ({cookiePrt.Name})");
    }

    private void OnClickCookieStarUpButton(CookieProto cookiePrt)
    {
        LOG.I($"OnClickCookieStarUpButton ({cookiePrt.Name})");
    }

    struct CookieSlot
    {
        public Button Button;
        public Image CookieProfileImage;
        public UI_StarGroup StarGroup;
        public TMP_Text GradeText;
        public Image GradeImage;
    }

    enum UI
    {
        CookieSelectGroup,
        CookieSelectSlot,
        CookieMainGO,
        CookieMainExitButton,
        CookieSelectProfile,

        CookieFullImage,
        CookieNameTxt,
        CookieLvTxt,
        CookieStarTxt,
        CookieSoulStoneTxt,
        CookieRollTxt,
        CookieFormationTxt,
        CookieGradeTxt,
        CookieLvUpButton,
        CookieStarUpButton,
    }
}
