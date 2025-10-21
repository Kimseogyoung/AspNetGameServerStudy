using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GAME : ScriptBase
{
    public enum EGameState
    {
        PREPARE,
        PLAY,
        STOP
    }

    public InGameScene InGame
    {
        get
        {
            InGameScene scene = _sceneManager.GetCurrentScene<InGameScene>();
            if (scene == null)
            {
                LOG.E("InGameScene Is Null");

            }
            return scene;
        }
    }

    private List<IManagerUpdatable> _managerUpdatables = new List<IManagerUpdatable>();
    private List<IManager> _managers = new List<IManager>();
    private SceneManager _sceneManager;
    private ConfigManager _configManager;
    private InputManager _inputManager;
    private UIManager _uiManager;

    private EGameState _state = EGameState.PREPARE;
    private bool _mainInstance = true;

    public T AddUpdatablePublicManager<T>(T manager) where T : IManagerUpdatable
    {
        _managerUpdatables.Add(manager);
        return manager;
    }

    public void RemoveUpdatablePublicManager<T>(T manager) where T : IManagerUpdatable
    {
        if (!_managerUpdatables.Contains(manager)) return;
        _managerUpdatables.Remove(manager);
    }

    protected override bool OnCreateScript()
    {
        if (FindObjectsOfType(typeof(GAME)).Length >= 2)
        {
            _mainInstance = false;
            Destroy(gameObject);
            return false;
        }

        if (!LocalPlayerPrefs.Create(out LocalPlayerPrefs localPlayerPrefs))
        {
            LOG.E("CanNot Create LocalPlayerPref");
            return false;
        }

        APP.LocalPlayerPrefs = localPlayerPrefs;

        APP.GAME = this;

        LOG.I("GameManager Awake");

        DontDestroyOnLoad(gameObject);

        _configManager = new ConfigManager();
        AddManager(_configManager);

        _sceneManager = new SceneManager();
        AddManager(_sceneManager, true);

        _inputManager = new InputManager();
        AddManager(_inputManager, true);

        _uiManager = new UIManager();
        APP.UI = _uiManager;

        APP.SceneManager = _sceneManager;
        APP.InputManager = _inputManager;

        foreach (IManager manager in _managers)
        {
            manager.Init();
        }

        APP.InputManager.AddInputAction(EInputAction.PAUSE, () => { Pause(new PauseEvent(true)); });
        APP.InputManager.AddInputAction(EInputAction.PLAY, () => { Pause(new PauseEvent(false)); });

        EventQueue.AddEventListener<PauseEvent>(EEventActionType.PAUSE, Pause);
        EventQueue.AddEventListener<PauseEvent>(EEventActionType.PLAY, Pause);
        return true;
    }

    protected override void OnDestroyScript()
    {
        if (!_mainInstance)
            return;

        foreach (IManager manager in _managers)
        {
            manager.Destroy();
        }
    }

    void Start()
    {
        LOG.I("Start UIManager");
        APP.UI.StartManager();

        foreach (IManager manager in _managers)
        {
            LOG.I($"Start {manager.GetType().Name}");
            manager.StartManager();
        }

        _ = InitAsync();
    }

    void FixedUpdate()
    {
        if (_state == EGameState.PREPARE)
            return;

        if (_state == EGameState.STOP)
        {
            for (int i = 0; i < _managerUpdatables.Count; i++)
                _managerUpdatables[i].UpdatePausedManager();
            return;
        }

        for (int i = 0; i < _managerUpdatables.Count; i++)
            _managerUpdatables[i].UpdateManager();

    }

    private async Task InitAsync()
    {
        LOG.I($"Start Init");

        var csvPath = AppPath.GetCsvPath();
        ClientCore.APP.Init(csvPath, APP.GameConf.ServerUrl, TimeSpan.FromSeconds(APP.GameConf.RequestTimeoutSec));

        APP.Ctx = ClientCore.APP.Ctx;
        APP.Prt = ClientCore.APP.Prt;

        var isConnectServer = await APP.Ctx.IsSuccessConnect();
        if (!isConnectServer)
        {
            LOG.E($"Failed Connect Server. ServerUrl({APP.Ctx.Host})");
            return;
        }

        bool isLoadSuccess = await _sceneManager.StartFirstScene();

        if (!isLoadSuccess)
        {
            LOG.E("Failed Init. GAME.Init()");
            return;
        }
        LOG.I($"Success Game Init. ServerUrl({APP.Ctx.Host})");
        _state = EGameState.PLAY;
    }

    private void Pause(PauseEvent pause)
    {

        if ((pause.IsPause && _state == EGameState.STOP) || (!pause.IsPause && _state == EGameState.PLAY))
            return;

        TimeHelper.Stop(pause.IsPause);
        _state = pause.IsPause ? EGameState.STOP : EGameState.PLAY;

        for (int i = 0; i < _managerUpdatables.Count; i++)
            _managerUpdatables[i].Pause(pause.IsPause);
    }

    private void AddManager<T>(T manager, bool isUpdatable = false) where T : IManager
    {
        _managers.Add(manager);
        if (isUpdatable)
            _managerUpdatables.Add((IManagerUpdatable)manager);
    }
}
