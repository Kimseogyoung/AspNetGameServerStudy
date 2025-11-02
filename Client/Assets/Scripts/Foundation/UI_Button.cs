using System;
using TMPro;
using UnityEngine.UI;

public class UI_Button : UI_Base
{
    private Button _button;
    private TMP_Text _text;

    protected override void OnDestroyed()
    {
        RemoveAllEvent();
    }

    protected override void InitImp()
    {
        _button = Bind<Button>(UI.Button.ToString());
        _text = Bind<TMP_Text>(UI.ButtonText.ToString());

        _text.text = gameObject.name;
    }

    public void AddEvent(Action action)
    {
        _button.onClick.AddListener(() => { action(); });
    }

    public void RemoveAllEvent()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void SetText(string text)
    {
        _text.text = text;
    }

    enum UI
    {
        Button,
        ButtonText,
    }
}
