using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

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

        var id = string.Join(";", args);

        var hasItemFlags = args.Length > 2;

        if (!CustomAssetsItemCache.TryGetValue(id, out var value))
        {
            CustomAssetsItemCache[id] = value = SaveType<string>.Load(SaveId, Default).CompileRichText(
                new Dictionary<string, Action<RichTextLabel, string[]>>
                {
                    ["img"] = (l, _) => l.AddImage(
                        CustomAssets.ItemImage(args[0], args[1], args[1], reloadFunction), 0, 20
                    ),
                    ["name"] = (l, _) => l.AppendText(args[1]),
                }, false
            );
        }

        label.PushContext();
        if (hasItemFlags) label.PushColor(((ItemFlags)int.Parse(args[2])).GetColorFromItemFlag());
        label.ApplyCompiledPrintableObjs(value);
        label.PopContext();
    }
}