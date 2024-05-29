using System.Collections.Generic;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;

namespace SpaceBattle.Lib.Test;

public class SetIniPosTests
{
    [Fact]
    public void PosTest_SetIniPos()
    {
        Mock<ICommand> mcmd = new();
        mcmd.Setup(_m => _m.Execute()).Verifiable();

        Mock<IStrategyRenamed> mStrat = new();
        mStrat.Setup(_m => _m.Strategy(It.IsAny<object[]>())).Returns(mcmd.Object);

        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.SetIniPos", (object[] props) => new SetPositionStrategy().Strategy(props)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Services.GetStartingPoint", (object[] props) => (object)new Vector(1, 1)).Execute();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.UObject.Set", (object[] props) => mStrat.Object.Strategy(props)).Execute();

        var poit = new PosIterator(new List<int> { 3, 3 }, 2, 4);
        var iterStrat = new PosIterGetAndMove(poit);

        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.IniPosIter.Next", (object[] props) => iterStrat.Strategy()).Execute();

        Mock<IUObject> patient = new();

        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();
        IoC.Resolve<ICommand>("Game.SetIniPos", patient.Object).Execute();

        mcmd.VerifyAll();

        poit.Dispose();
    }
}
