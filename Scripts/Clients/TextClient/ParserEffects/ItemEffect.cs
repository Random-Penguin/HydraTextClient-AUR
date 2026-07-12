using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

// {{item;game name;item name}}
// {{item;game name;item name;item flags}}
namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class ItemEffect : MessageParserEffect
{
    public const string SaveId = "Clients/TextClient/TextEffects/ItemMessageEffect";
    public const string Default = "[{{img}}{{name}}]";

    private static Dictionary<string, IPrintableObj[]> CustomAssetsItemCache = [];

    public override string Key => "item";

    public override void Effect(RichTextLabel label, string[] args, Action? reloadFunction = null)
    {
        if (args.Length < 2 || reloadFunction is null)
        {
            label.AddText("[Invalid Item Tag]");
            return;
        }

        var id = $"{args[0]};{args[1]}";
        if (!CustomAssetsItemCache.TryGetValue(id, out var value))
        {
            CustomAssetsItemCache[id] = value = SaveType<string>.Load(SaveId, Default).CompileRichText(
                new Dictionary<string, Action<RichTextLabel, string[]>>
                {
                    ["img"] = (l, _) => l.AddImage(
                        CustomAssets.ItemImage(args[0], args[1], args[0], _ => reloadFunction()), 0, 20
                    ),
                    ["name"] = (l, _) => l.AppendText(args[1]),
                }, false
            );
        }

        label.PushContext();
        if (args.Length > 2)
        {
            var flags = (ItemFlags)int.Parse(args[2]);
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