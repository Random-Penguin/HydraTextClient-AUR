using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Mapper;

public partial class MapLocation : TextureRect
{
    public static Dictionary<string, Texture2D> TextureCache = [];

    [Export] public Texture2D BaseCheckImage;
    public Vector2? SetSize;
    public List<string> Locations;
    public bool QueueUpdate;
    public MapLoader Loader;
    public int MapId;
    public int NodeId;
    public bool HasCustomImage;

    [Signal] public delegate void OnSelectedEventHandler();

    [Signal] public delegate void OnUnSelectedEventHandler();

    [Signal] public delegate void OnEnteredEventHandler();

    [Signal] public delegate void OnExitedEventHandler();
    [Signal] public delegate void OnUnSelectHighlighterEventHandler();

    public void SetImage(int mapId, int nodeId, string path, string image, Vector2 size, MapLoader loader)
    {
        MapId = mapId;
        NodeId = nodeId;
        Loader = loader;
        SetSize = size;
        QueueUpdate = true;
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
        Size = SetSize!.Value;
        SetSize = null;
    }

    // 0: in logic (hinted) <- 1: in logic <- 2: not logic (hinted) <- 3: not in logic <- 4: nothing, queue delete
    private void LocationUpdate()
    {
        QueueUpdate = false;
        var applicableHints = Loader.Client.Hints
                                    .Where(hint => hint.FindingPlayer == Loader.Client.PlayerSlot && !hint.Found
                                         && hint.Status is HintStatus.Priority
                                     )
                                    .Select(hint => hint.LocationName)
                                    .ToArray();

        var color = 4;
        foreach (var loc in Locations.ToArray())
        {
            if (!Loader.Client.MissingLocations.Contains(loc))
            {
                Locations.Remove(loc);
                continue;
            }

            var locColor = 3;
            if (Loader.Page.LocationNamesInLogic.Contains(loc)) locColor = 1;
            if (applicableHints.Contains(loc)) locColor -= 1;
            color = Math.Min(color, locColor);
        }

        switch (color)
        {
            case 0: SelfModulate = ColorIdConstants.ColorConstant.InLogicHinted.Color(); break;
            case 1: SelfModulate = ColorIdConstants.ColorConstant.InLogic.Color(); break;
            case 2: SelfModulate = ColorIdConstants.ColorConstant.NotInLogicHinted.Color(); break;
            case 3: SelfModulate = ColorIdConstants.ColorConstant.NotInLogic.Color(); break;
            case 4: Loader.RemoveNode(MapId, NodeId); break;
        }
    }

    public void EmitUnSelect() => EmitSignalOnUnSelectHighlighter();
    public void EmitSelected() => EmitSignalOnSelected();
    public void EmitUnSelected() => EmitSignalOnUnSelected();
    public void EmitOnEntered() => EmitSignalOnEntered();
    public void EmitOnExited() => EmitSignalOnExited();
}