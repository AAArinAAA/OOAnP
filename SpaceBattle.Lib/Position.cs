namespace SpaceBattle.Lib;

public class PosIterGetAndMove : IStrategyRenamed
{
    private readonly IEnumerator<object> poit;

    public PosIterGetAndMove(IEnumerator<object> poit)
    {
        this.poit = poit;
    }

    public object Strategy(params object[] args)
    {
        var c = (Vector)poit.Current;
        var m = poit.MoveNext();
        if (!m)
        {
            poit.Reset();
        }

        return c;
    }
}
