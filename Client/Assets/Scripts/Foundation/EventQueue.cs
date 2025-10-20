using System;
using System.Collections.Generic;
using UnityEngine;

public class EventQueue : MonoBehaviour
{
    private static Dictionary<EEventActionType, Action<EventBase>> _actions = new Dictionary<EEventActionType, Action<EventBase>>();
    private static Queue<EventBase> _waitedEventQueue= new Queue<EventBase>();
    private static Queue<EventBase> _immediateEventQueue = new Queue<EventBase>();

    private float _playingTime = 0;
    private bool _isPlayingWaitedEvent = false;
    private EventBase _playingEvent = null;

    static public void PushEvent<T>(EEventActionType eventActionType, T evt, double time) where T : EventBase
    {
        evt.eventActionType = eventActionType;
        evt.eventPlayTime = time;
        _waitedEventQueue.Enqueue(evt);
    }

    static public void PushEvent<T>(EEventActionType eventActionType, T evt) where T : EventBase
    {
        evt.eventActionType = eventActionType;
        _immediateEventQueue.Enqueue(evt);
    }

    static public Action<EventBase> AddEventListener<T>(EEventActionType eventActionType, Action<T> action) where T : EventBase
    {
        Action<EventBase> act = (e) => { action.Invoke((T)e); };
        if (!_actions.ContainsKey(eventActionType))
            _actions.Add(eventActionType, act);
        else
            _actions[eventActionType] += act;

        return act;
    }
    static public void RemoveEventListener(EEventActionType eventActionType, Action<EventBase> action)
    {
        if (!_actions.ContainsKey(eventActionType))
        {
            return;
        }
        _actions[eventActionType] -= action;
    }

    static public void RemoveAllEventListener(EEventActionType eventActionType)
    {
        if (!_actions.ContainsKey(eventActionType))
        {
            return;
        }
        _actions[eventActionType] = null;
    }

    static public void RemoveAllEvent()
    {
        LOG.I("Remove All Event");
        _immediateEventQueue.Clear();
    }

    private void Update()
    {
        while (_immediateEventQueue.Count > 0)
        {
            EventBase dequeueAction =_immediateEventQueue.Dequeue();
            if (!_actions.TryGetValue(dequeueAction.eventActionType, out Action<EventBase> action) || dequeueAction == null)
            {
                return;
            }
            
            if(action == null)
            {
                LOG.E("ImmeEventQueue Dequeue : {0} 이벤트 호출 실패", dequeueAction.eventActionType);
            }
            else
            {
                LOG.I("ImmeEventQueue Dequeue : {0} 이벤트 호출", dequeueAction.eventActionType);
                action.Invoke(dequeueAction);
            }
        }

        if (!_isPlayingWaitedEvent)
        {
            if (_waitedEventQueue.Count > 0)
            {
                _playingEvent = _waitedEventQueue.Dequeue();
                if(!_actions.TryGetValue(_playingEvent.eventActionType, out Action<EventBase> action) || action == null)
                {
                    return; 
                }
                //GameLogger.Info("WaitedEventQueue Dequeue : {0} 이벤트 호출", _playingEvent.eventActionType);
                action.Invoke(_playingEvent);
              
            }
            return;
        }
        if (_playingTime >= _playingEvent.eventPlayTime)
        {
            _playingTime = 0;
            _isPlayingWaitedEvent = false;
            return;
        }
        else
            _isPlayingWaitedEvent = true;

        _playingTime += Time.deltaTime;
    }

    private static MonoBehaviour monoInstance;
    [RuntimeInitializeOnLoadMethod]
    private static void Initializer()
    {
        monoInstance = new GameObject($"[{nameof(EventQueue)}]").AddComponent<EventQueue>();
        DontDestroyOnLoad(monoInstance.gameObject);
    }
}

