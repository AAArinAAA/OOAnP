using Moq;

namespace SpaceBattle.Lib.Tests;
public class MoveTest
{
    [Fact]
    public void MoveGood()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5)).Verifiable();

        movable.SetupGet(m => m.Velocity).Returns(new Vector(-5, 3)).Verifiable();

        var move_command = new MoveCommand(movable.Object);

        move_command.Execute();

        movable.VerifySet(m => m.Position = new Vector(7, 8));
        movable.VerifyAll();
    }

    [Fact]
    public void SetPosErr()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Throws(new System.Exception());

        movable.SetupGet(m => m.Velocity).Returns(new Vector(-5, 3)).Verifiable();

        var move_command = new MoveCommand(movable.Object);

        Assert.Throws<System.Exception>(() => move_command.Execute());
    }

    [Fact]
    public void GetSpeedErr()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5)).Verifiable();

        movable.SetupGet(m => m.Velocity).Throws(new System.Exception());

        var move_command = new MoveCommand(movable.Object);

        Assert.Throws<System.Exception>(() => move_command.Execute());
    }

    [Fact]
    public void GetPosErr()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5)).Verifiable();

        movable.SetupGet(m => m.Velocity).Returns(new Vector(-5, 3)).Verifiable();

        movable.SetupSet(m => m.Position = It.IsAny<Vector>()).Throws(new System.Exception());

        var move_command = new MoveCommand(movable.Object);

        Assert.Throws<System.Exception>(() => move_command.Execute());
    }
}