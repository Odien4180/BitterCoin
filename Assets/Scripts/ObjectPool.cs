using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectPoolWrapper : IDisposable
{
    public abstract void Dispose();
}

public class ObjectPool<T> : ObjectPoolWrapper where T : MonoBehaviour
{
    private T _origin;
    private Queue<T> _pool = new Queue<T>();
    private Transform _poolHolder;

    public void Initialize(T origin, int count)
    {
        _origin = origin;
        _poolHolder = new GameObject($"[ObjectPool] {origin.name}").transform;
        _poolHolder.position = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            var obj = GameObject.Instantiate(origin, _poolHolder);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public T Pop()
    {
        if (_pool.Count == 0)
        {
            var obj = GameObject.Instantiate(_origin, _poolHolder);
            return obj;
        }

        var target = _pool.Dequeue();
        target.gameObject.SetActive(true);
        return target;
    }

    public void Push(T obj)
    {
        obj.transform.SetParent(_poolHolder);
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }

    public override void Dispose()
    {
        foreach (var obj in _pool)
        {
            if (obj == null)
                continue;

            GameObject.Destroy(obj.gameObject);
        }

        if (_poolHolder != null)
            GameObject.Destroy(_poolHolder.gameObject);
    }
}