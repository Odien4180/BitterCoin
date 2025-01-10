using System;
using System.Collections;
using System.Collections.Generic;

public class DisposablePoco : IDisposable, ICollection<IDisposable>
{
    private readonly object _gate = new object();

    private bool _disposed;
    private List<IDisposable> _disposables;
    private int _count;
    public int Count => _count;
    public bool IsReadOnly => false;

    private const int SHRINK_THRESHOLD = 64;


    public DisposablePoco()
    {
        _disposables = new List<IDisposable>();
    }

    public DisposablePoco(int capacity)
    {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException("capacity");

        _disposables = new List<IDisposable>(capacity);
    }

    public DisposablePoco(params IDisposable[] disposables)
    {
        if (disposables == null)
            throw new ArgumentNullException("disposables");

        _disposables = new List<IDisposable>(disposables);
        _count = _disposables.Count;
    }

    public DisposablePoco(IEnumerable<IDisposable> disposables)
    {
        if (disposables == null)
            throw new ArgumentNullException("disposables");

        _disposables = new List<IDisposable>(disposables);
        _count = _disposables.Count;
    }

    public void Add(IDisposable item)
    {
        if (item == null)
            throw new ArgumentNullException("item");

        var shouldDispose = false;
        lock (_gate)
        {
            shouldDispose = _disposed;
            if (!_disposed)
            {
                _disposables.Add(item);
                _count++;
            }
        }
        if (shouldDispose)
            item.Dispose();
    }

    public virtual void Dispose()
    {
        var currentDisposables = default(IDisposable[]);
        lock (_gate)
        {
            if (!_disposed)
            {
                _disposed = true;
                currentDisposables = _disposables.ToArray();
                _disposables.Clear();
                _count = 0;
            }
        }

        if (currentDisposables != null)
        {
            foreach (var d in currentDisposables)
                if (d != null)
                    d.Dispose();
        }
    }

    public bool Remove(IDisposable item)
    {
        if (item == null)
            throw new ArgumentNullException("item");

        var shouldDispose = false;

        lock (_gate)
        {
            if (!_disposed)
            {
                var i = _disposables.IndexOf(item);
                if (i >= 0)
                {
                    shouldDispose = true;
                    _disposables[i] = null;
                    _count--;

                    if (_disposables.Capacity > SHRINK_THRESHOLD && _count < _disposables.Capacity / 2)
                    {
                        var old = _disposables;
                        _disposables = new List<IDisposable>(_disposables.Capacity / 2);

                        foreach (var d in old)
                            if (d != null)
                                _disposables.Add(d);
                    }
                }
            }
        }

        if (shouldDispose)
            item.Dispose();

        return shouldDispose;
    }
    public void Clear()
    {
        var currentDisposables = default(IDisposable[]);
        lock (_gate)
        {
            currentDisposables = _disposables.ToArray();
            _disposables.Clear();
            _count = 0;
        }

        foreach (var d in currentDisposables)
            if (d != null)
                d.Dispose();
    }

    public bool Contains(IDisposable item)
    {
        if (item == null)
            throw new ArgumentNullException("item");

        lock (_gate)
        {
            return _disposables.Contains(item);
        }
    }

    public void CopyTo(IDisposable[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException("array");
        if (arrayIndex < 0 || arrayIndex >= array.Length)
            throw new ArgumentOutOfRangeException("arrayIndex");

        lock (_gate)
        {
            var disArray = new List<IDisposable>();
            foreach (var item in _disposables)
            {
                if (item != null) disArray.Add(item);
            }

            Array.Copy(disArray.ToArray(), 0, array, arrayIndex, array.Length - arrayIndex);
        }
    }

    public IEnumerator<IDisposable> GetEnumerator()
    {
        var res = new List<IDisposable>();

        lock (_gate)
        {
            foreach (var d in _disposables)
            {
                if (d != null) res.Add(d);
            }
        }

        return res.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public bool IsDisposed
    {
        get { return _disposed; }
    }
}
