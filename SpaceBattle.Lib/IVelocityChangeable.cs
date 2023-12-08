namespace SpaceBattle.Lib;

public interface IVelocityChangeable
{
    public Vector Velocity { get; set; }
    IUObject Obj { get; }
}
