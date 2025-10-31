using UnityEngine.UI;

public abstract class UI_Popup : UI_Base
{
    //�˾� UI �� ����, �˾� UI ĵ�������� �������� �κе�.
    protected override void InitImp()
    {
        APP.UI.SetCanvas(gameObject, true);
        if(Bind<Button>(_exitButton) != null)
            Get<Button>(_exitButton).onClick.AddListener(() => { APP.UI.ClosePopupUI(this); });
    }

    protected override void OnDestroyed()
    {
        Get<Button>(_exitButton)?.onClick.RemoveAllListeners();
    }

    public abstract void OnOpen();
    public abstract void OnClose();
}
