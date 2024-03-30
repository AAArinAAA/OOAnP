using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle.Lib;

public class SoftStop : ICommand
{
    private readonly ServerThread _thread;
    public SoftStop(ServerThread thread)
    {
        _thread = thread;
    }

    public void Execute()
    {
        var q = IoC.Resolve<BlockingCollection<ICommand>>("Get BlockingQ");
        if (_thread.Equals(Thread.CurrentThread))
        {
            _thread.UpdateBehaviour(() =>
            {
                if (q.TryTake(out var command))
                {
                    var cmd = q.Take();
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception e)
                    {
                        IoC.Resolve<ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                    }
                }
                else
                {
                    _thread.Stop();
                }
            });
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
