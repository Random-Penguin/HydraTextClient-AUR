using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient;

public static class MessageParser
{
    private static Dictionary<string, MessageParserEffect[]> Effects;

    static MessageParser()
    {
        Effects = TypeLoader.CreateTypesWithAbstractClass<MessageParserEffect>()
                            .GroupBy(effect => effect.Group)
                            .ToDictionary(g => g.Key, g => g.ToArray());

        foreach (var group in Effects.Keys)
        {
            foreach (var effect in Effects[group]) { GD.Print($"effects loaded: [{group}]:[{effect}]"); }
        }
    }

    public static Dictionary<string, Action<RichTextLabel, string[]>> CreateEffects(Action reloadFunction,
        params string[] groups)
    {
        Dictionary<string, Action<RichTextLabel, string[]>> effects = new();

        if (groups.Length == 0) groups = ["default"];

        foreach (var group in groups)
        {
            foreach (var effect in Effects[group])
            {
                effects[effect.Key] = (label, args) => effect.Effect(label, args, reloadFunction);
            }
        }

        return effects;
    }

    public static void ApplyCompiledPrintableObjs(this RichTextLabel label, IPrintableObj[] objs)
    {
        foreach (var printableObj in objs) printableObj.AddText(label);
    }

    public static IPrintableObj[] CompileRichText(this string rawText,
        Dictionary<string, Action<RichTextLabel, string[]>> effects, bool appendRawTextAsBBCode)
    {
        if (!rawText.Contains("{{")) return [new TextPrintObj(rawText, appendRawTextAsBBCode)];
        List<IPrintableObj> objs = [];

        var split = rawText.Split("{{");
        objs.Add(new TextPrintObj(split[0], appendRawTextAsBBCode));
        foreach (var section in split.Skip(1))
        {
            if (!section.Contains("}}"))
            {
                objs.Add(new TextPrintObj($"{{{{{section}", appendRawTextAsBBCode));
                continue;
            }

            var index = section.IndexOf("}}", StringComparison.Ordinal);
            var code = section[..index].Split(';');

            if (code.Length == 0)
            {
                objs.Add(new TextPrintObj($"{{{{{section}", appendRawTextAsBBCode));
                continue;
            }

            var key = code[0];

            if (!effects.ContainsKey(key.ToLower()))
            {
                objs.Add(new TextPrintObj($"{{{{{section}", appendRawTextAsBBCode));
                continue;
            }

            objs.Add(new CallablePrintObj(effects[key], code.Length > 1 ? code[1..] : []));
            if (section.Length <= index + 2) continue;
            objs.Add(new TextPrintObj(section[(index + 2)..], appendRawTextAsBBCode));
        }

        return objs.ToArray();
    }

    public static string CompileSimpleText(this string text, Dictionary<string, string> replacers)
        => replacers.Aggregate(text, (s, kv) => s.Replace($"{{{{{kv.Key}}}}}", kv.Value));

    public static string Sanitize(this string text) => ((string[]) //bbcode blacklist
    [
        "img", "opentype_features", "bgcolor", "hint", "outline_size", "outline_color", "color", "font_size",
        "font", "code", "url",
    ]).Distinct().Aggregate(text, (s, replace) => s.Replace($"[{replace}", $"[lb]{replace}"))
    // .Replace("\r", "")
    ;
}

public abstract class MessageParserEffect
{
    public virtual string Group => "default";
    public abstract string Key { get; }
    public abstract void Effect(RichTextLabel label, string[] args, Action? reloadFunction = null);
}

public readonly struct CallablePrintObj(Action<RichTextLabel, string[]> callable, string[] args) : IPrintableObj
{
    public void AddText(RichTextLabel label) => callable(label, args);
}

public readonly struct TextPrintObj(string text, bool append) : IPrintableObj
{
    public void AddText(RichTextLabel label)
    {
        if (append) label.AppendText(text);
        else label.AddText(text);
    }
}

public interface IPrintableObj
{
    public void AddText(RichTextLabel label);
}