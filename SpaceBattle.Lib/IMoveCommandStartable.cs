namespace SpaceBattle.Lib
{
    public interface IMoveCommandStartable
    {
        public IUObject target { get; }
        public Dictionary<string, object> property { get; }
    }
}
