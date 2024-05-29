namespace SpaceBattle.Lib;

public class SetFuelStrategy : IStrategyRenamed
{
    public object Strategy(params object[] args)
    {
        var patient = (IUObject)args[0];
        return new SetFuelCommand(patient);
    }
}
