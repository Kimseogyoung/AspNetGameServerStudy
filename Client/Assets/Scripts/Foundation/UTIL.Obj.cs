using System.Collections.Generic;
using System.Linq;
using UnityEngine;

static public partial class UTIL 
{
    static public void Destroy(GameObject gameObject)
    {
        GameObject.Destroy(gameObject.gameObject);
    }

    static public bool TryGetComponent<T>(out T comp, GameObject gameObject)
    {
        comp = gameObject.GetComponent<T>();
        return comp != null;
    }

    static public bool TryGetComponent<T>(out T comp, GameObject gameObject, string name)
    {
        comp = default;

        // 게임 오브젝트에서 특정 이름의 자식 오브젝트를 찾기
        Transform childTransform = gameObject.transform.Find(name);

        if (childTransform == null)
            return false;

       comp = childTransform.GetComponent<T>();

        return comp != null;
    }



    static public bool TryAddGetComponet<T>(out T comp, GameObject gameObject)
    {
        comp = AddGetComponent<T>(gameObject);

        return comp != null;
    }

    static public T GetComponent<T>(GameObject gameObject)
    {
        return gameObject.GetComponent<T>();
    }

    static public T AddGetComponent<T>(GameObject gameObject)
    {
        if(!TryGetComponent<T>(out T result, gameObject))
        {
            gameObject.AddComponent(typeof(T));
            result = gameObject.GetComponent<T>();
        }

        return result;
    }

    static public T GetComponentInChildren<T>(GameObject gameObject, string name = "")
    {

        if (!string.IsNullOrEmpty(name))
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform obj = gameObject.transform.GetChild(i);
                if (obj.name == name)
                {
                    return obj.GetComponent<T>();
                }
            }
        }

        return gameObject.GetComponentInChildren<T>();
    }


    static public T FindChild<T>(GameObject gameObject, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        const string cloneString = "(Clone)";
        var list = new List<T>();
        if (recursive)
        {
            foreach (T value in gameObject.GetComponentsInChildren<T>())
            {
                list.Add(value);
            }
        }
        else
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                T value = GetComponent<T>(gameObject.transform.GetChild(i).gameObject);

                if (value == null)
                {
                    continue;
                }
                list.Add(value);
            }
        }

        if(name == null)
        {
            return list.FirstOrDefault();
        }

        for (int i = 0; i < list.Count; i++)
        {
               
            if (list[i].name == name || list[i].name == name + cloneString)
            {
                return list[i];
            }
        }

        return null;
    }

    static public GameObject FindChild(GameObject gameObject, string name = null, bool recursive = false)
    {
        UnityEngine.Transform transform = FindChild<Transform>(gameObject, name, recursive);
        if(transform == null)
            return null;
            
        return transform.gameObject;
    }

    static public T[] FindChildAll<T>(GameObject gameObject, string name = null) where T : UnityEngine.Object
    {
        List<T> objects= new List<T>();
        foreach (T value in gameObject.GetComponentsInChildren<T>())
        {
            if (value.name == name || value.name.StartsWith(name) && value.name.EndsWith(')'))
                objects.Add(value);
        }
        return objects.ToArray();
    }

    static public GameObject[] FindChildAll(GameObject gameObject, string name = null)
    { 
        Transform[] transforms = FindChildAll<Transform>(gameObject, name);
        if (transforms == null || transforms.Length == 0)
            return null;

        GameObject[] objects = new GameObject[transforms.Length];
        for(int i=0; i < transforms.Length; i++)
            objects[i] = transforms[i].gameObject;

        return objects;
    }



    static public bool TryFind<T>(out T result, string name) where T : UnityEngine.Object
    {
        result = Find<T>(name);

        if (result == null)
            return false;
        return true;
    }
    static public T Find<T>(string name) where T : UnityEngine.Object
    {
        return GameObject.FindObjectsOfType<T>().Where(e => e.name == name).FirstOrDefault();
    }

}


