using System.Collections.Concurrent;

namespace SpaceBattle.Lib;

public class CreateAndStartThreadCommand
{
    public static void Execute(params int[] ints)
    {
        var id = ints[0];
        var blq = new BlockingCollection<ICommand>();
        
    }
}
