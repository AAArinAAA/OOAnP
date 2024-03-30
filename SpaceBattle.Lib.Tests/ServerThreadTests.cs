using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
namespace SpaceBattle.Lib.Test;

public class ServerTheardTests
{
    public ServerTheardTests()
    {

        new InitScopeBasedIoCImplementationCommand().Execute();

        var scope = IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"));
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", scope).Execute();

        var threadhashTable = new Hashtable();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Get Hashtable",
            (object[] args) =>
            threadhashTable
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
        "Add Thread To Hashtable And Get UUid",
            (object[] args) =>
            {
                var threadHashtable = IoC.Resolve<Hashtable>("Get Hashtable");
                var UUid = Guid.NewGuid();
                threadHashtable.Add(UUid, (ServerThread)args[0]);
                return (object)UUid;
            }
        ).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register",
            "Create and Start Thread",
            (object[] args) =>
            {
                return new ActionCommand(() =>
                {
                    var hasht = IoC.Resolve<Hashtable>("Get Hashtable");
                    var st = (ServerThread)hasht[(Guid)args[0]]!;
                    st?.Start();
                    if (args.Length == 2 && args[1] != null)
                    {
                        new ActionCommand((Action)args[1]).Execute();
                    }
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
                    var q = IoC.Resolve<BlockingCollection<ICommand>>("Get BlockingQ");
                    q.Add((ICommand)args[0]);
                    if (args.Length == 2 && args[1] != null)
                    {
                        new ActionCommand((Action)args[1]).Execute();
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
                    var hasht = IoC.Resolve<Hashtable>("Get Hashtable");
                    var st = (ServerThread)hasht[(Guid)args[0]]!;
                    new HardStop(st).Execute();
                    if (args.Length == 3 && args[2] != null)
                    {
                        new ActionCommand((Action)args[2]).Execute();
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
                    var hasht = IoC.Resolve<Hashtable>("Get Hashtable");
                    var st = (ServerThread)hasht[(Guid)args[0]]!;
                    new SoftStop(st).Execute();
                }
                );
            }
        ).Execute();
    }

    [Xunit.Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get BlockingQ", (object[] args) => q).Execute();

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var executedOnce = false;
        var mre = new ManualResetEvent(false);

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute()).Callback(() =>
        {
            if (!executedOnce)
            {
                executedOnce = true;
                mre.Set();
            }
        });

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", uuid);

        IoC.Resolve<ICommand>("Send Command", cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", hs).Execute();
        IoC.Resolve<ICommand>("Send Command", cmd.Object).Execute();

        mre.WaitOne(1000);
        Xunit.Assert.Single(q);
    }

    [Xunit.Fact]
    public void HardStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get BlockingQ", (object[] args) => q).Execute();

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", uuid);
        var ecmd = new Mock<ICommand>();
        var executeActions = new Action[]
        {
            () => {},
            () => mre.Set()
        };

        var executionStep = 0;

        ecmd.Setup(m => m.Execute()).Callback(() =>
        {
            executeActions[executionStep]();
            executionStep++;
        }).Throws(new Exception());

        IoC.Resolve<ICommand>("Send Command", ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", hs).Execute();
        IoC.Resolve<ICommand>("Send Command", ecmd.Object).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Throws<Exception>(() => hs.Execute());
        Xunit.Assert.Single(q);
    }

    [Xunit.Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<ICommand>(10);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get BlockingQ", (object[] args) => q).Execute();

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));

        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", uuid);

        IoC.Resolve<ICommand>("Send Command", hs).Execute();

        mre.WaitOne(1000);

        Assert.Throws<Exception>(() => hs.Execute());
        Assert.Empty(q);
    }

    [Xunit.Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get BlockingQ", (object[] args) => q).Execute();

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", uuid);

        var cmd = new Mock<ICommand>();
        var executeActions = new Action[]
        {
            () => {},
            () => mre.Set()
        };

        var executionStep = 0;

        cmd.Setup(m => m.Execute()).Callback(() =>
        {
            executeActions[executionStep]();
            executionStep++;
        });

        IoC.Resolve<ICommand>("Send Command", cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", ss).Execute();
        IoC.Resolve<ICommand>("Send Command", cmd.Object).Execute();

        mre.WaitOne(1000);
        Xunit.Assert.Empty(q);
    }

    [Xunit.Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        var q = new BlockingCollection<ICommand>(10);
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get BlockingQ", (object[] args) => q).Execute();

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", uuid, () => { mre.Set(); });

        var ecmd = new Mock<ICommand>();
        var executeActions = new Action[]
        {
            () => {},
            () => mre.Set()
        };

        var executionStep = 0;

        ecmd.Setup(m => m.Execute()).Callback(() =>
        {
            executeActions[executionStep]();
            executionStep++;
        }).Throws(new Exception());

        IoC.Resolve<ICommand>("Send Command", ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", ss).Execute();
        IoC.Resolve<ICommand>("Send Command", ecmd.Object).Execute();

        mre.WaitOne(1000);

        Xunit.Assert.Throws<Exception>(() => ss.Execute());

        Xunit.Assert.Empty(q);
    }

    [Xunit.Fact]
    public void HashCodeTheSame()
    {
        var q1 = new BlockingCollection<ICommand>();
        var sT1 = new ServerThread(q1, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var q2 = new BlockingCollection<ICommand>();
        var sT2 = new ServerThread(q2, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Xunit.Assert.True(sT1.GetHashCode() != sT2.GetHashCode());
    }

    [Xunit.Fact]
    public void EqualThreadsWithNull()
    {
        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        Xunit.Assert.False(st.Equals(null));
    }

    [Xunit.Fact]
    public void PositiveEqualThreads()
    {
        var q1 = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q1, Thread.CurrentThread);
        var st2 = new ServerThread(q1, Thread.CurrentThread);

        Xunit.Assert.False(st1.Equals(st2));
    }

    [Xunit.Fact]
    public void AbsoluteDifferentEquals()
    {
        var q = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var not_st = 15;

        Xunit.Assert.False(st1.Equals(not_st));
    }
}
