using System.Collections;
using Hwdtech;

namespace SpaceBattle.Lib;

public class CheckCollisionCommand : ICommand
{
    private readonly IUObject _obj1;
    private readonly IUObject _obj2;

    public CheckCollisionCommand(IUObject obj1, IUObject obj2)
    {
        _obj1 = obj1;
        _obj2 = obj2;
    }

    public void Execute()
    {
        var pos1 = IoC.Resolve<Vector>("Game.Object.GetProperty", _obj1, "Position");
        var vel1 = IoC.Resolve<Vector>("Game.Object.GetProperty", _obj1, "Velocity");
        var pos2 = IoC.Resolve<Vector>("Game.Object.GetProperty", _obj2, "Position");
        var vel2 = IoC.Resolve<Vector>("Game.Object.GetProperty", _obj2, "Velocity");

        var newCoord = new int[4];
        newCoord[0] = pos2._coordinates[0] - pos1._coordinates[0];
        newCoord[1] = pos2._coordinates[1] - pos1._coordinates[1];
        newCoord[2] = vel2._coordinates[0] - vel1._coordinates[0];
        newCoord[3] = vel2._coordinates[1] - pos1._coordinates[1];

        var tree = IoC.Resolve<Hashtable>("Game.Collision.GetTree");

        newCoord.ToList().ForEach(c => tree = (Hashtable)tree.GetValueOrThrowException(c));

        IoC.Resolve<object>("Game.Collision.Process", tree);
    }
}
