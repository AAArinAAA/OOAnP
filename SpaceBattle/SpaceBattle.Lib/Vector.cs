namespace SpaceBattle.Lib;

public class Vector
{
    public int[] coordinates;
    private readonly int _size;

    public Vector(int a, int b)
    {
        coordinates[0] = a;
        coordinates[1] = b;
        _size = coordinates.Length;
    }

    public static Vector operator +(Vector a, Vector b)
    {
        a.coordinates[0] += b.coordinates[0];
        return a;
    }

     public static Vector operator -(Vector a, Vector b)
    {
        a.coordinates[0] -= b.coordinates[0];

        return a;
    }
    public static bool operator == (Vector a, Vector b)
    {
        var result=true;
        if (a._size != b._size) 
        {
            result = false;
        }

        for (var i = 0; i < a._size; i++)
        {
            if (a.coordinates[i] != b.coordinates[i]) 
            {
            }

            result = true;
        } 

        return result;
    }

    public static bool operator != (Vector a, Vector b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return $"<{string.Join(", ", coordinates)}>";
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (ReferenceEquals(obj, null))
        {
            return false;
        }

        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}
