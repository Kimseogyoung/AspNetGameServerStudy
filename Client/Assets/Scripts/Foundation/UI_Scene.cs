public abstract class UI_Scene : UI_Base
{
    //�� UI �� ����, �˾�ó�� ���ʴ�� �ߴ°� �ƴ϶� ���� �ڸ���� �ִ� �������� UI ĵ�������� �������� �κе�.

    protected override void InitImp()
	{
		APP.UI.SetCanvas(gameObject, false);
	}
}
