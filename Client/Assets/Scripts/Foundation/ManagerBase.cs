using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public interface IManager
{
    public void Init();
    public void StartManager();
    public void Destroy();
}
public interface IManagerUpdatable
{
    public void UpdateManager();
    public void UpdatePausedManager();
    public void Pause(bool IsPause);
}


