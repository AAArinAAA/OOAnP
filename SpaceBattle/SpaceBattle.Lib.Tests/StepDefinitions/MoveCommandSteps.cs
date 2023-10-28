using System;
using Moq;
using TechTalk.SpecFlow;

namespace SpaceBattle.Lib.Tests;

[Binding]
public class Move
{
    private readonly Mock<IMovable> _movable;

    private Action commandExecutionLambda;

    public Move()
    {
        _movable = new Mock<IMovable>();

        commandExecutionLambda = () => { };

    }

    [Given(@"космический корабль находится в точке пространства с координатами \((.*), (.*)\)")]
    public void ДопустимКосмическийКорабльНаходитсяВТочкеПространстваСКоординатами(int p0, int p1)
    {
        _movable.SetupGet(m => m.Position).Returns(new Vector ( p0, p1 ));
    }
    
    [Given(@"имеет мгновенную скорость \((.*), (.*)\)")]
    public void ДопустимИмеетМгновеннуюСкорость(int p0, int p1)
    {
        _movable.SetupGet(m => m.Velocity).Returns(new Vector ( p0, p1 ));
    }
    
    [Given(@"скорость корабля определить невозможно")]
    public void ДопустимСкоростьКорабляОпределитьНевозможно()
    {
        _movable.SetupGet(m => m.Velocity).Throws(new System.Exception());
    }

    [Given(@"изменить положение в пространстве космического корабля невозможно")]
    public void ДопустимИзменитьПоложениеВПространствеКосмическогоКорабляНевозможно()
    {
        _movable.SetupSet(m => m.Position = It.IsAny<Vector>()).Throws(new System.Exception());
    }

    [Given(@"космический корабль, положение в пространстве которого невозможно определить")]
    public void ДопустимКосмическийКорабльПоложениеВПространствеКоторогоНевозможноОпределить()
    {
        _movable.SetupGet(m => m.Position).Throws<Exception>();
    }
    
    [When(@"происходит прямолинейное равномерное движение без деформации")]
    public void КогдаПроисходитПрямолинейноеРавномерноеДвижениеБезДеформации()
    {
        var mc = new MoveCommand(_movable.Object);
        commandExecutionLambda = () => mc.Execute();
    }
    
    [Then(@"космический корабль перемещается в точку пространства с координатами \((.*), (.*)\)")]
    public void ТоКосмическийКорабльПеремещаетсяВТочкуПространстваСКоординатами(int p0, int p1)
    {
        commandExecutionLambda();
        _movable.VerifySet(_movable => _movable.Position = new Vector(p0, p1), Times.Once);
    }

    [Then(@"возникает ошибка Exception")]
    public void ТоВозникаетОшибкаException()
    {
        Assert.Throws<Exception>(() => commandExecutionLambda());
    }
}
