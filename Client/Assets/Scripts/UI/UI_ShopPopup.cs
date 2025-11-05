using Proto;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ShopPopup : UI_Popup
{
    // Gacha
    private GameObject _gachaRoot;
    private GameObject _gachaScheduleGroup;
    private GameObject _selectedGachaRoot;


    protected override void InitImp()
    {
        base.InitImp();
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
      
    }

    private void UpMainCookiePanel(CookieProto cookiePrt)
    {
    }

    private void DownMainCookiePanel()
    {
    }
   
    struct GachaScheduleSlot
    {
        public Image Image;
        public TMP_Text Name;

        public void Bind(GameObject parentObject)
        {
            Image = UTIL.FindChild<Image>(parentObject);
            Name = UTIL.FindChild<TMP_Text>(parentObject, "Name");
        }
    }

    enum UI
    {
        GachaRoot,
        GachaScheduleGroup,
        GachaScheduleSlot,
        GachaScheduleSlotName,
        
        SelectedGachaCookieImage,
        SelectedGachaName,
        SelectedGachaPeriod,

        ProbButton,
        CostButtonGroup,
        CostButtonObject,

        GachaResultRoot,
        GachaResultGroup,
        GachaResultSlot,
        GachaResultCookieImage,
        GachaResultGradeImage,
        GachaResultGradeText,
    }
}
