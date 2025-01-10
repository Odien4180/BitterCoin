using System;
using UniRx;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private float _speed = 10.0f;

    [SerializeField]
    private float _damage = 10.0f;

    [SerializeField]
    private Hitable _hitable;

    [SerializeField]
    private HitArgs _hitArgs;

    private ObjectPool<Bullet> _bulletPool;

    private IDisposable _updateDisposable;

    private void Start()
    {
        ObjectPoolManager.Instance.GetPool<Bullet>(out _bulletPool);
    }

    public void Initialize(Hitable hitable, HitArgs hitArgs)
    {
        _hitable = hitable;
        _hitArgs = hitArgs;

        transform.position = _hitArgs.FirePosition;
        gameObject.SetActive(true);

        _updateDisposable?.Dispose();
        _updateDisposable = Observable.EveryUpdate()
            .Subscribe(_ => Move())
            .AddTo(this);
    }

    private void Move()
    {
        if (_hitArgs.Target == null || _hitArgs.Target.gameObject.activeSelf == false)
        {
            PoolPush();
            return;
        }

        var dir = _hitArgs.Target.position - transform.position;
        transform.Translate(dir.normalized * _speed * Time.deltaTime);

        var distance = _speed * Time.deltaTime;

        if (dir.magnitude <= distance)
        {
            Hit();
            PoolPush();
        }
    }

    private void Hit()
    {
        if (_hitable == null)
            return;

        _hitable.Hit(_hitArgs);
    }

    private void PoolPush()
    {
        _hitable = null;
        _hitArgs = default;

        _bulletPool.Push(this);

        _updateDisposable?.Dispose();
        _updateDisposable = null;
    }
}
