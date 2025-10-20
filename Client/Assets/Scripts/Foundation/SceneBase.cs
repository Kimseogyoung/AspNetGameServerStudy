using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

abstract public class SceneBase 
{
    public string _sceneName { get; protected set; } = string.Empty;
    

    public bool EnterBase()
    {
        LOG.I($"{_sceneName} Enter!");
        if (!Enter())
            return false;

        return true;
    }

    public void StartBase()
    {
        LOG.I($"{_sceneName} Start!");
        Start();
    }

    public void UpdateBase()
    {
        Update();
    }

    public void ExitBase()
    {
        LOG.I($"{_sceneName} Exit!");
        Exit();
    }

    abstract protected bool Enter();

    abstract protected void Start();

    abstract protected void Update();

    abstract protected void Exit();

}
