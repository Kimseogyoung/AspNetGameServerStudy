using Proto;
using Protocol;
using TMPro;
using UnityEngine.UI;

public class UI_PocketSlot : UI_Base
{
    private ObjKey _objKey;
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

    public void SetObjKey(ObjKey objKey)
    {
        _objKey = objKey;
        _iconImg.sprite = IconHelper.GetIconImage(objKey);
        RefreshValue();
    }

    // TODO: 값 바뀔때마다 이벤트로 호출되도록
    public void RefreshValue()
    {
        _valueText.text = ContextHelper.GetObjAmount(_objKey.Type).ToString();
    }

    enum UI
    {
        IconImage,
        Text,
    }
}
