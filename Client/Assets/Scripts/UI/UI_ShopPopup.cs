using Proto;
using Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UI_ShopPopup : UI_Popup
{
    // Gacha
    private GameObject _gachaRoot;
    private GameObject _gachaScheduleGroup;
    private List<GachaScheduleSlot> _gachaScheduleSlotList = new();

    private Image _selectedGachaBG;
    private Image _selectedGachaCookieImage;
    private TMP_Text _selectedGachaNameTxt;
    private TMP_Text _selectedGachaPeriodTxt;
    private UI_Button _probButton;
    private List<UI_CostButton> _costButtonList = new();

    private GameObject _gachaResultRoot;
    private Vector3 _gachaResultPopupOriginPos = Vector3.zero;
    private List<GachaResultSlot> _gachaResultSlotList= new();

    private Task _loadingTask;
    private List<SchedulePacket> _gachaSchedulePacketList = new();

    protected override void InitImp()
    {
        base.InitImp();

        _gachaRoot = Bind<GameObject>(UI.GachaRoot.ToString());
        _gachaScheduleGroup = Bind<GameObject>(UI.GachaScheduleGroup.ToString());
        var gachaScheduleSlotObjList = BindMany<GameObject>(UI.GachaScheduleSlot.ToString());
        foreach (var gachaScheduleSlotObj in gachaScheduleSlotObjList)
        {
            var gachaScheduleSlot = new GachaScheduleSlot();
            gachaScheduleSlot.Bind(gachaScheduleSlotObj);
            _gachaScheduleSlotList.Add(gachaScheduleSlot);
        }

        _selectedGachaBG = Bind<Image>(UI.SelectedGachaBG.ToString());
        _selectedGachaCookieImage = Bind<Image>(UI.SelectedGachaCookieImage.ToString());
        _selectedGachaNameTxt = Bind<TMP_Text>(UI.SelectedGachaName.ToString());
        _selectedGachaPeriodTxt = Bind<TMP_Text>(UI.SelectedGachaPeriod.ToString());
        _probButton = Bind<UI_Button>(UI.ProbButton.ToString());
        _costButtonList = BindMany<UI_CostButton>(UI.CostButtonObject.ToString());

        _gachaResultRoot = Bind<GameObject>(UI.GachaResultRoot.ToString());
        _gachaResultPopupOriginPos = _gachaResultRoot.gameObject.transform.localPosition;
        Bind<UI_Button>(UI.GachaResultExitButton.ToString()).SetEvent(CloseGachaResult);

         var gachaResultSlotObjList = BindMany<GameObject>(UI.GachaResultSlot.ToString());
        foreach (var gachaResultSlotObj in gachaResultSlotObjList)
        {
            var gachaResultSlot = new GachaResultSlot();
            gachaResultSlot.Bind(gachaResultSlotObj);
            _gachaResultSlotList.Add(gachaResultSlot);
        }

    }

    protected override void OnClose()
    {
        _loadingTask.Dispose();
        _loadingTask = null;

        _probButton.RemoveAllEvent();

        foreach (var costButton  in _costButtonList)
        {
            costButton.SetCost(EObjType.NONE, 0);
        }
    }

    protected override void OnOpen()
    {
        if (_loadingTask != null)
        {
            return;
        }

        
        _loadingTask = Refresh();
        _loadingTask.FireAndForget();
    }

    public async Task Refresh()
    {
        // 임시 
        await APP.Ctx.RequestCheatReward(new ObjValue(EObjType.POINT_C_GACHA_NORMAL, 0, 100));


        var loadRes = await APP.Ctx.RequestLoadSchedule();
        _gachaSchedulePacketList = loadRes.ScheduleList;

        for (var i = 0; i < _gachaScheduleSlotList.Count; i++)
        {
            var slot = _gachaScheduleSlotList[i];
            if (loadRes.ScheduleList.Count <= i)
            {
                slot.Activate(false);
                continue;
            }
            
            var gachaSchedule = _gachaSchedulePacketList[i];
            var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(gachaSchedule.Num);
            var idx = i;
            slot.SetGacha(prtGachaSchedule, () => SelectGacha(idx));
        }

        // 첫번째 스케쥴 선택 상태로 만듦.
        SelectGacha(0);
    }

    private void SelectGacha(int scheduleIdx)
    {
        if(scheduleIdx >= _gachaSchedulePacketList.Count)
        {
            LOG.E($"Invalid GachaSchedule Index({scheduleIdx}) ScheduleCount({_gachaSchedulePacketList.Count})");
            return;
        }

        var schedulePtk = _gachaSchedulePacketList[scheduleIdx];
        var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(schedulePtk.Num);
        _selectedGachaBG.sprite = UTIL.LoadSprite(prtGachaSchedule.BGSprite);
        _selectedGachaNameTxt.text = L10n.GetText(prtGachaSchedule.NameKey);
        _selectedGachaPeriodTxt.text = $"{L10n.GetPeriodText(schedulePtk.ActiveStartTime)} ~ {L10n.GetPeriodText(schedulePtk.ActiveEndTime)}";

        if (prtGachaSchedule.PickupCookieNumList.Any(x=>x != 0)) // 픽업
        {
            _selectedGachaCookieImage.gameObject.SetActive(true);

            var cookiePrt = APP.Prt.GetCookiePrt(prtGachaSchedule.PickupCookieNumList[0]);
            _selectedGachaCookieImage.sprite = UTIL.LoadSprite(cookiePrt.Sprite);
        }
        else
        {
            _selectedGachaCookieImage.gameObject.SetActive(false);
        }

        _probButton.RemoveAllEvent();
        _probButton.SetEvent(() => ShowGachaProb(prtGachaSchedule));

        // Cost 버튼 세팅
        for (var costIdx = 0; costIdx < prtGachaSchedule.CostTypeList.Count; costIdx++)
        {
            var costType = prtGachaSchedule.CostTypeList[costIdx];
            var costAmount = prtGachaSchedule.CostAmountList[costIdx];

            for (var cntIdx = 0; cntIdx < prtGachaSchedule.CntList.Count; cntIdx++)
            {
                var cnt = prtGachaSchedule.CntList[cntIdx];
                var buttonIdx = (costIdx * prtGachaSchedule.CntList.Count) + cntIdx;
                if (buttonIdx >= _costButtonList.Count)
                {
                    LOG.E($"Not Enough Gacha CostButton ButtonIdx({buttonIdx})");
                    continue;
                }
                var costButton = _costButtonList[buttonIdx];
                costButton.SetCost(costType, costAmount);
                costButton.Button.SetText($"{cnt}회 뽑기"); // TODO: L10n
                costButton.Button.SetEvent(() => RunGacha(prtGachaSchedule, costType, costAmount, cnt).FireAndForget());
            }
        }
    }

    private async Task RunGacha(GachaScheduleProto prt, EObjType costType, int costAmount, int cnt)
    {
        LOG.I($"RunGacha {prt.Num}");
        var res = await APP.Ctx.RequestGachaNormal(prt.Num, costType, costAmount, cnt);
        if (APP.Ctx.IsErrorRes(res))
        {
            return;
        }

        ShowGachaResult(prt, res.GachaResultList);// GachaResultList 가챠 결과
        var gachaResultObjValueList = res.GachaResultList; // GachaResultObjValueList 가챠 결과
        if (gachaResultObjValueList.Count > _gachaResultSlotList.Count)
        {
            // 표시 가능한 슬롯보다 많이 뽑는 경우는 오류
            LOG.E($"Too Many GachaResult Cnt({gachaResultObjValueList.Count})");
            return;
        }

        for (var i = 0; i < _gachaResultSlotList.Count; i++)
        {
            var gachaResultSlot = _gachaResultSlotList[i];
            if (i >= gachaResultObjValueList.Count)
            {
                gachaResultSlot.Disable();
                continue;
            }

            gachaResultSlot.SetResult(gachaResultObjValueList[i]);
        }
    }

    private void ShowGachaProb(GachaScheduleProto prt)
    {
        LOG.I($"ShowProbGacha {prt.Num}");
    }

    private void ShowGachaResult(GachaScheduleProto prt, List<GachaResultPacket> gachaResultPktList)
    {
        _gachaResultRoot.gameObject.transform.localPosition = Vector3.zero;

        if (gachaResultPktList.Count > _gachaResultSlotList.Count)
        {
            // 표시 가능한 슬롯보다 많이 뽑는 경우는 오류
            LOG.E($"Too Many GachaResult Cnt({gachaResultPktList.Count})");
            return;
        }

        for (var i = 0; i < _gachaResultSlotList.Count; i++)
        {
            var gachaResultSlot = _gachaResultSlotList[i];
            if (i >= gachaResultPktList.Count)
            {
                gachaResultSlot.Disable();
                continue;
            }

            gachaResultSlot.SetResult(gachaResultPktList[i]);
        }
    }

    private void CloseGachaResult()
    {
        _gachaResultRoot.gameObject.transform.localPosition = _gachaResultPopupOriginPos;
    }

    private void UpGachaResultPanel()
    {
    }

    private void DownGachaResultPanel()
    {
    }
   
    class GachaScheduleSlot
    {
        public GachaScheduleProto Prt;
        public Image Image;
        public TMP_Text Name;
        public Button Button;

        private GameObject _gameObject;

        public void SetGacha(GachaScheduleProto prt, UnityAction action)
        {
            Activate(true);

            Prt = prt;
            Name.text = L10n.GetText(Prt.NameKey);
            Image.sprite = UTIL.LoadSprite(Prt.BGSprite);

            Button.onClick.RemoveAllListeners();
            Button.onClick.AddListener(action);
        }

        public void Bind(GameObject gameObject)
        {
            _gameObject = gameObject;

            Image = UTIL.GetComponent<Image>(_gameObject);
            Button = UTIL.GetComponent<Button>(_gameObject);
            Name = UTIL.FindChild<TMP_Text>(_gameObject, nameof(Name));
        }

        public void Destroy()
        {
            Button.onClick.RemoveAllListeners();
        }

        public void Activate(bool activate)
        {
            _gameObject.SetActive(activate);
        }
    }

    class GachaResultSlot 
    {
        public Image Image;
        public Image GradeBGImage;
        public Image GradeImage;
        public TMP_Text GradeText;
        public Image SoulStoneIconImage;
        public TMP_Text SoulStoneCntText;
        public TMP_Text SoulStoneHasCntText;

        private GameObject _gameObject;

        public void Bind(GameObject gameObject)
        {
            _gameObject = gameObject;

            Image = UTIL.FindChild<Image>(_gameObject, nameof(Image));
            GradeBGImage = UTIL.FindChild<Image>(_gameObject, nameof(GradeBGImage));
            GradeImage = UTIL.FindChild<Image>(_gameObject, nameof(GradeImage));
            GradeText = UTIL.FindChild<TMP_Text>(_gameObject, nameof(GradeText));
            SoulStoneIconImage = UTIL.FindChild<Image>(_gameObject, nameof(SoulStoneIconImage));
            SoulStoneCntText = UTIL.FindChild<TMP_Text>(_gameObject, nameof(SoulStoneCntText));
            SoulStoneHasCntText = UTIL.FindChild<TMP_Text>(_gameObject, nameof(SoulStoneHasCntText));
        }

        public void SetResult(GachaResultPacket gachaResult)
        {
            var image = IconHelper.GetFullImage(gachaResult.ResultObjValue.Key);
            Image.sprite = image;

            var soulStonePrt = APP.Prt.GetCookieSoulStonePrt(gachaResult.SoulStoneNum);
            var soulStoneCnt = gachaResult.SoulStoneAmount;
            var cookiePrt = APP.Prt.GetCookiePrt(soulStonePrt.CookieNum);
            var cookiePkt = ContextHelper.GetCookie(cookiePrt.Num);

            var prtCookieStarEnhance = APP.Prt.GetCookieStarEnhancePrt(cookiePrt.GradeType, cookiePkt.Star);

            var grade = cookiePrt.GradeType;
            GradeImage.sprite = IconHelper.GetGradeIconImage(grade);
            GradeText.text = L10nKey.GetCookieGradeText(grade);

            SoulStoneIconImage.sprite = IconHelper.GetIconImage(new ObjKey(EObjType.SOUL_STONE, soulStonePrt.Num));
            SoulStoneCntText.text = $"x{soulStoneCnt}";
            SoulStoneHasCntText.text = $"{cookiePkt.SoulStone}/{prtCookieStarEnhance.SoulStone}";
            _gameObject.SetActive(true);
        }

        public void Disable()
        {
            _gameObject.SetActive(false);
        }
    }

    enum UI
    {
        GachaRoot,
        GachaScheduleGroup,
        GachaScheduleSlot,

        SelectedGachaBG,
        SelectedGachaCookieImage,
        SelectedGachaName,
        SelectedGachaPeriod,

        ProbButton,
        CostButtonGroup,
        CostButtonObject,

        GachaResultRoot,
        GachaResultGroup,
        GachaResultSlot,
        GachaResultExitButton,
    }
}
