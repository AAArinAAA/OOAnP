using System.Collections.Concurrent;
using Hwdtech;

namespace SpaceBattle.Lib;
public class ServerThread
{
    public BlockingCollection<ICommand> _q;
    private bool _stop = false;
    public Thread _thread;
    private Action? _behaviour;

    public ServerThread(BlockingCollection<ICommand> q)
    {
        _q = q;
        _behaviour = () =>
        {
            while (!_stop)
            {
                var cmd = q.Take();
                try
                {
                    cmd.Execute();
                }
                catch (Exception _)
                {
                    IoC.Resolve<ICommand>("ExceptionHandler", cmd, _);
                }
            }
        };
        _thread = new Thread(() => _behaviour());
    }

    public void Start()
    {
        _thread.Start();
    }

    public void Stop()
    {
        _stop = true;
    }

    internal void UpdateBehaviour(Action NewBehaviour)
    {
        _behaviour = NewBehaviour;
    }
}
