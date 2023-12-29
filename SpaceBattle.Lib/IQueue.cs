namespace SpaceBattle.Lib
{
    public interface IQueue
    {
        void Enqueue(ICommand cmd);
        ICommand Dequeue();
    }
}
