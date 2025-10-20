using UnityEngine.UI;

public class UI_Popup : UI_Base
{
    //팝업 UI 의 조상, 팝업 UI 캔버스들의 공통적인 부분들.
    protected override void InitImp()
    {
        APP.UI.SetCanvas(gameObject, true);
        if(Bind<Button>(_exitButton) != null)
            Get<Button>(_exitButton).onClick.AddListener(() => { ClosePopupUI(); });
    }

    public virtual void ClosePopupUI()  // 팝업이니까 고정 캔버스(Scene)과 다르게 닫는게 필요
    {
        APP.UI.ClosePopupUI(this);
    }
}
