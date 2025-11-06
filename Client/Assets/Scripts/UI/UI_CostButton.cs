using Proto;
using Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;

public class UI_CostButton : UI_Base
{
    private UI_Button _button;
    private Image _iconImage;
    private TMP_Text _costText;

    private EObjType _costType;
    private long _costAmount;

    protected override void InitImp()
    {
        _button = Bind<UI_Button>(UI.Button.ToString());
        _iconImage = Bind<Image>(UI.IconImage.ToString());
        _costText = Bind<TMP_Text>(UI.CostText.ToString());

        _button.SetText("±¸¸Å"); // TODO: L10n
    }

    protected override void OnDestroyed()
    {
    }

    public void SetCost(EObjType costType, long costAmount)
    {
        _costType = costType;
        _costAmount = costAmount;

        var iconSprite = IconHelper.GetIcon(new ObjKey(costType, 0));
        _iconImage.sprite = iconSprite;

        Refresh();
    }

    public void Refresh()
    {
        var hasAmount = ContextHelper.GetObjAmount(_costType);
        _costText.text = $"{hasAmount} / {_costAmount}";
    }

    enum UI 
    {
        Button,
        IconImage,
        CostText
    }
}
