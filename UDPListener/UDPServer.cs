using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Hwdtech;

namespace Udp;
public class UDPServer
{
    private readonly Thread _listenThread;
    //private readonly Socket? _socket;
    private Action _HookAfter = () => { };
    private Action _HookBefore = () => { };
    private readonly int _listenPort;

    private bool running = true;

    public UDPServer(int port)
    {
        _listenPort = port;
        var listener = new UdpClient(_listenPort);
        var RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, _listenPort);
        _listenThread = new Thread(() =>
        {
            try
            {
                IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

                _HookBefore();
                var bytes = new byte[1024];
                
                while (!bytes.SequenceEqual(Encoding.ASCII.GetBytes("STOP")) && running)
                {
                    bytes = listener.Receive(ref RemoteIpEndPoint);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                listener.Close();
                _HookAfter();
            }
        });
    }

    public void Start()
    {
        _listenThread.Start();
    }

    public void UpdateHookAfter(Action NewHookAfter)
    {
        _HookAfter = NewHookAfter;
    }

    public void UpdateHookBefore(Action NewHookBefore)
    {
        _HookBefore = NewHookBefore;
    }
    public void Stop()
    {
        running = false; 
    }

    public void Wait(int time)
    {
        _listenThread.Join(time);
    }

    public static void TableOfThreadsAndQueues()
    {
        var gameToThread = new ConcurrentDictionary<string, string>();
        var threadToQueue = new ConcurrentDictionary<string, BlockingCollection<ICommand>>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get GameToThreadDict", (object[] args) => gameToThread).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get ThreadToQueueDict", (object[] args) => threadToQueue).Execute();

        gameToThread.TryAdd("asdfg", "thefirst");
        threadToQueue.TryAdd("thefirst", new BlockingCollection<ICommand>());
    }
}
