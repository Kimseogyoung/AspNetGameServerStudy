using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
    public static Vector2 ConvertVec2(this Vector3 vec3)
    {
        return new Vector2(vec3.x, vec3.z);
    }

    public static T AddGetComponent<T>(this GameObject gameObject) where T : Component
    {
        var component = gameObject.GetComponent<T>();
        if (component == null)
            component= gameObject.AddComponent<T>();
        return component;
    }

    public static Vector3 GetUIPosition(this Vector3 vec3)
    {
        return Camera.main.WorldToScreenPoint(vec3);
    }
}
