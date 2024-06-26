﻿using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
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
    }

    [Fact]
    public void MessageWasRecivedAndAddedToNessesaryQueue()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var message = new CommandData
        {
            CommandType = "fire",
            gameId = "asdfg",
            gameItemId = "548",
        };

        var listenport = 11103;
        var server = new Udp.EndPoint(listenport);
        var checkStart = new ManualResetEvent(false);
        server.UpdateHookBefore(() =>
        {
            IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "InterpretCommand", (object[] args) =>
            {
                var cmd = new Mock<ICommand>();
                cmd.Setup(cmd => cmd.Execute());
                return cmd.Object;
            }).Execute();

            IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get Thread ID by Game ID", (object[] args) =>
            {
                var threadID = (object)Guid.NewGuid().ToString();
                return (object)threadID;
            }).Execute();

            IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Send Message", (object[] args) =>
            {
                return new ActionCommand(() =>
                {
                    var send_cmd = new Mock<ICommand>();
                    send_cmd.Setup(cmd => cmd.Execute());
                    var q = new BlockingCollection<ICommand>(10);
                });

            }).Execute();

            checkStart.Set();
        });
        var checkStop = new ManualResetEvent(false);
        server.UpdateHookAfter(() => checkStop.Set());
        server.Start();

        var client = new UdpClient();

        checkStart.WaitOne();

        var s = JsonConvert.SerializeObject(message, Formatting.Indented);
        var sendbuf = Encoding.ASCII.GetBytes(s);

        var ep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), listenport);

        client.Send(sendbuf, sendbuf.Length, ep);
        var message2 = Encoding.ASCII.GetBytes("STOP");
        client.Send(message2, message2.Length, ep);

        checkStop.WaitOne();

        client.Close();

        server.Stop();

    }
}
