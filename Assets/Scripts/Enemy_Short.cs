using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class Enemy_Short : MonoBehaviour, Hitable, IDisposable
{
    [SerializeField]
    private int _hp = 1000;

    [SerializeField]
    private float _speed = 4.0f;

    private Queue<Transform> _waypoints;

    private IDisposable _updateDisposable;

    private ObjectPool<Enemy_Short> _enemyPool;

    private void Start()
    {
        ObjectPoolManager.Instance.GetPool<Enemy_Short>(out _enemyPool);
    }

    public void Hit(HitArgs args)
    {
        _hp -= args.Damage;

        if (_hp <= 0)
        {
            PoolPush();
        }
    }

    public Enemy_Short Initialize(List<Transform> waypoints)
    {
        _waypoints = new Queue<Transform>(waypoints);
        transform.position = _waypoints.Peek().position;
        gameObject.SetActive(true);

        _updateDisposable = Observable.EveryUpdate()
            .Subscribe(_ => Move())
            .AddTo(this);

        GameManager.Instance.ActiveEnemies.Add(this);

        return this;
    }

    private void Move()
    {
        if (_waypoints.Count == 0)
        {
            PoolPush();
            return;
        }

        var target = _waypoints.Peek();
        var dir = target.position - transform.position;
        var distance = _speed * Time.deltaTime;

        if (dir.magnitude <= distance)
        {
            _waypoints.Dequeue();
            transform.position = target.position;
        }
        else
        {
            transform.position += dir.normalized * distance;
        }
    }

    private void PoolPush()
    {
        if (GameManager.Instance.ActiveEnemies.Contains(this))
        {
            GameManager.Instance.ActiveEnemies.Remove(this);
        }

        if (_enemyPool == null)
        {
            Dispose();
            GameObject.Destroy(gameObject);
        }
        else
        {
            _enemyPool.Push(this);
            Dispose();
        }
    }

    public void Dispose()
    {
        _waypoints?.Clear();
        _waypoints = null;

        _enemyPool = null;

        _updateDisposable?.Dispose();
        _updateDisposable = null;
    }
}
