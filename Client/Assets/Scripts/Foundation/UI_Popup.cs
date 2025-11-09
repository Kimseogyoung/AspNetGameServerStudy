using UnityEngine.UI;

public abstract class UI_Popup : UI_Base
{
    protected string _exitButton = "ExitButton";
    private bool _isOpen = false;

    //팝업 UI 의 조상, 팝업 UI 캔버스들의 공통적인 부분들.
    protected override void InitImp()
    {
        APP.UI.SetCanvas(gameObject, true);
        Bind<UI_Button>(_exitButton)?.AddEvent(() => { APP.UI.ClosePopupUI(this); });
    }

    protected override void OnDestroyed()
    {
        Close();
        Get<UI_Button>(_exitButton)?.RemoveAllEvent();
    }

    public void Open()
    {
        if (_isOpen)
        {
            LOG.W($"Already Open Popup({gameObject.name})");
            return;
        }

        _isOpen = true;
        OnOpen();
    }

    public void Close()
    {
        if (!_isOpen)
        {
            LOG.W($"Already Closed Popup({gameObject.name})");
            return;
        }

        _isOpen = false;
        OnClose();
    }

    protected abstract void OnOpen();
    protected abstract void OnClose();
}
