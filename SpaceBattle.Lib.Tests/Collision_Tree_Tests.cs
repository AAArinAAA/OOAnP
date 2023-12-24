using System;
using System.Collections.Generic;
using System.IO;
using Hwdtech;
using Hwdtech.Ioc;
using Moq;
using Xunit;

namespace SpaceBattle.Test;

public class DecisionTreesTests
{

    public DecisionTreesTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();
    }

    [Fact]
    public void PositiveBuildingDecisionTreesTest()
    {
        var path = "C:/Users/NoteBook/from_wsl/root/OOAnP/SpaceBattle.Lib.Tests/test.txt";
        var getDecisionTreesStrategy = new Mock<IStrategy>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.GetDecisionTrees", (object[] args) => getDecisionTreesStrategy.Object.Strategy(args)).Execute();
        getDecisionTreesStrategy.Setup(t => t.Strategy(It.IsAny<object[]>())).Returns(new Dictionary<int, object>()).Verifiable();

        var bdts = new BuildingDecisionTrees(path);

        bdts.Execute();

        getDecisionTreesStrategy.Verify();
    }

    [Fact]
    public void NegativeBuildingDecisionTreesTestThrowsException()
    {
        var path = "";
        var getDecisionTreesStrategy = new Mock<IStrategy>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.GetDecisionTrees", (object[] args) => getDecisionTreesStrategy.Object.Strategy(args)).Execute();
        getDecisionTreesStrategy.Setup(t => t.Strategy(It.IsAny<object[]>())).Returns(new Dictionary<int, object>()).Verifiable();

        var bdts = new BuildingDecisionTrees(path);

        Assert.Throws<Exception>(() => bdts.Execute());

        getDecisionTreesStrategy.Verify();
    }

    [Fact]
    public void NegativeBuildingDecisionTreesTestThrowsFileNotFoundException()
    {
        var path = "./DT_File.txt";
        var getDecisionTreesStrategy = new Mock<IStrategy>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "SpaceBattle.GetDecisionTrees", (object[] args) => getDecisionTreesStrategy.Object.Strategy(args)).Execute();
        getDecisionTreesStrategy.Setup(t => t.Strategy(It.IsAny<object[]>())).Returns(new Dictionary<int, object>()).Verifiable();

        var bdts = new BuildingDecisionTrees(path);

        Assert.Throws<FileNotFoundException>(() => bdts.Execute());

        getDecisionTreesStrategy.Verify();
    }
}