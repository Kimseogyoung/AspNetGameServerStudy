using System;
using System.Threading.Tasks;
using UnityEngine;

public class IntroScene : SceneBase
{
    private enum ELoadStage
    {
        NONE,
        LOAD_RESOURCE,
        SIGN_IN,
        LOAD_PLAYER,
        FINISH,
        LOAD_NEXT_SCENE
    }

    private UI_IntroScene _ui;
    private double _currentTimeSec = 0;
    private ELoadStage _stage = ELoadStage.NONE;

    public IntroScene(string sceneName)
    {
        _sceneName = sceneName;
    }

    protected override bool Enter()
    {
        _ui = APP.UI.ShowSceneUI<UI_IntroScene>("UI_IntroScene");
        return true;
    }

    protected override void Exit()
    {

    }

    protected override void Start()
    {
        _ = StartLoadAsync();
    }

    protected override void Update()
    {
        _currentTimeSec += Time.fixedDeltaTime;
        if (_stage < ELoadStage.FINISH)
        {
            return;
        }

        if(_currentTimeSec > APP.GameConf.IntroLoadingMinSec && _stage != ELoadStage.LOAD_NEXT_SCENE)
        {
            _stage = ELoadStage.LOAD_NEXT_SCENE;
             _ = APP.SceneManager.ChangeScene("InGameScene");
        }
    }

    private async Task<bool> StartLoadAsync()
    {
        foreach (ELoadStage stage in Enum.GetValues(typeof(ELoadStage)))
        {
            if (stage == ELoadStage.LOAD_NEXT_SCENE)
            {
                continue;
            }

            _stage = stage;
            _ui.SetLoadingText(_stage.ToString());

            switch (_stage)
            {
                case ELoadStage.NONE:
                    break;
                case ELoadStage.LOAD_RESOURCE:
                    break;
                case ELoadStage.SIGN_IN:
                    // TODO: 계정 있으면 SignIn
                    await APP.Ctx.RequestSignUpAsync(Guid.NewGuid().ToString());
                    if (string.IsNullOrEmpty(APP.Ctx.SessionId))
                    {
                        continue;
                    }

                    LOG.I($"Success SignUpRequest SessionId({APP.Ctx.SessionId})");
                    break;
                case ELoadStage.LOAD_PLAYER:
                    await APP.Ctx.RequestEnterAsync();
                    break;
                case ELoadStage.FINISH:
                    break;
                default:
                    LOG.E($"No Handling LoadStage({_stage})");
                    continue;

            }

            // 가짜 로딩하는 척
            await Task.Delay(500);
        }

        if (_stage < ELoadStage.FINISH)
        {
            LOG.E($"Failed Intro Loading Stage({_stage})");
            return false;
        }

        LOG.I("Success Intro Loading");
        return true;
    }
}
