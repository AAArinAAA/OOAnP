using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace SpaceBattle.Lib.Test;
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

public class ServerTheardTests
{
    public ServerTheardTests()
    {

        new InitScopeBasedIoCImplementationCommand().Execute();

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var idDict = new Dictionary<int, ServerThread>();
        var queueDict = new Dictionary<int, BlockingCollection<ICommand>>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        idDict.Add((int)args[0], (ServerThread)args[1]);
                        var st = (ServerThread)args[1];
                        st.Start();
                        if (args.Length == 3 && args[2] != null)
                        {
                            new ActionCommand((Action)args[2]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Add Command To QueueDict",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        queueDict.Add((int)args[0], (BlockingCollection<ICommand>)args[1]);
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Send Command",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        var queue = queueDict[(int)args[0]];
                        queue.Add((ICommand)args[1]);
                        if (args.Length == 3 && args[2] != null)
                        {
                            new ActionCommand((Action)args[2]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Hard Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        new HardStop(idDict[(int)args[0]]).Execute();
                        if (args.Length == 2 && args[1] != null)
                        {
                            new ActionCommand((Action)args[1]).Execute();
                        }
                    }
                );
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Soft Stop The Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                    {
                        new SoftStop(idDict[(int)args[0]], (BlockingCollection<ICommand>)args[2], (Action)args[1]).Execute();
                    }
                );
            }
        ).Execute();
    }
    [Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<ICommand>("Add Command To QueueDict", 1, q).Execute();
        IoC.Resolve<ICommand>("Create and Start Thread", 1, st).Execute();

        var command = new Mock<ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        var mre = new ManualResetEvent(false);
        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", 1, () => { mre.Set(); });

        IoC.Resolve<ICommand>("Send Command", 1, command.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", 1, hs).Execute();
        IoC.Resolve<ICommand>("Send Command", 1, command.Object).Execute();

        mre.WaitOne(1000);

        Assert.Single(q);
        command.Verify(m => m.Execute(), Times.Once);
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        IoC.Resolve<ICommand>("Add Command To QueueDict", 4, q).Execute();
        IoC.Resolve<ICommand>("Create and Start Thread", 4, st).Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", 4, () => { mre.Set(); });

        IoC.Resolve<ICommand>("Send Command", 4, hs).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => hs.Execute());
        Assert.Empty(q);
    }

    [Fact]
    public void HashCodeTheSame()
    {
        var queue1 = new BlockingCollection<ICommand>();
        var serverThread1 = new ServerThread(queue1, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var queue2 = new BlockingCollection<ICommand>();
        var serverThread2 = new ServerThread(queue2, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Assert.True(serverThread1.GetHashCode() != serverThread2.GetHashCode());
    }

    [Fact]
    public void NegativeEqualThreads()
    {
        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Assert.False(st.Equals(null));
    }

    [Fact]
    public void PositiveEqualThreads()
    {
        var q1 = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q1, Thread.CurrentThread);
        var st2 = new ServerThread(q1, Thread.CurrentThread);

        Assert.False(st1.Equals(st2));
    }

    [Fact]
    public void PositiveCurrentEqualThreads()
    {
        var q = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var nothing = 22;

        Assert.False(st1.Equals(nothing));
    }
}
