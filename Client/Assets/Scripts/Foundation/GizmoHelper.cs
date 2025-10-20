using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SG;

public class DrawAction
{
    public Action Action;
    public float startTime = 0;
    public float playTime = 0;

    public DrawAction(Action action, float startTime, float showTime)
    {
        Action = action;
        this.startTime = startTime;
        this.playTime = showTime;
    }
}

public class GizmoHelper : MonoBehaviour
{
    private static Queue<DrawAction> _drawQueue = new Queue<DrawAction>();
    private static List<DrawAction> _playingActions = new List<DrawAction>();

    private static float _time = 0;
    private static MonoBehaviour monoInstance;

    [RuntimeInitializeOnLoadMethod]
    private static void Initializer()
    {
        monoInstance = new GameObject($"[{nameof(GizmoHelper)}]").AddComponent<GizmoHelper>();
        DontDestroyOnLoad(monoInstance.gameObject);
    }


    static public void PushDrawQueue(Action action, float time = 0f) 
    { 
        _drawQueue.Enqueue(new DrawAction(action, _time, time)); 
    }

    private void OnDrawGizmos()
    {
        _time += Time.deltaTime;

        for(int i=0; i<_drawQueue.Count; i++)
        {
            DrawAction action = _drawQueue.Dequeue();
            action.Action.Invoke();

            if (_time < action.startTime + action.playTime)
                _drawQueue.Enqueue(action);

        }
    }


}
