using System;
using Godot;
using Godot.Collections;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class UISaver : Node
{
    [Export] private string Prefix;
    [Export] private Dictionary<string, Node> Controls = [];

    public override void _Ready()
    {
        foreach (var (rawId, control) in Controls) BuildSavable(control, rawId);
    }

    public void BuildSavable(Node node, string rawId)
    {
        switch (node)
        {
            case FoldableContainer fc: BuildSavable(fc, rawId, fc.Folded); break;
            case SplitContainer sc: BuildSavable(sc, rawId, sc.SplitOffsets ?? []); break;
            case TabContainer tc: BuildSavable(tc, rawId, tc.GetTabCount()); break;
            case SpinBox sb: BuildSavable(sb, rawId, sb.Value); break;
            case CheckButton cb: BuildSavable(cb, rawId, cb.ButtonPressed); break;
            case OptionButton ob: BuildSavable(ob, rawId, ob.Selected); break;
            case CheckBox cb: BuildSavable(cb, rawId, cb.ButtonPressed); break;
            case Window win: BuildSavable(win, rawId, Vector2I.Zero); break;
            default: GD.Print($"{node.GetType()} is not configured UiSaver (can ignore if not dev)"); break;
        }
    }

    public void BuildSavable(Node node, string rawId, object? def)
    {
        if (def is null) BuildSavable(node, rawId); // warning: CAN cause null ref if above build is incorrect
        var id = $"{Prefix}/{rawId}";
        switch (node)
        {
            case FoldableContainer fc:
                fc.Folded = SaveType<bool>.Load(id, (bool)def);
                fc.FoldingChanged += b => SaveType<bool>.Save(id, b, true);
                break;
            case SplitContainer sc:
                sc.SplitOffsets = SaveType<int[]>.Load(id, (int[])def);
                sc.Dragged += _ => SaveType<int[]>.Save(id, sc.SplitOffsets, true);
                break;
            case TabContainer tc:
                if (tc.GetTabCount() > 0)
                    tc.CurrentTab = Math.Clamp(SaveType<int>.Load(id, 0), 0, (int)def - 1);
                tc.TabChanged += _ => SaveType<int>.Save(id, tc.CurrentTab, true);
                break;
            case SpinBox sb:
                sb.Value = SaveType<double>.Load(id, (double)def);
                sb.ValueChanged += d => SaveType<double>.Save(id, d, true);
                break;
            case CheckButton cb:
                cb.ButtonPressed = SaveType<bool>.Load(id, (bool)def);
                cb.Pressed += () => SaveType<bool>.Save(id, cb.ButtonPressed, true);
                break;
            case OptionButton ob:
                ob.Selected = SaveType<int>.Load(id, (int)def);
                ob.ItemSelected += _ => SaveType<int>.Save(id, ob.Selected, true);
                break;
            case CheckBox cb:
                cb.ButtonPressed = SaveType<bool>.Load(id, (bool)def);
                cb.Pressed += () => SaveType<bool>.Save(id, cb.ButtonPressed, true);
                break;
            case Window win:
                win.Position = SaveType<Vector2I>.Load($"{id}_pos", win.GetPosition());
                win.Size = SaveType<Vector2I>.Load($"{id}_size", win.GetSize());
                win.TreeExiting += () => SaveType<Vector2I>.Save($"{id}_pos", win.Position, true);
                win.SizeChanged += () => SaveType<Vector2I>.Save($"{id}_size", win.Size, true);
                break;
            default: GD.Print($"{node.GetType()} is not configured UiSaver (can ignore if not dev)"); break;
        }
    }
}