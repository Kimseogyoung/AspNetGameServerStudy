using NUnit.Framework;
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

    protected override void InitImp()
    {
        base.InitImp();

        _cookieSelectGroupGO = Bind<GameObject>(UI.CookieSelectGroup.ToString());

        var slotList = UTIL.FindChildAll(_cookieSelectGroupGO);
        foreach (var slot in slotList)
        {
            if (_cookieSelectGroupGO != slot)
            {
                UTIL.Destroy(slot);
            }
        }

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
        LOG.I($"cookie ({cookiePrt.Name})");
        _cookieMainGO.transform.localPosition = new Vector3(0, 0, 0);
    }

    private void DownMainCookiePanel()
    {
        _cookieMainGO.transform.localPosition = _cookieMainOriginPos;
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
        CookieName,
        CookieLv,
        CookieStar,
        CookieStarExp
    }
}
