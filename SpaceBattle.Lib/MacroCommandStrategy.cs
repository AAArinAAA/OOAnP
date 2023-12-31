using Hwdtech;

namespace SpaceBattle.Lib;

public interface IStrategy
{
    public object Init(params object[] args);
}

public class MacroCommandStrategy : IStrategy
{
    public object Init(params object[] args)
    {
        var nameOperation = (string)args[0];
        var obj = (IUObject)args[1];

        var dependencies = IoC.Resolve<IList<string>>("Component" + nameOperation);
        IList<ICommand> list = new List<ICommand>();

        foreach (var d in dependencies)
        {
            list.Add(IoC.Resolve<ICommand>(d, obj));
        }

        return new MacroCommand(list);
    }
}
