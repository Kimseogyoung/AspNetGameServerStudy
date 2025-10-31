using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class UI_Base : MonoBehaviour
{
	protected string _exitButton = "ExitButton";
    protected bool _isInit = false;
    private Dictionary<string, Object> _objects = new Dictionary<string, Object>();
    private Dictionary<string, Object[]> _objectLists = new Dictionary<string, Object[]>();


    public void Init()
    {
        if (_isInit)
        {
            LOG.I("Already Init");
            return;
        }
        
        _isInit = true;

        InitImp();
    }

    protected T BindComponent<T>(string name) where T : Component
    {
        Object obj = UTIL.FindChild(gameObject, name, true);

        if (obj == null)
        {
            //GameLogger.Error($"Failed to bind({name}) to {nameof(gameObject.name)}");
            return null;
        }

        var newGameObject = obj as GameObject;
        var newComp = newGameObject.AddComponent<T>();
        BindInternal(name, newComp);

        return Get<T>(name);
    }

    //Bind UI 오브젝트 이름으로 찾아 바인딩해주기
    protected T Bind<T>(string name) where T : UnityEngine.Object
	{
		Object obj = null;
        if (typeof(T) == typeof(GameObject))
            obj = UTIL.FindChild(gameObject, name, true);
        else
            obj = UTIL.FindChild<T>(gameObject, name, true);

		if (obj == null)
		{
            LOG.E($"Failed to bind({name}) to {nameof(gameObject.name)}");
			return null;
        }

        BindInternal(name, obj);
		return Get<T>(name);
    }

    protected List<T> BindManyComponent<T>(string name) where T : Component
    {
        Object[] objs= UTIL.FindChildAll(gameObject, name);

        if (objs == null)
        {
            LOG.E($"Failed to BindMany({name}) to {nameof(gameObject.name)}");
            return null;
        }

        Object[] componentArr = new T[objs.Length];
        for (int i=0; i < objs.Length; i++)
        {
            GameObject newGameObject = objs[i] as GameObject;
            componentArr[i] = newGameObject.AddComponent<T>();
        }

        BindListInternal(name, componentArr);
        return GetMany<T>(name);

    }


    protected List<T> BindMany<T>(string name) where T : UnityEngine.Object
	{
		Object[] objs;
        if (typeof(T) == typeof(GameObject))
            objs = UTIL.FindChildAll(gameObject, name);
        else
            objs = UTIL.FindChildAll<T>(gameObject, name);

        if (objs == null)
        {
            LOG.E($"Failed to BindMany({name}) to {nameof(gameObject.name)}");
            return null;
        }

        BindListInternal(name, objs);
        return GetMany<T>(name);

    }

    private void BindInternal(string name, Object obj)
    {
        _objects.Add(name, obj);
        if (obj is UI_Base uiObj)
        {
            uiObj.Init();
        }
    }

    private void BindListInternal(string name, Object[] objs)
    {
        _objectLists.Add(name, objs);
        foreach (Object obj in objs)
        {
            if (obj is UI_Base uiObj)
            {
                uiObj.Init();
            }
        }
    }

	//Get UI 오브젝트 가져오기
	protected T Get<T>(string name) where T : UnityEngine.Object
	{
		UnityEngine.Object obj = null;
        if (_objects.TryGetValue(name, out obj) == false)
        {
            LOG.E($"Failed to Get UI Object Name({name})");
            return null;
        }

		return obj as T;
	}

    protected List<T> GetMany<T>(string name) where T : UnityEngine.Object
    {
        Object[] objs;
        if (_objectLists.TryGetValue(name, out objs) == false)
            return null;

		List<T> list = new List<T>();
		for(int i=0; i<objs.Length; i++)
			list.Add(objs[i] as T);

        return list;
    }

    public void AddEvent(Button button, UnityAction unityAction)
	{
		button.onClick.AddListener(unityAction);
	}

    /*
	//BindEvent UI 오브젝트에 이벤트 등록하기
	public static void BindEvent(GameObject go, Action<PointerEventData> action, Define.UIEvent type = Define.UIEvent.Click)
	{
		UI_EventHandler evt = Util.AddGetComponent<UI_EventHandler>(go);

		switch (type)
		{
			case Define.UIEvent.Click:
				evt.OnClickHandler -= action; // 혹시나 이미 있을까봐 빼줌
				evt.OnClickHandler += action;
				break;
			case Define.UIEvent.Drag:
				evt.OnDragHandler -= action; // 혹시나 이미 있을까봐 빼줌
				evt.OnDragHandler += action;
				break;
		}
	}
	*/


    private void OnDestroy()
    {
        if (!_isInit)
        {
            return;
        }

        _isInit = false;
        //LOG.I($"Destroy {gameObject.name}");
        OnDestroyed();

        _objects.Clear();
        _objectLists.Clear();
    }


    protected abstract void OnDestroyed();
    protected abstract void InitImp();

}
