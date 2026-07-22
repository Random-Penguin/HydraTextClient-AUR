using System;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

// {{e;emote name}}
namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class EmoteEffect : MessageParserEffect
{
    public override string Key => "e";
    
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length != 1)
        {
            label.AddText("[Invalid Emote Tag]");
            return;
        }
        
        if (!EmoteLoader.Singleton.TryGet(args[0], out var img))
        {
            label.AddText($"[{args[0]}]");
            return;
        }

        label.PushContext();
        label.PushHint($"emote {args[0]}");
        label.AddImage(img, 20, 20);
        label.PopContext();
    }
}