namespace SpaceBattle.Lib;

public class SpeedChange : ICommand
{
    private readonly IUObject _obj;
    private readonly Vector _velocity;

    public SpeedChange(object obj, Vector velocity)
    {
        _obj = (IUObject)obj;
        _velocity = velocity;
    }

    public void Execute()
    {
        _obj.SetProperty("velocity", _velocity);
    }
}
