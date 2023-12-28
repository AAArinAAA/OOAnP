using Hwdtech;

namespace SpaceBattle.Lib
{
    public class StartMoveCommand : ICommand
    {
        private readonly IMoveCommandStartable moveCommandStartable;
        public StartMoveCommand(IMoveCommandStartable movableCommand)
        {
            moveCommandStartable = movableCommand;
        }
        public void Execute()
        {
            moveCommandStartable.property.ToList().ForEach(p => moveCommandStartable.target.SetProperty(p.Key, p.Value));
            var command = IoC.Resolve<ICommand>("Game.Operation.Move", moveCommandStartable.target);
            var injectable = IoC.Resolve<ICommand>("Commands.Injectable", command);
            moveCommandStartable.target.SetProperty("command", command);
            IoC.Resolve<IQueue>("Game.Queue", injectable).Enqueue(command);
        }
    }
}
