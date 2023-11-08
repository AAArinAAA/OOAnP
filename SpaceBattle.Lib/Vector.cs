﻿namespace SpaceBattle.Lib;

public class Vector
{
    private readonly int[] _coordinates;
    public int Size => _coordinates.Length;

    public Vector(params int[] coordinates)
    {
        if (coordinates.Length == 0)
        {
            throw new ArgumentException();
        }

        _coordinates = coordinates;
    }

    public static Vector operator +(Vector a, Vector b)
    {
        if (a.Size != b.Size)
        {
            throw new System.ArgumentException();
        }

        var i = 0;
        var size = a.Size;
        while (i < size)
        {
            a._coordinates[i] += b._coordinates[i];
            i++;
        }

        return a;
    }

    public static Vector operator -(Vector a, Vector b)
    {
        if (a.Size != b.Size)
        {
            throw new System.ArgumentException();
        }

        var i = 0;
        var size = a.Size;
        while (i < size)
        {
            a._coordinates[i] -= b._coordinates[i];
            i++;
        }

        return a;
    }
    public static bool operator ==(Vector a, Vector b)
    {
        if (a.Size != b.Size)
        {
            throw new System.ArgumentException();
        }

        return b.Equals(a);
    }

    public static bool operator !=(Vector a, Vector b)
    {
        if (a.Size != b.Size)
        {
            throw new System.ArgumentException();
        }

        return !(a == b);
    }

    public override int GetHashCode()
    {
        return 0;
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector vector && _coordinates.SequenceEqual(vector._coordinates);
    }
}
