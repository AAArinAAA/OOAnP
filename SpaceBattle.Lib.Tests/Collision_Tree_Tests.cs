using SpaceBattle.Lib;
using Moq;
using Hwdtech;
using Hwdtech.Ioc;
using System.Collections.Generic;
using Xunit;
using System.IO;
using System.Linq;
using System;

namespace SpaceBattle.Tests;

public class BuildTreeTests
{
    public BuildTreeTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var DecisionTree = new Dictionary<int, object>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.BuildDecisionTree", (object[] args) => {
            return DecisionTree;
        }).Execute();
    }

    [Fact]
    public void BuildDecisionTreeTrue()
    {
        var read = new Mock<IReadList>();

        var path = "../test.txt";
        var vectors = File.ReadAllLines(path).Select(line => line.Split().Select(int.Parse).ToArray()).ToList();
        
        read.Setup(i => i.ReadFile()).Returns(vectors);

        var BuildTree = new BuildDecisionTree(read.Object);
        BuildTree.Execute();

        var DecisionTree = IoC.Resolve<IDictionary<int, object>>("Game.BuildDecisionTree");

        Assert.NotNull(DecisionTree);
        Assert.True(DecisionTree.ContainsKey(1));

        var NextTree = (IDictionary<int, object>)DecisionTree[1];
        Assert.True(NextTree.ContainsKey(2));

        var NextTree2 = (IDictionary<int, object>)NextTree[2];
        Assert.True(NextTree2.ContainsKey(3));

        var NextTree3 = (IDictionary<int, object>)NextTree2[3];
        Assert.True(NextTree3.ContainsKey(10));
    }

    [Fact]
    public void BuildDecisionTreeCantReadFile()
    {
        var read = new Mock<IReadList>();
        read.Setup(i => i.ReadFile()).Returns(() => throw new Exception("This file is not readable"));
        var BuildTree = new Lib.BuildDecisionTree(read.Object);
        
        var exception = Assert.Throws<Exception>(BuildTree.Execute);
        Assert.Equal("This file is not readable", exception.Message);
    }
}