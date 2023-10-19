using Moq;


namespace SpaceBattle.Lib.Tests;
public class MoveTest
{
    [Fact]
    public void MoveGood()
    {
        var movable = new Mock<IMovable>();
        movable.SetupGet(m => m.Position).Returns(new Vector(12, 5)).Verifiable();

        movable.SetupGet(m => m.Velocity).Returns(new Vector(-7, 3)).Verifiable();

        var move_command = new MoveCommand(movable.Object);

        b.action();

        movable.VerifySet(m => m.Position = new Vector(5, 8));
    }

    [Fact]
    public void SetPosErr()
    {
        // Arrange
        var a = new Mock<IMovable>();
        a.SetupGet(m => m.Position).Throws(new Exception());

        a.SetupGet(m => m.Velocity).Returns(new Vector(-7, 3)).Verifiable();

        var b = new MoveCommand(a.Object);

        // Act
        // Assert
        Assert.Throws<Exception>(() => b.action());
    }

    [Fact]
    public void GetSpeedErr()
    {
        // Arrange
        var a = new Mock<IMovable>();
        a.SetupGet(m => m.position).Returns(new Vector(12, 5)).Verifiable();

        a.SetupGet(m => m.velocity).Throws(new Exception());

        var b = new MoveCommand(a.Object);

        // Act
        // Assert
        Assert.Throws<Exception>(() => b.action());
    }

    [Fact]
    public void GetPosErr()
    {
        // Arrange
        var a = new Mock<IMovable>();
        a.SetupGet(m => m.position).Returns(new Vector(12, 5)).Verifiable();

        a.SetupGet(m => m.velocity).Returns(new Vector(-7, 3)).Verifiable();

        a.SetupSet(m => m.position = It.IsAny<Vector>()).Throws(new Exception());

        var b = new MoveCommand(a.Object);

        // Act
        // Assert
        Assert.Throws<Exception>(() => b.action());
    }
}