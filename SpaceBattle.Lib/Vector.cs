namespace SpaceBattle.Lib;

public class Vector
{
    private readonly int[] _coordinates;
    public int Size => _coordinates.Length;

    public Vector(params int[] coordinates)
    {
        if (coordinates.Length == 0)
        {
            throw new ArgumentException("not good, bro...");
        }

        _coordinates = coordinates;
    }

    public static Vector operator +(Vector a, Vector b)
    {
        a._coordinates[0] += b._coordinates[0];
        a._coordinates[1] += b._coordinates[1];
        return a;
    }

    public static Vector operator -(Vector a, Vector b)
    {
        a._coordinates[0] -= b._coordinates[0];
        a._coordinates[1] -= b._coordinates[1];

        return a;
    }
    public static bool operator ==(Vector a, Vector b)
    {
        var result = true;
        if (a.Size != b.Size)
        {
            result = false;
        }

        for (var i = 0; i < a.Size; i++)
        {
            if (a._coordinates[i] == b._coordinates[i])
            {
                result = true;
            }
        }

        return result;
    }

    public static bool operator !=(Vector a, Vector b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector vector && _coordinates.SequenceEqual(vector._coordinates);
    }
}
