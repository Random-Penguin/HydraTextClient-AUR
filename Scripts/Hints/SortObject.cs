namespace HydraTextClient.Scripts.Hints;

public struct SortObject(string name)
{
    public readonly string Name = name;
    public bool IsDescending;
}