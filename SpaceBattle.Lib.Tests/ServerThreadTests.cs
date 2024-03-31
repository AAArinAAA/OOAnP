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
                    var hasht = IoC.Resolve<Hashtable>("Get Hashtable");
                    var st = (ServerThread)hasht[(Guid)args[0]]!;
                    var q = st.GetQueue();
                    q.Add((ICommand)args[1]);
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
                    var hasht = IoC.Resolve<Hashtable>("Get Hashtable");
                    var st = (ServerThread)hasht[(Guid)args[0]]!;
                    new SoftStop(st, (Action)args[1]).Execute();
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

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute());
        var threadStoped = false;

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", uuid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        IoC.Resolve<ICommand>("Send Command", uuid, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid, hs).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid, cmd.Object).Execute();

        mre.WaitOne(1000);
        Assert.Single(q);
        Assert.True(threadStoped);
    }


    [Xunit.Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);
        var st2 = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid2 = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st2);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", uuid, () => { });
        IoC.Resolve<ICommand>("Send Command", uuid2, hs).Execute();

        Assert.Throws<Exception>(() => hs.Execute());
    }

    [Xunit.Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);
        var threadStoped = false;

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", uuid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute());

        IoC.Resolve<ICommand>("Send Command", uuid, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid, ss).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid, cmd.Object).Execute();

        mre.WaitOne(1000);
        Assert.True(threadStoped);
        Assert.Empty(q);
    }

    [Xunit.Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        var q = new BlockingCollection<ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        var st2 = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var uuid2 = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st2);

        IoC.Resolve<ICommand>("Create and Start Thread", uuid).Execute();

        var mre = new ManualResetEvent(false);

        var threadStoped = false;

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", uuid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        var ecmd = new Mock<ICommand>();
        ecmd.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<ICommand>("Send Command", uuid, ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid, ss).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid, ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", uuid2, ecmd.Object).Execute();

        mre.WaitOne(1000);

        Assert.True(threadStoped);
        Assert.Empty(q);
        Assert.Throws<Exception>(() => ss.Execute());
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
