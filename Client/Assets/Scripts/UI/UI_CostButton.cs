using UnityEngine;

public class UI_CostButton : UI_Base
{
    private UI_Button _button;
    protected override void InitImp()
    {
        _button = Bind<UI_Button>(UI.Button.ToString());
        _button.SetText("±¸¸Å"); // TODO: L10n
    }

    protected override void OnDestroyed()
    {
    }

    enum UI 
    {
        Button
    }
}
