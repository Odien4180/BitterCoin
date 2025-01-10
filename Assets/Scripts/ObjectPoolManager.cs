using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : Singleton<ObjectPoolManager>
{
    private Dictionary<Type, ObjectPoolWrapper> _opDict = new Dictionary<Type, ObjectPoolWrapper>();

    public ObjectPool<T> CreatePool<T>(T origin, int count) where T : MonoBehaviour
    {
        if (_opDict.TryGetValue(typeof(T), out ObjectPoolWrapper opw) == false)
        {
            var pool = new ObjectPool<T>();
            pool.Initialize(origin, count);
            _opDict.Add(typeof(T), pool);
            return pool;
        }

        return _opDict[typeof(T)] as ObjectPool<T>;
    }

    public bool GetPool<T>(out ObjectPool<T> pool) where T : MonoBehaviour
    {
        if (_opDict.TryGetValue(typeof(T), out ObjectPoolWrapper opw) == false)
        {
            pool = null;
            return false;
        }

        pool = opw as ObjectPool<T>;
        return pool != null;
    }
}
