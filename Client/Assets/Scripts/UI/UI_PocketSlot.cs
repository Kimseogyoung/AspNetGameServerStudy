using TMPro;
using UnityEngine.UI;

public class UI_PocketSlot : UI_Base
{
    private Image _iconImg; // TODO: Icon관련 Proto만들고 Type별로.. 처리
    private TMP_Text _valueText;

    protected override void OnDestroyed()
    {

    }

    protected override void InitImp()
    {
        _iconImg = Bind<Image>(UI.IconImage.ToString());
        _valueText = Bind<TMP_Text>(UI.Text.ToString());
        _valueText.text = "0";
    }

    // TODO: 값 바뀔때마다 이벤트로 호출되도록
    public void SetValue(long value)
    {
        _valueText.text = value.ToString();
    }

    enum UI
    {
        IconImage,
        Text,
    }
}
