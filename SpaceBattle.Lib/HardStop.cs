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
       Thread.CurrentThread.

    }
}
