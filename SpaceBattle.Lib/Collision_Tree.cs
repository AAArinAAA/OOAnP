using Hwdtech;

namespace SpaceBattle.Lib;

public class BuildDecisionTree : ICommand
{
    private readonly IReadList _read;
    public BuildDecisionTree(IReadList read)
    {
        _read = read;
    }
    public void Execute()
    {
        var vectors = _read.ReadFile();

        var BuildTree = IoC.Resolve<Dictionary<int, object>>("Game.BuildDecisionTree");

        vectors.ForEach(line => {
            var DecisionTree = BuildTree;
            line.ToList().ForEach(vector =>
            {
                DecisionTree.TryAdd(vector, new Dictionary<int, object>());
                DecisionTree = (Dictionary<int, object>)DecisionTree[vector];
            });
        });
    }
}