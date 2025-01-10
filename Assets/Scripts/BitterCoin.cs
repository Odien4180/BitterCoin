using UniRx;
using Unity.VisualScripting;
using UnityEngine;

public class BitterCoin : MonoBehaviour
{
    [SerializeField]
    private int _rotSpeed = 100;

    [SerializeField]
    private float _shootInterval = 1.0f;
    private float _shootTimer = 0;

    [SerializeField]
    private int _damage = 10;

    private ObjectPool<Bullet> _bulletPool;

    private void Start()
    {
        var bulletOrigin = Resources.Load<Bullet>("CoinAssets/Prefabs/Bullet");
        _bulletPool = ObjectPoolManager.Instance.CreatePool<Bullet>(bulletOrigin, 10);

        Observable.EveryUpdate()
            .Subscribe(_ => EveryUpdate())
            .AddTo(this);
    }

    private void EveryUpdate()
    {
        RotateEveryFrame();
        ShootBullet();
    }

    private void RotateEveryFrame()
    {
        transform.Rotate(Vector3.up, _rotSpeed * Time.deltaTime);
    }

    private void ShootBullet()
    {
        _shootTimer += Time.deltaTime;

        if (_shootTimer < _shootInterval)
            return;

        _shootTimer -= _shootInterval;

        Enemy_Short nearestEnemy = null;
        float hitableDistance = 0;
        foreach (var enemy in GameManager.Instance.ActiveEnemies)
        {
            var distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (hitableDistance == 0 || distance < hitableDistance)
            {
                hitableDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null)
            return;

        var bullet = _bulletPool.Pop();
        bullet.Initialize(nearestEnemy, new HitArgs
        {
            FirePosition = transform.position,
            Target = nearestEnemy.transform,
            Damage = _damage,
        });
    }
}
