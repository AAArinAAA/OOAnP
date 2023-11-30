namespace SpaceBattle.Lib
{
    public interface IMoveCommandStartable
    {
        IUObject UObject { get; }
        Vector Velocity { get; }
        Queue<ICommand> Queue { get; }
    }
}
