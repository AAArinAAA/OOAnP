using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Hwdtech;
using Hwdtech.Ioc;
using Newtonsoft.Json;
using Udp;

namespace SpaceBattle.Test;

public class ActionCommand : ICommand
{
    private readonly Action _action;
    public ActionCommand(Action action)
    {
        _action = action;
    }

    public void Execute()
    {
        _action();
    }
}

public class EndPointTests
{
    public EndPointTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var dictOfCommands = new ConcurrentDictionary<string, ICommand>();
        var command = new ActionCommand(() => { });
        dictOfCommands.TryAdd("fire", command);
        dictOfCommands.TryAdd("start", command);
        dictOfCommands.TryAdd("stop", command);
        dictOfCommands.TryAdd("spin", command);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get CommandsDict", (object[] args) => dictOfCommands).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Send Message",
        (object[] args) =>
        {
            var dictthread = IoC.Resolve<ConcurrentDictionary<string, string>>("Get GameToThreadDict");
            var threadId = dictthread[(string)args[0]];
            var dictqu = IoC.Resolve<ConcurrentDictionary<string, BlockingCollection<ICommand>>>("Get ThreadToQueueDict");
            var commanddd = (CommandData)args[1];
            var command = commanddd.CommandType;
            dictqu[(string)threadId].Add(IoC.Resolve<ConcurrentDictionary<string, ICommand>>("Get CommandsDict")[command!]);
            return dictqu[(string)threadId];
        }).Execute();

    }

    [Fact]
    public void MessageWasRecivedAndAddedToNessesaryQueue()
    {
        //var client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();       

        var listenport = 11103;
        var server = new UDPServer(listenport);
        var checkStart = new ManualResetEvent(false);
        server.UpdateHookBefore(() => 
        {
            IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "InterpretCommand", (object[] args) => 
                new ActionCommand(() => 
                { 
                    
                })
            ).Execute();
            checkStart.Set();
        });
        var checkStop = new ManualResetEvent(false);
        server.UpdateHookAfter(() => checkStop.Set());
        server.Start();

        var client = new UdpClient();

        checkStart.WaitOne();

        var message = new CommandData
        {
            CommandType = "fire",
            gameId = "asdfg",
            gameItemId = "548",
        };
        var s = JsonConvert.SerializeObject(message, Formatting.Indented);
        var sendbuf = Encoding.ASCII.GetBytes(s);

        var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listenport);

        // client.Connect(ep);
        client.Send(sendbuf, sendbuf.Length, ep);
        var message2 = Encoding.ASCII.GetBytes("STOP");
        client.Send(message2, message2.Length, ep);

        checkStop.WaitOne();

        //Udp.EndPoint.GetMessage(sendbuf);
        client.Close();

        server.Stop();

        var qu = IoC.Resolve<BlockingCollection<ICommand>>("Get Queue");
        Assert.Single(qu);
    }
}
