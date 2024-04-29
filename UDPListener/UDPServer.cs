using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using Hwdtech;
public class UDPServer
{
    private Thread? listenThread;
    private readonly Socket? _socket;

    private void StartListener()
    {
        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"));
        IoC.Resolve<ICommand>("Scopes.Current.Set", scope).Execute();
        var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        listenThread = new Thread(() =>
        {
            try
            {
                var bytes = new byte[1024];
                while (!bytes.SequenceEqual(Encoding.ASCII.GetBytes("STOP")))
                {
                    _socket.Receive(bytes);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _socket.Shutdown(SocketShutdown.Receive);
            }
        });
        listenThread?.Start();
    }

    public void Main()
    {
        StartListener();
    }
    public void Stop()
    {
        _socket?.Close();
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
