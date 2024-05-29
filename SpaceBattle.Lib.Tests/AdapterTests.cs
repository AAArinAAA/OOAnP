namespace SpaceBattle.Lib.Test;
using Hwdtech;
using Hwdtech.Ioc;

public class AdapterGeneratorTests
{
    public AdapterGeneratorTests()
    {
        new InitScopeBasedIoCImplementationCommand().Execute();
        IoC.Resolve<ICommand>("Scopes.Current.Set", IoC.Resolve<object>("Scopes.New", IoC.Resolve<object>("Scopes.Root"))).Execute();

        IoC.Resolve<ICommand>("IoC.Register", "Game.Reflection.GenerateAdapterCode", (object[] args) => new AdapterCodeGeneratorStrategy().Strategy(args)).Execute();
    }

    [Fact]
    public void AdapterCodeGeneratorTest_1()
    {

        var MoveStartableAdapterCode =
       @"class MoveCommandStartableAdapter : IMoveCommandStartable {
        Object target;
        public MoveCommandStartableAdapter(Object target) => this.target = target; 
        public IUObject target {
               get { return IoC.Resolve<IUObject>(""Game.target.Get"", target); }
        }
        public Dictionary<String, Object> property {
               get { return IoC.Resolve<Dictionary<String, Object>>(""Game.property.Get"", target); }
        }
    }";

        var MovableAdapterCode =
        @"class MovableAdapter : IMovable {
        Vector target;
        public MovableAdapter(Vector target) => this.target = target; 
        public Vector Position {
               get { return IoC.Resolve<Vector>(""Game.Position.Get"", target); }
               set { IoC.Resolve<_ICommand.ICommand>(""Game.Position.Set"", target, value).Execute(); }
        }
        public Vector Velocity {
               get { return IoC.Resolve<Vector>(""Game.Velocity.Get"", target); }
        }
    }";

        Assert.Equal(MoveStartableAdapterCode, IoC.Resolve<string>("Game.Reflection.GenerateAdapterCode", typeof(IMoveCommandStartable), typeof(object)));
        Assert.Equal(MovableAdapterCode, IoC.Resolve<string>("Game.Reflection.GenerateAdapterCode", typeof(IMovable), typeof(Vector)));
    }
}
