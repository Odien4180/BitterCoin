using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct MarketWeight
{
    public int Up;
    public int Down;
}

public class GameManager : Singleton<GameManager>
{
    [SerializeField]
    private List<Transform> _waypoints = new List<Transform>();
    public List<Transform> Waypoints => _waypoints;

    [SerializeField]
    private TMPro.TextMeshProUGUI _marginBalanceText;

    [SerializeField]
    private TMPro.TextMeshProUGUI _pnlText;

    [SerializeField]
    private Button _buybutton;
    [SerializeField]
    private Button _sellbutton;

    [SerializeField]
    private float _enemyGenerateInterval = 1.0f;
    [SerializeField]
    private float _priceInterval = 1.0f;
    [SerializeField]
    private float _currentPriceInterval = 0.25f;

    private ObjectPool<Enemy_Short> _enemyPool;

    private List<Enemy_Short> _activeEnemies = new List<Enemy_Short>();
    public List<Enemy_Short> ActiveEnemies => _activeEnemies;

    private List<int> _priceHistory = new List<int>();
    public List<int> PriceHistory => _priceHistory;

    private Subject<List<int>> _priceHistorySubject = new Subject<List<int>>();
    public Subject<List<int>> PriceHistorySubject => _priceHistorySubject;

    private Subject<int> _currentPriceSubject = new Subject<int>();
    public Subject<int> CurrentPriceSubject => _currentPriceSubject;

    [SerializeField]
    private int _lastPrice;
    public int LastPrice => _lastPrice;

    [SerializeField]
    private int _currentPrice;
    public int CurrentPrice => _currentPrice;
    [SerializeField]
    private int _historicalTopPrice;

    [SerializeField]
    private MarketWeight _marketWeight;
    public MarketWeight MarketWeight => _marketWeight;

    [SerializeField]
    private IntReactiveProperty _marginBalance;
    public IntReactiveProperty MarginBalance => _marginBalance;

    [SerializeField]
    private IntReactiveProperty _bitterCoinQty;

    [SerializeField]
    private FloatReactiveProperty _costAvg;

    [SerializeField]
    private FloatReactiveProperty _currentPnl;
    public FloatReactiveProperty CurrentPnl => _currentPnl;

    [SerializeField]
    private IntReactiveProperty _curSanity;
    public IntReactiveProperty CurSanity => _curSanity;

    private void Start()
    {
        var originShort = Resources.Load<Enemy_Short>("EnemyAssets/Prefabs/Enemy_Short");

        _enemyPool = ObjectPoolManager.Instance.CreatePool<Enemy_Short>(originShort, 10);

        _marginBalance = new IntReactiveProperty(10000);
        _bitterCoinQty = new IntReactiveProperty(0);
        _curSanity = new IntReactiveProperty(100);
        _currentPnl = new FloatReactiveProperty(0);
        _costAvg = new FloatReactiveProperty(0);

        _marginBalance.Subscribe(x => _marginBalanceText.text = x.ToString()).AddTo(this);
        _buybutton.OnClickAsObservable().Subscribe(_ => OnBuyButton()).AddTo(this);
        _sellbutton.OnClickAsObservable().Subscribe(_ => OnSellButton()).AddTo(this);
        _costAvg.Subscribe(_ => OnChangeCostAvg()).AddTo(this);
        _currentPnl.Subscribe(_ => SetPnl()).AddTo(this);
    }

    private void OnChangeCostAvg()
    {
        if (_costAvg.Value == 0)
        {
            _currentPnl.Value = 0;
            return;
        }
        _currentPnl.Value = (_currentPrice - _costAvg.Value) / _costAvg.Value * 100;
    }

    private void SetPnl()
    {
        _pnlText.text = _currentPnl.Value.ToString("F2") + "%";
    }

    private void Update()
    {
        if (Time.time % _currentPriceInterval < Time.deltaTime)
        {
            GenCurrentPrice();
        }

        if (Time.time % _priceInterval < Time.deltaTime)
        {
            AddPriceHistory();
            _priceHistorySubject.OnNext(_priceHistory);
        }

        if (Time.time % _enemyGenerateInterval < Time.deltaTime)
        {
            PopEnemy();
        }
    }

    private void GenCurrentPrice()
    {
        var offset = _lastPrice;

        var randomRange = Mathf.Max(_currentPrice / 100, 1);

        _currentPrice = offset + UnityEngine.Random.Range(-randomRange - _marketWeight.Down, randomRange + 1 + _marketWeight.Up);

        _currentPrice = Mathf.Max(100, _currentPrice);
        _historicalTopPrice = Mathf.Max(_historicalTopPrice, _currentPrice);
        _currentPriceSubject.OnNext(_currentPrice);
        OnChangeCostAvg();
    }

    private void AddPriceHistory()
    {
        _priceHistory.Add(_currentPrice);
        _lastPrice = _currentPrice;
    }

    private void PopEnemy()
    {
        var enemy = _enemyPool.Pop()
            .Initialize(_historicalTopPrice, _waypoints);
    }

    private void OnBuyButton()
    {
        if (_marginBalance.Value < _currentPrice)
            return;

        _costAvg.Value = (_costAvg.Value * _bitterCoinQty.Value + _currentPrice) / (_bitterCoinQty.Value + 1);
        _marginBalance.Value -= _currentPrice;
        _bitterCoinQty.Value += 1;
    }

    private void OnSellButton()
    {
        if (_bitterCoinQty.Value < 1)
            return;
        if (_bitterCoinQty.Value == 1)
        {
            _costAvg.Value = 0;
            _marginBalance.Value += _currentPrice;
            _bitterCoinQty.Value = 0;
            return;
        }

        _marginBalance.Value += _currentPrice;
        _bitterCoinQty.Value -= 1;
    }
}
