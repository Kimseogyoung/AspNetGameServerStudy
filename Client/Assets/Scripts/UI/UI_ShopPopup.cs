using Proto;
using System;
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

    public override void OnClose()
    {
        _loadingTask.Dispose();
        _loadingTask = null;
    }

    public override void OnOpen()
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
        for (var i = 0; i < _gachaScheduleSlotList.Count; i++)
        {
            var slot = _gachaScheduleSlotList[i];
            if (loadRes.ScheduleList.Count <= i)
            {
                slot.Activate(false);
                continue;
            }
            
            var gachaSchedule = loadRes.ScheduleList[i];
            var prtGachaSchedule = APP.Prt.GetGachaSchedulePrt(gachaSchedule.Num);
            slot.SetGacha(prtGachaSchedule, () => SelectGacha(prtGachaSchedule));
        }

        SelectGacha(_gachaScheduleSlotList[0].Prt);
    }

    private void SelectGacha(GachaScheduleProto prt)
    {
        LOG.I($"Select ({prt.Num})");
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
