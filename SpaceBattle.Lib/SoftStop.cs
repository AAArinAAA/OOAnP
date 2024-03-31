using Hwdtech;

namespace SpaceBattle.Lib;
public class SoftStop : ICommand
{
    public ServerThread thread;
    public Action action = () => { };

    public SoftStop(ServerThread thread, Action action)
    {
        this.thread = thread;
        this.action = action;
    }
    public void Execute()
    {
        var queue = thread.GetQueue();
        if (thread.Equals(Thread.CurrentThread))
        {
            thread.UpdateBehavior(() =>
            {
                while (queue.TryTake(out var cmd))
                {
                    try
                    {
                        cmd.Execute();
                    }
                    catch (Exception e)
                    {
                        IoC.Resolve<ICommand>("ExceptionHandler.Handle", cmd, e).Execute();
                    }
                }

                thread.Stop();
                action();
            });
        }
        else
        {
            throw new Exception("Wrong Thread");
        }
    }
}
