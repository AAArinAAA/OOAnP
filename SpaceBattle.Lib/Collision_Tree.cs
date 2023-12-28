using Hwdtech;

namespace SpaceBattle.Lib;

public interface ITreeBuilder
{
    public void Build(string path);
}

public class TreeBuilder : ITreeBuilder
{
    private static IEnumerable<IEnumerable<int>> ReadFileData(string path)
    {
        return File.ReadAllLines(path).Select(line => line.Split().Select(int.Parse));
    }

    public void Build(string path)
    {
        ReadFileData(path).ToList().ForEach(vector =>
        {
            var prefixTrie = IoC.Resolve<IDictionary<int, object>>("Game.CollisionTree");

            vector.ToList().ForEach(feature =>
            {
                prefixTrie.TryAdd(feature, new Dictionary<int, object>());
                prefixTrie = (IDictionary<int, object>)prefixTrie[feature];
            });
        });
    }
}
