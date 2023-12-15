using System.Collections;

namespace SpaceBattle.Lib;

public static class HashTableExp
{
    public static object? GetValueOrThrowException(this Hashtable hashtable, object key)
    {
        return hashtable.ContainsKey(key) ? hashtable[key] : throw new Exception("NotCollision!");
    }
}
