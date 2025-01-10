using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private List<Transform> _waypoints = new List<Transform>();
    public List<Transform> Waypoints => _waypoints;

    [SerializeField]
    private float _enemyGenerateInterval = 1.0f;

    private ObjectPool<Enemy_Short> _enemyPool;

    private List<Enemy_Short> _activeEnemies = new List<Enemy_Short>();
    public List<Enemy_Short> ActiveEnemies => _activeEnemies;

    private void Start()
    {
        var originShort = Resources.Load<Enemy_Short>("EnemyAssets/Prefabs/Enemy_Short");

        _enemyPool = ObjectPoolManager.Instance.CreatePool<Enemy_Short>(originShort, 10);
    }

    private void Update()
    {
        if (Time.time % _enemyGenerateInterval < Time.deltaTime)
        {
            PopEnemy();
        }
    }
    private void PopEnemy()
    {
        var enemy = _enemyPool.Pop()
            .Initialize(_waypoints);
    }
}
