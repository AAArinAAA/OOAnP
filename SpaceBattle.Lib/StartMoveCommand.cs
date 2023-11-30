namespace SpaceBattle.Lib;

public class StartMoveCommand : ICommand
{
    private readonly IMoveCommandStartable _obj;

    public StartMoveCommand(IMoveCommandStartable obj)
    {
        _obj = obj;
    }

    public void Execute()
    {
        Hwdtech.IoC.Resolve<SpaceBattle.Lib.ICommand>("SpaceBattle.Lib.SpeedChange", _obj.UObject, _obj.Velocity);
        var smth_cmd = Hwdtech.IoC.Resolve<SpaceBattle.Lib.ICommand>("SpaceBattle.Lib.Move", _obj.UObject);
        _ = Hwdtech.IoC.Resolve<Queue<SpaceBattle.Lib.ICommand>>("SpaceBattle.Queue");
        _obj.Queue.Enqueue(smth_cmd);
    }
}
