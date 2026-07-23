using System;
using Godot;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

// {{entrance;entrance text}}
public class EntranceEffect : MessageParserEffect
{
    public static event Action? OnUpdate;
    public override string Key => "entrance";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        var entrance = args.Length == 0 ? "Vanilla" : string.Join(';', args[0]);
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.EntranceColor.Color());
        label.AddText(entrance);
        label.PopContext();
    }

    public override void AddValueUpdater() => SaveType<HexColor>.AddIndividualEvent(
        ColorIdConstants.ColorConstant.EntranceColor.SaveId(), _ => OnUpdate?.Invoke()
    );
}