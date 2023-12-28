using Hwdtech;

namespace SpaceBattle.Lib;

public class BuildCollisionTreeCommand : ICommand
{
    private readonly string _path;

    public BuildCollisionTreeCommand(string path)
    {
        _path = path;
    }

    public void Execute()
    {
        IoC.Resolve<ITreeBuilder>("Game.CollisionTree.Build").Build(_path);
    }
}
