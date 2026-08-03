using System;
using System.Collections.Generic;
using Godot;

namespace HydraTextClient.Scripts.Mapper;

public partial class MapLocation : TextureRect
{
    public static Dictionary<string, Texture2D> TextureCache = [];

    [Export] public Texture2D BaseCheckImage;
    public Vector2? SetSize;
    public List<LocationCheck> Locations;
    public bool QueueUpdate;
    public MapLoader Loader;

    public void SetImage(string path, string image, Vector2 size, MapLoader loader)
    {
        Loader = loader;
        SetSize = size;
        GD.Print($"to {SetSize} to {size}");
        if (image is "")
        {
            Texture = BaseCheckImage;
            QueueRedraw();
            return;
        }

        // var imagePath = $"{path}/{image}";
        // if (TextureCache.ContainsKey())
    }

    public override void _Process(double delta)
    {
        if (QueueUpdate) LocationUpdate();
        if (SetSize is null) return;
        GD.Print($"Set size: [{SetSize}]");
        Size = SetSize!.Value;
        SetSize = null;
        GD.Print($"size was set: [{Size}]");
    }

    // 0: in logic (hinted) <- 1: in logic <- 2: not logic (hinted) <- 3: not in logic <- 4: nothing, queue delete
    public void LocationUpdate()
    {
        QueueUpdate = false;
        var color = 4;
        foreach (var loc in Locations.ToArray())
        {
            if (!Loader.Client.MissingLocations.Contains(loc.Location))
            {
                Locations.Remove(loc);
                continue;
            }

            color = Math.Min(color, 3);
            
        }

        switch (color)
        {
            case 0: break;
            case 1: break;
            case 2: break;
            case 3: break;
            case 4: Loader.RemoveNode(this); break;
        }
    }
}