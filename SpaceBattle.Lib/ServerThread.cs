
using Hwdtech;

namespace SpaceBattle.Lib;
public class ServerThread
{
    public void runServer()
    {
        IoC.Resolve<ICommand>("CreateAndStartThreadCommand", 1);
    }
   
    // запуск энпоинта
    // определение кол-ва потоков

}
