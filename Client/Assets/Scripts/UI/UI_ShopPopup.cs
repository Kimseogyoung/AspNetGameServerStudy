using Proto;
using Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

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
    private GameObject _costButtonGroup;
    private List<UI_CostButton> _costButtonList = new();

    private GameObject _gachaResultRoot;
    private GameObject _gachaResultGroup;

    private GameObject _gachaResultSlotPrefab;

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
        _costButtonGroup = Bind<GameObject>(UI.CostButtonGroup.ToString());
        _costButtonList = BindMany<UI_CostButton>(UI.CostButtonObject.ToString());

        _gachaResultRoot = Bind<GameObject>(UI.GachaResultRoot.ToString());
        _gachaResultGroup = Bind<GameObject>(UI.GachaResultGroup.ToString());
        _gachaResultSlotPrefab = UTIL.LoadRes<GameObject>(AppPath.GetPrefabPath("GachaResultSlot"));
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

        if (prtGachaSchedule.PickupCookieNumList.Any(x=>x != 0)) // ÇÈ¾÷
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
                costButton.Button.SetText($"{cnt}È¸ »Ì±â");
                costButton.Button.SetEvent(() => RunGacha(prtGachaSchedule, costType, costAmount, cnt));
            }
        }
    }

    private void RunGacha(GachaScheduleProto prt, EObjType costType, int costAmount, int cnt)
    {
        LOG.I($"RunGacha {prt.Num}");
    }

    private void ShowGachaProb(GachaScheduleProto prt)
    {
        LOG.I($"ShowProbGacha {prt.Num}");
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
        public Image CookieImage;
        public Image GradeBGImage;
        public TMP_Text GradeName;
        public TMP_Text GradeTxt;

        private GameObject _gameObject;
        public void Bind(GameObject gameObject)
        {
            _gameObject = gameObject;

            CookieImage = UTIL.FindChild<Image>(_gameObject, nameof(CookieImage));
            GradeBGImage = UTIL.FindChild<Image>(_gameObject, nameof(GradeBGImage));
            GradeName = UTIL.FindChild<TMP_Text>(_gameObject, nameof(GradeName));
            GradeTxt = UTIL.FindChild<TMP_Text>(_gameObject, nameof(GradeTxt));
        }

        public void Activate(bool activate)
        {
            _gameObject.SetActive(activate);
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
    }
}
