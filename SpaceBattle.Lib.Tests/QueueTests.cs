using System.Collections.Concurrent;
using System.Collections.Generic;
using Moq;

namespace SpaceBattle.Lib.Tests;
public class QueueTests
{
    [Fact]
    public void QueueTest1()
    {
        var qReal = new Queue<ICommand>();
        var qMock = new Mock<IQueue>();

        _ = qMock.Setup(q => q.Dequeue()).Returns(() => qReal.Dequeue());

        var cmd = new Mock<ICommand>();
        qReal.Enqueue(cmd.Object);

        Assert.Equal(cmd.Object, qMock.Object.Dequeue());
    }

    [Fact]
    public void QueueTest2()
    {
        var qBC = new BlockingCollection<ICommand>();

        var qReal = new Queue<ICommand>();
        var qMock = new Mock<IQueue>();

        qMock.Setup(q => q.Dequeue()).Returns(() => qReal.Dequeue());
        qMock.Setup(q => q.Enqueue(It.IsAny<ICommand>())).Callback(
            (ICommand cmd) => qReal.Enqueue(cmd));

        var cmd = new Mock<ICommand>();

        qMock.Object.Enqueue(cmd.Object);

        Assert.Equal(cmd.Object, qMock.Object.Dequeue());
    }
}
