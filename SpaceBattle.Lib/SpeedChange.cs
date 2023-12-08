namespace SpaceBattle.Lib;

public class SpeedChange : ICommand
{
    private readonly IVelocityChangeable _vch;

    public SpeedChange(IVelocityChangeable vch, Vector velocity)
    {
        _vch = vch;
        _vch.Velocity = velocity;
    }

    public void Execute()
    {
        _vch.Obj.SetProperty("velocity", _vch.Velocity);
    }
}
