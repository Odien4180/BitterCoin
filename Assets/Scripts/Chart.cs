using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class Chart : MonoBehaviour
{
    [SerializeField]
    private int _chartWidth = 30;

    [SerializeField]
    private RectTransform _chartContent;
    [SerializeField]
    private Image _comparePriceLine;

    private Stack<int> _visualPrices = new Stack<int>();
    private int _historicalTopPrice;
    private int _historicalBottomPrice;

    [SerializeField]
    private int _chartTopPrice = 300;
    [SerializeField]
    private int _chartBottomPrice = 0;

    [SerializeField]
    private GameObject _lastPriceDot;

    private LinkedList<ChartLine> _chartLines = new LinkedList<ChartLine>();
    private ChartLine _lastPriceLine;

    private ObjectPool<ChartLine> _chartLinePool;

    private int _lastPrice;
    private float _lastPricePosX;
    private float _currentPricePosX;

    private void Start()
    {
        var chartLineOrigin = Resources.Load<ChartLine>("MarketAssets/ChartLine");
        _chartLinePool = ObjectPoolManager.Instance.CreatePool<ChartLine>(chartLineOrigin, _chartWidth);
        _lastPriceLine = _chartLinePool.Pop();
        _lastPriceLine.transform.SetParent(_chartContent);

        GameManager.Instance.PriceHistorySubject
            .Subscribe(_ => ChartRefresh())
            .AddTo(this);

        GameManager.Instance.CurrentPriceSubject
            .Subscribe(OnChaneCurrentPrice)
            .AddTo(this);
    }

    private void ChartRefresh()
    {
        PushAllChartLines();
        SetVisualPrices();
        DrawChartHistory();
        DrawCurrentPrice(GameManager.Instance.CurrentPrice);
    }

    private void SetVisualPrices()
    {
        _visualPrices.Clear();
        var priceHistory = GameManager.Instance.PriceHistory;

        _historicalTopPrice = 0;
        _historicalBottomPrice = int.MaxValue;
        //Index 0 angle need count + 1
        for (int i = priceHistory.Count - 1; i >= 0 && _visualPrices.Count + 1 < _chartWidth; i--)
        {
            _visualPrices.Push(priceHistory[i]);

            if (priceHistory[i] > _historicalTopPrice)
                _historicalTopPrice = priceHistory[i];

            if (priceHistory[i] < _historicalBottomPrice)
                _historicalBottomPrice = priceHistory[i];
        }
    }

    private void PushAllChartLines()
    {
        foreach (var chartLine in _chartLines)
        {
            _chartLinePool.Push(chartLine);
        }
        _chartLines.Clear();
    }

    private void DrawChartHistory()
    {
        if (_visualPrices.Count == 0)
            return;

        _lastPrice = _visualPrices.Pop();

        for (int i = 0; i < _chartWidth; ++i)
        {
            if (_visualPrices.Count == 0)
                break;

            var line = _chartLinePool.Pop();
            line.transform.SetParent(_chartContent);
            _chartLines.AddLast(line);

            var visualChartWidth = _chartWidth + 2;
            var price = _visualPrices.Pop();
            var realChartWidth = _chartContent.rect.width;
            var realChartHeight = _chartContent.rect.height;
            var lastPos = new Vector2(i * realChartWidth / visualChartWidth, (_lastPrice - _chartBottomPrice) / (float)(_chartTopPrice - _chartBottomPrice) * realChartHeight);
            var currentPos = new Vector2((i + 1) * realChartWidth / visualChartWidth, (price - _chartBottomPrice) / (float)(_chartTopPrice - _chartBottomPrice) * realChartHeight);

            line.Initialize(lastPos, currentPos);
            _lastPrice = price;
            _lastPricePosX = currentPos.x;
            _currentPricePosX = (i + 2) * realChartWidth / visualChartWidth;
        }
    }

    private void OnChaneCurrentPrice(int currentPrice)
    {
        if (currentPrice < _chartBottomPrice)
        {
            //_chartTopPrice = _chartTopPrice - (_chartBottomPrice - currentPrice);
            _chartTopPrice = _historicalTopPrice;
            _chartBottomPrice = currentPrice;

            ChartRefresh();
            return;
        }
        else if (currentPrice > _chartTopPrice)
        {
            //_chartBottomPrice = _chartBottomPrice + (currentPrice - _chartTopPrice);
            _chartBottomPrice = _historicalBottomPrice;

            _chartTopPrice = currentPrice;
            ChartRefresh();
            return;
        }

        DrawCurrentPrice(currentPrice);
    }

    private void DrawCurrentPrice(int currentPrice)
    {
        var realChartHeight = _chartContent.rect.height;
        var lastPos = new Vector2(_lastPricePosX, (_lastPrice - _chartBottomPrice) / (float)(_chartTopPrice - _chartBottomPrice) * realChartHeight);
        var currentPos = new Vector2(_currentPricePosX, (currentPrice - _chartBottomPrice) / (float)(_chartTopPrice - _chartBottomPrice) * realChartHeight);

        _lastPriceDot.transform.localPosition = currentPos;

        _lastPriceLine.Initialize(lastPos, currentPos);

        _comparePriceLine.transform.localPosition = new Vector2(0, currentPos.y);
        if (currentPrice == _lastPrice)
            _comparePriceLine.color = Color.gray;
        else if (currentPrice > _lastPrice)
            _comparePriceLine.color = Color.green;
        else
            _comparePriceLine.color = Color.red;
    }
}
