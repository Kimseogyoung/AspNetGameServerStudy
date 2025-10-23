using TMPro;
using UnityEngine.UI;

public class UI_PocketSlot : UI_Base
{
    private Image _iconImg; // TODO: Icon���� Proto����� Type����.. ó��
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

    // TODO: �� �ٲ𶧸��� �̺�Ʈ�� ȣ��ǵ���
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
