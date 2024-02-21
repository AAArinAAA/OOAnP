using System.Collections.Concurrent;

namespace SpaceBattle.Lib;
public class ServerThread
{
    public required BlockingCollection<ICommand> _q;
    private bool _stop;
    public required Thread _thread;
    private Action? _behaviour;
    public void ServerThreadthread(BlockingCollection<ICommand> q)
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
                catch (Exception)
                {
                    Console.WriteLine("Поймал ошибку!");
                }
            }
        };
        _thread = new Thread(() =>
        {
            while (!_stop)
            {
                _behaviour();
            }
        });
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
    // запуск энпоинта
    // определение кол-ва потоков

}
