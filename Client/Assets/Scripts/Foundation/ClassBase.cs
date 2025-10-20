using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class ClassBase
{
    public static void Destroy<T>(ref T classObj) where T : ClassBase
    {
        classObj.OnDestroy();
        classObj = null;
    }

    public static bool Create<T>(out T classBase) where T : ClassBase, new()
    {
        classBase = new T();

        if (!classBase.OnCreate())
        {
            Destroy(ref classBase);
            return false;
        }

        return true;
    }

    protected abstract bool OnCreate();
    protected abstract void OnDestroy();

}
