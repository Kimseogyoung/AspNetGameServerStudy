
using System;
using UnityEngine;
using System.Collections.Generic;

public class UIManager : IManager, IManagerUpdatable
{
    public void Init()
    {
    }

    public void StartManager()
    {
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

    int _order = 10; // 현재까지 최근에 사용한 오더

    Stack<UI_Popup> _popupStack = new Stack<UI_Popup>(); // 오브젝트 말고 컴포넌트를 담음. 팝업 캔버스 UI 들을 담는다.
    UI_Scene _sceneUI = null; // 현재의 고정 캔버스 UI
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = UTIL.AddGetComponent<Canvas>(go);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true; // 캔버스 안에 캔버스 중첩 경우 (부모 캔버스가 어떤 값을 가지던 나는 내 오더값을 가지려 할때)

        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else // soring 요청 X 라는 소리는 팝업이 아닌 일반 고정 UI
        {
            canvas.sortingOrder = 0;
        }
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        T sceneUI;
        GameObject go;
        if (!UTIL.TryFind<T>(out sceneUI, name))
        {
            LOG.I($"Create {name} scene");
            go = UTIL.Instantiate($"UI/Scene/{name}");
            sceneUI = UTIL.AddGetComponent<T>(go);

        }
        else
        {
            go = sceneUI.gameObject;
        }

        _sceneUI = sceneUI;

        go.transform.SetParent(Root.transform);
        _sceneUI.Init();

        return sceneUI;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name)) // 이름을 안받았다면 T로 ㄱㄱ
            name = typeof(T).Name;

        T popup = null;
        GameObject go = null;
        if (!UTIL.TryFind<T>(out popup, name))
        {
            go = UTIL.Instantiate($"UI/Popup/{name}");
            popup = UTIL.AddGetComponent<T>(go);
        }
        else
            go = popup.gameObject;

        _popupStack.Push(popup);

        go.transform.SetParent(Root.transform);
        popup.Init();

        return popup;
    }

    public void ClosePopupUI(UI_Popup popup) // 안전 차원
    {
        if (_popupStack.Count == 0) // 비어있는 스택이라면 삭제 불가
            return;

        if (_popupStack.Peek() != popup)
        {
            Debug.Log("Close Popup Failed!"); // 스택의 가장 위에있는 Peek() 것만 삭제할 수 잇기 때문에 popup이 Peek()가 아니면 삭제 못함
            return;
        }

        ClosePopupUI();
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        UTIL.Destroy(popup.gameObject);
        popup = null;
        _order--; // order 줄이기
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    public void Destroy()
    {

    }
}