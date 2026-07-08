using System;
using Godot;

namespace HydraTextClient.Scripts.Utility.DataTypes;

public class HexColor
{
    public ulong Color { get => GodotColor.ToRgba64(); set => GodotColor = new Color(value); }
    [NonSerialized] private Color GodotColor = Colors.Olive;

    public HexColor(ulong code) => Color = code;
    
    public static implicit operator HexColor(ulong code) => new(code);
    public static implicit operator HexColor(Color color) => new(color.ToRgba64());
    public static implicit operator Color(HexColor color) => color.GodotColor;
}