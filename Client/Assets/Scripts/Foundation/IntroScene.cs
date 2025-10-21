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

        // 로딩이 빠르게 완료되었더라도 최소 _currentTimeSec만큼은 대기한 후 넘어감.
        if (_currentTimeSec > APP.GameConf.IntroLoadingMinSec && _stage != ELoadStage.LOAD_NEXT_SCENE)
        {
            _stage = ELoadStage.LOAD_NEXT_SCENE;
             _ = APP.SceneManager.ChangeScene("InGameScene");
        }
    }

    private async Task<bool> StartLoadAsync()
    {
        foreach (ELoadStage stage in Enum.GetValues(typeof(ELoadStage)))
        {
            // NOTE: LOAD_NEXT_SCENE는 Update함수에서 Scene이동 하면서 처리함.
            if (stage == ELoadStage.LOAD_NEXT_SCENE)
            {
                continue;
            }

            _stage = stage;
            _ui.SetLoadingText(_stage.ToString());

            var isSuccess = await HandleStageAsync(_stage);
            if (!isSuccess)
            {
                break;
            }

            // 가짜 로딩하는 척
            await Task.Delay(500);
        }

        if (_stage < ELoadStage.FINISH)
        {
            LOG.E($"Failed Intro Loading Stage({_stage})");
            _ui.SetLoadingText(_stage.ToString() + "_FAILED");
            return false;
        }

        LOG.I("Success Intro Loading");
        return true;
    }

    private async Task<bool> HandleStageAsync(ELoadStage stage)
    {
        switch (stage)
        {
            case ELoadStage.NONE:
            case ELoadStage.LOAD_RESOURCE:
                return true;
            case ELoadStage.SIGN_IN:
                var signUpRes = await APP.Ctx.RequestSignUpAsync(Guid.NewGuid().ToString());
                return !APP.Ctx.IsErrorRes(signUpRes);
            case ELoadStage.LOAD_PLAYER:
                var enterRes = await APP.Ctx.RequestEnterAsync();
                return !APP.Ctx.IsErrorRes(enterRes);
            case ELoadStage.FINISH:
                return true;
            default:
                LOG.E($"No Handling LoadStage({stage})");
                return false;
        }
    }
}
