
public class InGameScene : SceneBase
{

    public Rule_InGame Rule { get { if (_rule == null) { LOG.E("Rule Is Null"); } return _rule; } }
    public UI_InGameScene UI { get { if (_ui == null) { LOG.E("UI Is Null"); } return _ui; } }

    private Rule_InGame _rule = null;
    private UI_InGameScene _ui = null;

    public InGameScene(string sceneName)
    {
        _sceneName = sceneName;
    }

    protected override bool Enter()
    {
        _ui = APP.UI.ShowSceneUI<UI_InGameScene>("UI_InGameScene");
        if (!Rule_InGame.Create(out _rule))
            return false;

        return true;
    }

    protected override void Exit()
    {
        Rule_InGame.Destroy(ref _rule);
    }

    protected override void Start()
    {
        _rule.StartFirst();
    }

    protected override void Update()
    {
        _rule.Update();
    }
}
