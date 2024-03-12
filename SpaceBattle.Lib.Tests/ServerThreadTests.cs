using System;
using System.Collections.Concurrent;
using System.Threading;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace SpaceBattle.Lib.Tests;

public class ActionCommand: ICommand 
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

public class ServerThreadTests
{
public ServerThreadTests() 
{

    new InitScopeBasedIoCImplementationCommand().Execute();

    IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "HardStop",
        (object[] args) => {
            var thread = (ServerThread)args[0];
            var action = (Action)args[1]; 
            return new ActionCommand (
                () => {
                    new HardStop(thread).Execute();
                    new ActionCommand(action).Execute();
                }
            );
        }
    ).Execute();
}

     [Fact]
    public void HardStopShouldStopServerThread()
    {

        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.Root"));
        
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "ExceptionHandler", (object[] args) => new ActionCommand(() => {})).Execute();
        
       

        var q = new BlockingCollection<ICommand>(10);
        var st = new ServerThread(q);

        var command = new Mock<ICommand>();
        command.Setup(m => m.Execute()).Verifiable();

        q.Add(command.Object);
        var mre = new ManualResetEvent(false);

        
        IoC.Resolve<Hwdtech.ICommand>(
            "IoC.Register", 
            "HardStopCommand", 
            (object[] args) => new ActionCommand(() => {mre.Set();})
        ).Execute();
   

        q.Add(IoC.Resolve<ICommand>("HardStopCommand"));

        q.Add(command.Object);
        
        st.Start();
        
        Assert.True(mre.WaitOne(1000));

        Assert.Empty(q);
        command.Verify(m=> m.Execute(), Times.AtLeast(2));
    }
}
