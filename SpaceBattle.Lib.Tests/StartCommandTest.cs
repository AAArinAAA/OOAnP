﻿using System;
using System.Collections.Generic;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Lib.Tests;

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
    public void Positive_Test()
    {
        var movingCommandMock = new Mock<ICommand>();
        var commandMock = new Mock<ICommand>();
        var queueMock = new Mock<IQueue>();
        var injMock = new Mock<ICommand>();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Commands.Move", (object[] args) => movingCommandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Сommand", (object[] args) => commandMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Queue", (object[] args) => queueMock.Object).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.Commands.Injectable", (object[] args) => injMock.Object).Execute();

        _startMoveCommand.Execute();

        _moveCommandStartableMock.Verify(m => m.property, Times.Once());
        queueMock.Verify(q => q.Enqueue(It.IsAny<ICommand>()), Times.Once());
    }
}
