using System;
using System.Collections.Generic;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class ActionCommand : Lib.ICommand
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
public class StartMoveCommandTests
{
    private readonly Mock<IMoveCommandStartable> _moveCommandStartableMock;
    private readonly Mock<IUObject> _uObjectMock;
    private readonly StartMoveCommand _startMoveCommand;

    public StartMoveCommandTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        _moveCommandStartableMock = new Mock<IMoveCommandStartable>();
        _uObjectMock = new Mock<IUObject>();

        _moveCommandStartableMock.Setup(m => m.target).Returns(_uObjectMock.Object);
        _moveCommandStartableMock.Setup(m => m.property).Returns(new Dictionary<string, object>());

        _startMoveCommand = new StartMoveCommand(_moveCommandStartableMock.Object);
    }

    [Fact]
    public void Execute_RegistersTargetsAndPushesMovingCommand_WhenCalled()
    {
        var movingCommandMock = new Mock<Lib.ICommand>();
        var commandMock = new Mock<Lib.ICommand>();
        var queueMock = new Mock<IQueue>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Operation.Move", (object[] args) => movingCommandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "command", (object[] args) => commandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Queue", (object[] args) => queueMock.Object).Execute();

        _startMoveCommand.Execute();

        _moveCommandStartableMock.Verify(m => m.property, Times.Once());
        queueMock.Verify(q => q.Enqueue(It.IsAny<Lib.ICommand>()), Times.Once());
    }
}
