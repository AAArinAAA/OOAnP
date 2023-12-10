using System;
using Moq;

namespace SpaceBattle.Lib.Tests;

public class SpeedChangeTests
{

    [Fact]
    public void SpeedChange()
    {
        var m = new Mock<IVelocityChangeable>();
        var o = new Mock<IUObject>();
        m.SetupGet(m => m.Velocity).Returns(new Vector(1, 0)).Verifiable();
        m.SetupGet(m => m.Obj).Returns(o.Object).Verifiable();

        o.Setup(o => o.SetProperty("velocity", new Vector(1, 0))).Verifiable();

        var cmd = new SpeedChange(m.Object, new Vector(1, 0));
        cmd.Execute();
        o.VerifyAll();
    }

    [Fact]
    public void Speed_Not_Changed_Without_Obj()
    {
        var m = new Mock<IVelocityChangeable>();
        var o = new Mock<IUObject>();
        m.SetupGet(m => m.Velocity).Returns(new Vector(1, 0)).Verifiable();
        m.SetupGet(m => m.Obj).Throws(new NotImplementedException());

        o.Setup(o => o.SetProperty("velocity", new Vector(2, 0))).Verifiable();

        var cmd = new SpeedChange(m.Object, new Vector(2, 0));
        Assert.Throws<NotImplementedException>(cmd.Execute);
    }
}
