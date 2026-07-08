namespace HydraTextClient.Scripts.Utility.DataTypes;

public struct Circle(int num, ulong[] locations)
{
    public int CircleNumber = num;
    public ulong[] Locations = locations;
}