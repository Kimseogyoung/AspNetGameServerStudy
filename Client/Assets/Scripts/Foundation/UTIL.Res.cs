
using UnityEngine;


    
static public partial class UTIL 
{
    static public bool TryCreateEmptyInstance<T>(out T outGO, GameObject parent, string name) where T : Component
    {
        GameObject obj = new GameObject();

        outGO =  obj.AddGetComponent<T>();
        
        if (outGO == null)
            return false;

        if(parent != null)
            outGO.transform.parent = parent.transform;

        outGO.name = name;

        return true;
    }

    static public bool TryCreateInstance<T>(out T outGO, GameObject resGo, GameObject parent, string name) where T : Component
    {
        outGO = null;
        GameObject obj = Instantiate(resGo, parent);
        if ( obj == null)
            return false;

        outGO =  obj.AddGetComponent<T>();

        outGO.name = name;
        obj.transform.position = parent.transform.position;
        return true;
    }

    static public bool TryCreateInstance<T>(out T outGO, string path, GameObject parent, string name) where T : Component
    {
        outGO = GameObject.Instantiate(UTIL.LoadRes<T>(path));

        if (outGO == null)
            return false;

        if (parent != null)
            outGO.transform.SetParent(parent.transform);

        outGO.name = name;

        return outGO;
    }

    static public T Instantiate<T>(string path, Vector3 pos, GameObject parent, string name) where T : Component
    {
        GameObject obj = Instantiate(path, pos, parent);
        if (obj != null)
            obj.name = name;
        return obj.AddGetComponent<T>();
    }

    static public GameObject Instantiate(GameObject resGO, GameObject parent = null)
    {
        GameObject obj = GameObject.Instantiate(resGO);
        if (parent != null)
            obj.transform.SetParent(parent.transform);
        return obj;
    }

    static public GameObject Instantiate(string path, GameObject parent = null)
    {
        GameObject obj = GameObject.Instantiate(UTIL.LoadRes<GameObject>(path));
        if (parent != null)
            obj.transform.SetParent(parent.transform);
        return obj;
    }

    static public GameObject Instantiate(string path, Vector3 pos, GameObject parent = null)
    {
        GameObject obj = GameObject.Instantiate(UTIL.LoadRes<GameObject>(path));
        if (parent != null)
            obj.transform.SetParent(parent.transform);

        obj.transform.position = pos;

        return obj;
    }

    static public T LoadRes<T>(string path) where T : Object
    {
        T obj = Resources.Load<T>(path);
        LOG.D("{0} 타입 {1}경로에서 Load {2}", typeof(T), path, obj);
        return obj;
    }

    static public bool TryLoadRes<T>(out T outRes, string path) where T : Object
    {
        LOG.D("{0} 타입 {1}경로에서 Load ", typeof(T), path);

        outRes = LoadRes<T>(path);
        if (outRes == null)
            return false;

        return true;
    }

    static public Sprite LoadSprite(string imageName, string spriteName)
    {
        Sprite[] all = Resources.LoadAll<Sprite>(imageName);
        foreach (var s in all)
        {
            if (s.name == spriteName) return s;

        }
        return null;
    }
}


