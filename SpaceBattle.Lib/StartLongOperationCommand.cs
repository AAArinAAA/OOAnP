using Hwdtech;

namespace SpaceBattle.Lib
{
    public interface IMoveCommandStart : ICommand
    {
        Lib.IUObject Obj { get; }
    }

    public class StartMoveCommand : IMoveCommandStart
    {
        public IUObject Obj {get;}
        public StartMoveCommand(IUObject obj)
        {
            Obj = obj;
        }
        public void Execute()
        {
            var mcContinious = IoC.Resolve<ICommand>("IUObject.IMovable.Continious", Obj);
            IoC.Resolve<ICommand>("IQueue.Push", mcContinious);
        }
    }
}