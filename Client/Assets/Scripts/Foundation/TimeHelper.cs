using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class TimeHelper : MonoBehaviour
{ 
    private static bool _isStopped = false;
    private static float _currentTime = 0;
    private static List<TimeAction> _timeActionList = new List<TimeAction>();

    public static TimeAction AddTimeEvent(string name, float time, Action action)
    {
        if(time <= 0)
        {
            action.Invoke();
            return null;
        }
        TimeAction timeAction = new TimeAction(_currentTime + time, action, name);
        _timeActionList.Add(timeAction);
        return timeAction;
    }

    public static void Stop(bool isStopped)
    {
        _isStopped = isStopped;
    }

    public static void RemoveTimeEvent(TimeAction action)
    {
        if (action == null)
            return;

        action.RemoveAction();
        _timeActionList.Remove(action);
    }

    public static float GetTimeEventRemainingTime(TimeAction action)
    {
        var foundTimeAction = _timeActionList.Find(x => x.Name == action.Name);

        if(foundTimeAction == null)
        {
            LOG.E($"Can not Found Time Action. Return 0. Name({action.Name})");
            return 0;
        }
        return foundTimeAction.Time - _currentTime;
    }

    private void FixedUpdate()
    {
        if (_isStopped)
            return;

        _currentTime += Time.fixedDeltaTime;

        for (int i = 0; i < _timeActionList.Count; i++)
        {
            if (_timeActionList[i].Time < _currentTime) 
            {
                _timeActionList[i].InvokeAction();
                _timeActionList[i] = null;
                _timeActionList.RemoveAt(i);
                i--;
            }
        }
    }

    public class TimeAction
    {
        public float Time { get; private set; }
        public Action Action { get; private set; }
        public string Name { get; private set; }
        public bool isPlaying { get { return Action != null; } }
        public TimeAction(float time, Action action, string name = "")
        {
            Time = time;
            Action = action;
            Name = name;
        }

        public void InvokeAction()
        {
            Action.Invoke();
            Action = null;
        }

        public void RemoveAction()
        {
            Action = null;
        }
    }


    private static MonoBehaviour monoInstance;
    [RuntimeInitializeOnLoadMethod]
    private static void Initializer()
    {
        monoInstance = new GameObject($"[{nameof(TimeHelper)}]").AddComponent<TimeHelper>();
        DontDestroyOnLoad(monoInstance.gameObject);
    }

}

