using Hwdtech;

namespace SpaceBattle.Lib;

public class HardStop : ICommand
{
    private readonly ServerThread _thread;
    public HardStop(ServerThread thread)
    {
        _thread = thread;
    }

    public void Execute()
    {
        var id = IoC.Resolve<int>("Get ID", _thread);
        var stop_cmd = IoC.Resolve<ICommand>("CreateHardStopCommand", id);
        IoC.Resolve<ICommand>("HardStop", id, stop_cmd).Execute();
    }
}
