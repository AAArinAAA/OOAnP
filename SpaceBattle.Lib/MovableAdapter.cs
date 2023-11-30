namespace SpaceBattle.Lib;
public class MovableAdapter : IMovable
{
    private readonly IUObject _obj;
    public MovableAdapter(IUObject obj)
    {
        _obj = obj;
    }
    public Vector Position
    {
        get => (Vector)_obj.GetProperty("Position");
        set => _obj.SetProperty("Position", value);
    }
    public Vector Velocity => (Vector)_obj.GetProperty("Velocity");
}
