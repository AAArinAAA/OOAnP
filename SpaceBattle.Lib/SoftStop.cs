using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle.Lib;

public class SoftStop : ICommand
{
    private readonly BlockingCollection<ICommand> _queue;
    private readonly ServerThread _thread;
    public SoftStop(ServerThread thread, BlockingCollection<ICommand> queue)
    {
        _thread = thread;
        _queue = queue;
    }

    public void Execute()
    {
        _thread.UpdateBehaviour(() =>
        {
            if (_queue.TryTake(out var cmd) == true)
            {
                _queue.Take().Execute();
            }
            else
            {
                var id = IoC.Resolve<int>("Get ID", _thread);
                var stop_cmd = IoC.Resolve<ICommand>("CreateHardStopCommand", id);
                IoC.Resolve<ICommand>("HardStop", id, stop_cmd).Execute();
            }
        });
    }
}
