namespace SpaceBattle.Lib
{
    public interface IUObject
    {
        public object GetProperty(string name);
        object SetProperty(string v, object velocity);
    }
}
