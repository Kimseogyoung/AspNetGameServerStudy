using SG;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

public class SceneManager : IManager, IManagerUpdatable
{
    private string _firstScene;
    private bool _loadSuccess = false;
    private TaskCompletionSource<bool> _taskCompletion = null;
    private SceneBase _currentScene;

    public void FinishManager()
    {


    }

    public void Init()
    {
    
    }

    public void StartManager()
    {
        _firstScene = APP.GameConf.StartScene;

#if DEBUG
        _firstScene = APP.DebugConf.StartScene;
#endif
    }

    public async Task<bool> StartFirstScene()
    {
        return await ChangeScene(_firstScene);
    }

    public async Task<bool> ChangeScene(string nextSceneName)
    {
        LOG.I($"ChangeScene {nextSceneName}");

        _loadSuccess = false;
        if (_currentScene != null)
        {
            _currentScene.ExitBase();
            _currentScene = null;
        }

        _taskCompletion = new();
        CoroutineHelper.StartCoroutine(CoLoadScene(nextSceneName));
        await _taskCompletion.Task;
        _taskCompletion = null;

        if (!_loadSuccess)
            return false;
        return true;
    }

    public void FixedUpdateManager()
    {
        if (_taskCompletion != null)
            return;

        if (_currentScene == null) 
            return;
        _currentScene.UpdateBase();
    }

    public void UpdateManager()
    {

    }

    public void UpdatePausedManager()
    {
        
    }

    public void Pause(bool IsPause)
    {
       
    }

    public T GetCurrentScene<T>() where T : SceneBase
    {
        if(_currentScene == null) return null;

        if(!(_currentScene is T))
        {
            LOG.E("{0} is Not {1}", _currentScene._sceneName, typeof(T));
            return null;
        }
        return (T)_currentScene;
    }

    private IEnumerator CoLoadScene(string nextSceneName)
    {
        AsyncOperation async;
        float sec = 0;
        const int maxLoadSec = 10;
        if (UnitySceneManager.GetActiveScene().name != nextSceneName)
        {
            async = UnitySceneManager.LoadSceneAsync(nextSceneName);

            while (!async.isDone)
            {
                yield return new WaitForSeconds(0.1f);
                sec += 0.1f;
                if(sec > maxLoadSec)
                {
                    _taskCompletion.SetResult(true);
                    yield break;
                }
            }
        }

        if (InvokeNextScene(nextSceneName))
            _loadSuccess = true;
        
        _taskCompletion.SetResult(true);
    }

    private bool InvokeNextScene(string nextSceneName)
    {
        LOG.I($"Load Scene Done {nextSceneName}");
        switch (nextSceneName)
        {
            case "IntroScene":
                _currentScene = new IntroScene(nextSceneName);
                break;
            case "InGameScene":
                _currentScene = new InGameScene(nextSceneName);
                break;
            default:
                LOG.E($"NotFound Scene ({nextSceneName})");
                break;
        }
        if (!_currentScene.EnterBase())
        {
            LOG.E($"Faile Enter {nextSceneName}");
            return false;
        }

        _currentScene.StartBase();

        return true;
    }

    public void Destroy()
    {
        _taskCompletion = null;

        if (_currentScene != null)
            _currentScene.ExitBase();

        _currentScene = null;
    }
}

