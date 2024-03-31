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

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var threadHashtable = new Hashtable();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Get Hashtable", (object[] args) => threadHashtable).Execute();

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
                        st?.Execute();
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

    [Fact]
    public void HardStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", UUid).Execute();

        var mre = new ManualResetEvent(false);

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute());
        var threadStoped = false;

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", UUid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        IoC.Resolve<ICommand>("Send Command", UUid, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", UUid, hs).Execute();
        IoC.Resolve<ICommand>("Send Command", UUid, cmd.Object).Execute();

        mre.WaitOne(1000);
        Assert.Single(q);
        Assert.True(threadStoped);
    }

    [Fact]
    public void SoftStopShouldStopServerThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => new ActionCommand(() => { })).Execute();

        var q = new BlockingCollection<ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", UUid).Execute();

        var mre = new ManualResetEvent(false);
        var threadStoped = false;

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", UUid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        var cmd = new Mock<ICommand>();
        cmd.Setup(m => m.Execute());

        IoC.Resolve<ICommand>("Send Command", UUid, cmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", UUid, ss).Execute();
        IoC.Resolve<ICommand>("Send Command", UUid, cmd.Object).Execute();

        mre.WaitOne(1000);

        Assert.True(threadStoped);
        Assert.Empty(q);
    }

    [Fact]
    public void SoftStopShouldStopServerThreadWithCommandWithException()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var cmd = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler.Handle", (object[] args) => cmd.Object).Execute();

        var q = new BlockingCollection<ICommand>(10);

        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);

        IoC.Resolve<ICommand>("Create and Start Thread", UUid).Execute();

        var mre = new ManualResetEvent(false);

        var threadStoped = false;

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", UUid, () =>
        {
            mre.Set();
            threadStoped = true;
        });

        var ecmd = new Mock<ICommand>();
        ecmd.Setup(m => m.Execute()).Throws(new Exception());

        IoC.Resolve<ICommand>("Send Command", UUid, ecmd.Object).Execute();
        IoC.Resolve<ICommand>("Send Command", UUid, ss).Execute();
        IoC.Resolve<ICommand>("Send Command", UUid, ecmd.Object).Execute();

        mre.WaitOne(1000);

        Assert.True(threadStoped);
        Assert.Empty(q);
    }

    [Fact]
    public void HardStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);
        var st2 = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid2 = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st2);

        IoC.Resolve<ICommand>("Create and Start Thread", UUid).Execute();

        var hs = IoC.Resolve<ICommand>("Hard Stop The Thread", UUid, () => { });
        IoC.Resolve<ICommand>("Send Command", UUid2, hs).Execute();

        Assert.Throws<Exception>(() => hs.Execute());
    }

    [Fact]
    public void SoftStopCanNotStopServerBecauseOfWrongThread()
    {
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current"))).Execute();

        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st);
        var st2 = new ServerThread(q, IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Current")));
        var UUid2 = IoC.Resolve<Guid>("Add Thread To Hashtable And Get UUid", st2);

        IoC.Resolve<ICommand>("Create and Start Thread", UUid).Execute();

        var ss = IoC.Resolve<ICommand>("Soft Stop The Thread", UUid, () => { });
        IoC.Resolve<ICommand>("Send Command", UUid2, ss).Execute();

        Assert.Throws<Exception>(() => ss.Execute());
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
    public void EqualThreadsWithNull()
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
    public void AbsoluteDifferendEquals()
    {
        var q = new BlockingCollection<ICommand>(10);

        var st1 = new ServerThread(q, Thread.CurrentThread);
        var nothing = 15;

        Assert.False(st1.Equals(nothing));
    }
}
