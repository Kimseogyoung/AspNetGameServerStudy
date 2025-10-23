using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGameScene : UI_Scene
{
    //private List<GameObject> _stageButtonPoints;
    //private List<TMP_Text> _stageButtonTexts;
    //private List<int> _stageButtonNumList;
    private UI_Button _menuButton;
    private UI_Button _mailButton;
    private UI_Button _friendButton;
    private UI_Button _editButton;
    private UI_Button _shopButton;

    private GameObject _pocketGroupRootGO;
    private UI_PocketSlot _cashPocketSlot;
    private UI_PocketSlot _goldPocketSlot;

    // Profile
    private Image _profileCookieImage;
    private TMP_Text _profileNameText;
    private TMP_Text _lvText;
    private TMP_Text _expText;
    private TMP_Text _castleLvText;

    private GameObject _effRootGO;
    private Image _effHideImage;

    protected override void OnDestroyed()
    {

    }

    protected override void InitImp()
    {
        base.InitImp();

        _pocketGroupRootGO = Bind<GameObject>(UI.PocketGroupRoot.ToString());

        UTIL.TryCreateInstance<UI_PocketSlot>(out _, "UI/PocketSlotObject", _pocketGroupRootGO, "CashSlot");
        UTIL.TryCreateInstance<UI_PocketSlot>(out _, "UI/PocketSlotObject", _pocketGroupRootGO, "GoldSlot");
        _cashPocketSlot = Bind<UI_PocketSlot>("CashSlot");
        _goldPocketSlot = Bind<UI_PocketSlot>("GoldSlot");

        _menuButton = Bind<UI_Button>(UI.MenuButton.ToString());
        _mailButton = Bind<UI_Button>(UI.MailButton.ToString());
        _friendButton = Bind<UI_Button>(UI.FriendButton.ToString());
        _editButton = Bind<UI_Button>(UI.EditButton.ToString());
        _shopButton = Bind<UI_Button>(UI.ShopButton.ToString());

        _menuButton.AddEvent(() => { LOG.I("MenuButton Click!"); });
        _friendButton.AddEvent(() => { LOG.I("FriendButton Click!"); });
        _mailButton.AddEvent(() => { LOG.I("MailButton Click!"); });

        _profileCookieImage = Bind<Image>(UI.ProfileCookieImage.ToString());
        _profileNameText = Bind<TMP_Text>(UI.ProfileNameText.ToString());
        _lvText = Bind<TMP_Text>(UI.LvText.ToString());
        _expText = Bind<TMP_Text>(UI.ExpText.ToString());
        _castleLvText = Bind<TMP_Text>(UI.CastleLvText.ToString());

        _effRootGO = Bind<GameObject>(UI.EffRoot.ToString());
        if (!UTIL.TryGetComponent(out _effHideImage, _effRootGO))
        {
            LOG.E("Failed Get _effHideImage");
        }
    }

    public void SetShowHidePanel(bool isShow)
    {
        if (isShow)
            _effHideImage.enabled = true;
        else
            _effHideImage.enabled = false;
    }

    enum UI
    {
        EffRoot,
        PocketGroupRoot,

        MenuButton,
        MailButton,
        FriendButton,
        EditButton,
        ShopButton,

        // Profile
        ProfileNameText,
        ProfileCookieImage,
        LvText,
        ExpText,
        CastleLvText,
    }
    
    enum UIs
    {
        Point,
        StageButton,
        StageButtonImage,
        StageButtonText,
        StageStars
    }
    
}
