using System;

namespace HydraTextClient.Scripts.Hints;

public struct SortObject(string name) : IEquatable<SortObject>
{
    public readonly string Name = name;
    public bool IsDescending;

    public bool Equals(SortObject other) => Name == other.Name && IsDescending == other.IsDescending;
    public override bool Equals(object obj) => obj is SortObject other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Name, IsDescending);
}