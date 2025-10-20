using System;
using System.Collections.Generic;
using UnityEngine;
using SG;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<GameObject> _queue = new Queue<GameObject>();
    private List<(GameObject gameObject, T component)> _activeList = new List<(GameObject gameObject, T component)>();
    private Transform _parentTransform;
    private GameObject _defaultGameObject;

    public ObjectPool(int maxCnt = 5, Transform parentTransform = null, GameObject defaultGameObject = null)
    {
        _defaultGameObject = defaultGameObject;

        if (parentTransform == null)
        {
            GameObject parentObject = new GameObject();
            parentObject.name = typeof(T).Name + " Pool";
            _parentTransform = parentObject.transform;
        }
        else
        {
            _parentTransform = parentTransform;
        }

        for (int i = 0; i < maxCnt; i++)
        {
            CreateObject();
        }
    }

    public void Destroy()
    {
        while(_queue.Count > 0)
        {
            var queueObj = _queue.Dequeue();
            GameObject.Destroy(queueObj);
        }

        _queue.Clear();
        _activeList.Clear();

        _parentTransform = null;
        _defaultGameObject = null;
    }

    public void Enqueue(GameObject gameObject)
    {
        T component = gameObject.GetComponent<T>();
        gameObject.SetActive(false);
        _queue.Enqueue(gameObject);
        _activeList.Remove((gameObject, component));
    }

    public (GameObject gameObject, T component) Dequeue()
    {
        if(_queue.Count <= 0 )
        {
            CreateObject();
        }

        GameObject gameObject = _queue.Dequeue();
        T component = gameObject.GetComponent<T>();
        gameObject.SetActive(true);

        _activeList.Add((gameObject, component));

        return (gameObject, component);
    }

    public int PoolCount()
    {
        return _queue.Count;
    }

    public int ActiveCount()
    {
        return _activeList.Count;
    }

    public List<(GameObject gameObject, T component)> GetActiveList()
    {
        return _activeList;
    }

    private void CreateObject()
    {
        GameObject newGameObject;
        if (_defaultGameObject != null)
        {
            newGameObject = GameObject.Instantiate(_defaultGameObject);
        }
        else
        {
            newGameObject = new GameObject();
        }

        T component = newGameObject.AddGetComponent<T>();

        newGameObject.SetActive(false);
        newGameObject.transform.SetParent(_parentTransform, true);

        _queue.Enqueue(newGameObject);
    }
}

