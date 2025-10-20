using UnityEngine;

public class IntroScene : SceneBase
{
    private double _introTime = 1;
    private double _currentTime = 0;
    private bool _isLoadingNextScene = false;
    public IntroScene(string sceneName)
    {
        _sceneName = sceneName;
    }

    protected override bool Enter()
    {
        return true;
    }

    protected override void Exit()
    {

    }

    protected override void Start()
    {

    }

    protected override void Update()
    {
        _currentTime += Time.fixedDeltaTime;
        if(_currentTime > _introTime && _isLoadingNextScene == false)
        {
            _isLoadingNextScene = true;
            _ = APP.SceneManager.ChangeScene("InGameScene");
        }
    }

}
