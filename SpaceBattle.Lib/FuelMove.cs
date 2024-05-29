namespace SpaceBattle.Lib;

public class FuelIteratorGetAndMove : IStrategyRenamed
{
    private readonly IEnumerator<object> _poit;

    public FuelIteratorGetAndMove(IEnumerator<object> poit)
    {
        _poit = poit;
    }

    public object Strategy(params object[] args)
    {
        var c = (int)_poit.Current;
        _poit.MoveNext();
        return c;
    }
}