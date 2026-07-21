using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

// {{item;``game name``;``item name``}}
// {{item;``game name``;``item name``;item flags}}
namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class ItemEffect : MessageParserEffect
{
    public const string SaveId = "Clients/TextClient/TextEffects/ItemMessageEffect";
    public const string Default = "[{{img}}{{name}}]";

    private static Dictionary<string, IPrintableObj[]> CustomAssetsItemCache = [];

    public override string Key => "item";

    public override void Effect(RichTextLabel label, string[] args, Action? reloadFunction = null)
    {
        try
        {
            var argList = args.ToList();
            while (!argList[0].EndsWith("``"))
            {
                argList[0] = $"{argList[0]};{argList[1]}";
                argList.RemoveAt(1);
            }
            
            while (!argList[1].EndsWith("``"))
            {
                argList[1] = $"{argList[1]};{argList[2]}";
                argList.RemoveAt(2);
            }
            args = argList.ToArray();
        }
        catch (IndexOutOfRangeException)
        {
            label.AddText("[Invalid Item Tag]");
            return;
        }
        
        if (args.Length < 2 || reloadFunction is null)
        {
            label.AddText("[Invalid Item Tag]");
            return;
        }

        args[0] = args[0][2..^2];
        args[1] = args[1][2..^2];
        var id = $"{args[0]};{args[1]}";
        if (!CustomAssetsItemCache.TryGetValue(id, out var value))
        {
            CustomAssetsItemCache[id] = value = SaveType<string>.Load(SaveId, Default).CompileRichText(
                new Dictionary<string, Action<RichTextLabel, string[]>>
                {
                    ["img"] = (l, _) => l.AddImage(
                        CustomAssets.ItemImage(args[0], args[1], args[0], _ => reloadFunction()), 0, 20
                    ),
                    ["name"] = (l, _) => l.AddText(args[1]),
                }, false
            );
        }

        label.PushContext();
        if (args.Length > 2 && int.TryParse(args[2], out var flagsRaw))
        {
            var flags = (ItemFlags)flagsRaw;
            if ((int)flags == 3) flags = ItemFlags.Advancement;
            var ft = SaveType<FilterType>.Load(FilterType.MakeUID(args[1], args[0], flags), default, false);

            label.PushMeta((string[])["itemfilter", ..args]);
            if (ft.IsSpecial)
            {
                label.PushColor(SpecialItemColor.Color());
                label.PushBgcolor(SpecialItemBackgroundColor.Color());
            }
            else
            {
                label.PushColor(flags.GetColorFromItemFlag());
                label.PushBgcolor(flags.GetBgColorFromItemFlag());
            }
        }
        label.ApplyCompiledPrintableObjs(value);
        label.PopContext();
    }
}