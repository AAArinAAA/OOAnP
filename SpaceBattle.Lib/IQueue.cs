namespace SpaceBattle.Lib
{
    public interface IQueue
    {
        public object Enqueue(object[] args);
        public object Dequeue();
    }
}