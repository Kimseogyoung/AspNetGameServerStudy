using Proto;
using Protocol;
using System;
using TMPro;
using UnityEngine.UI;

public class UI_CostButton : UI_Base
{
    public  UI_Button Button { get; private set; }

    private Image _iconImage;
    private TMP_Text _costText;

    private EObjType _costType;
    private long _costAmount;

    protected override void InitImp()
    {
        Button = Bind<UI_Button>(UI.Button.ToString());

        _iconImage = Bind<Image>(UI.IconImage.ToString());
        _costText = Bind<TMP_Text>(UI.CostText.ToString());

        Button.SetText("±¸¸Å"); // TODO: L10n
    }

    protected override void OnDestroyed()
    {
        Button.RemoveAllEvent();
    }

    public void SetCost(EObjType costType, long costAmount)
    {
        _costType = costType;
        _costAmount = costAmount;

        if (!IsActivate())
        {
            gameObject.SetActive(false);
            Button.RemoveAllEvent();
        }
        else
        {
            gameObject.SetActive(true);
            var iconSprite = IconHelper.GetIconImage(new ObjKey(costType, 0));
            _iconImage.sprite = iconSprite;

            Refresh();
        }
    }

    public void Refresh()
    {
        if (!IsActivate())
        {
            return;
        }

        var hasAmount = ContextHelper.GetObjAmount(_costType);
        _costText.text = $"{hasAmount} / {_costAmount}";
        Button.SetEnable(hasAmount >= _costAmount);
    }

    private bool IsActivate()
    {
        return _costType != EObjType.NONE;
    }

    enum UI 
    {
        Button,
        IconImage,
        CostText
    }
}
