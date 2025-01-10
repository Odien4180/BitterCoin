public class Presenter<T> : DisposablePoco where T : MonoView
{
    private T _view;
    public T View
    {
        get { return _view; }
        set { _view = value; }
    }
}
