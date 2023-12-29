using System.Collections.Generic;
using System.IO;
using Hwdtech;
using Hwdtech.Ioc;

namespace SpaceBattle.Lib.Tests;
using IDict = IDictionary<int, object>;

public class CollisionTreeCommandTest
{
    public CollisionTreeCommandTest()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<Hwdtech.ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        var tree = new Dictionary<int, object>();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.CollisionTree", (object[] args) => tree).Execute();

        var treeBuilder = new TreeBuilder();
        IoC.Resolve<Hwdtech.ICommand>("IoC.Register", "Game.CollisionTree.Build", (object[] args) => treeBuilder).Execute();

    }

    [Xunit.Fact]
    public void SuccessfullyBuildingCollisionTreeFromFileWithSomBranches()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        var path = "../../../test.txt";
        var buildtree = new BuildCollisionTreeCommand(path);

        buildtree.Execute();

        var resultingTree = IoC.Resolve<IDict>("Game.CollisionTree");

        Xunit.Assert.Equal(2, resultingTree.Count);
        Xunit.Assert.Equal(2, ((IDict)resultingTree[1]).Count);
        Xunit.Assert.Equal(2, ((IDict)((IDict)resultingTree[1])[2]).Count);
        Xunit.Assert.Equal(1, ((IDict)((IDict)((IDict)resultingTree[1])[3])[7]).Count);

        Xunit.Assert.True(resultingTree.ContainsKey(1));
        Xunit.Assert.True(((IDict)resultingTree[1]).ContainsKey(2));
        Xunit.Assert.True(((IDict)((IDict)resultingTree[1])[3]).ContainsKey(7));
        Xunit.Assert.True(((IDict)((IDict)((IDict)resultingTree[1])[3])[7]).ContainsKey(5));
    }

    [Xunit.Fact]
    public void IncorrectFilePathInputThrowExceptionWhenBuildingTree()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();

        var build = new BuildCollisionTreeCommand("text.txt");

        Xunit.Assert.Throws<FileNotFoundException>(build.Execute);
    }
}
