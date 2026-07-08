using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class UISaver : Node
{
    [Export] private string Prefix;
    [Export] private Godot.Collections.Dictionary<string, Control> Controls = [];

    [Signal] public delegate void LoadedControlsEventHandler();

    private Dictionary<NodePath, string> ControlPaths = [];

    public override void _Ready()
    {
        Controllers.MainController.OnEarlySave += SaveControls;
        ControlPaths = Controls.ToDictionary(kv => kv.Value.GetPath(), kv => kv.Key);
        LoadControls();
        EmitSignalLoadedControls();
    }

    public void SaveControl(string rawId, Control control, bool broadcast)
    {
        var id = GetId(rawId);
        switch (control)
        {
            case FoldableContainer fc: SaveType<bool>.Save(id, fc.Folded, broadcast); break;
            case SplitContainer sc: SaveType<int>.Save(id, sc.SplitOffsets[0], broadcast); break;
            case TabContainer tc:
            {
                if (tc.GetTabCount() > 0) SaveType<int>.Save(id, tc.CurrentTab, broadcast);
                break;
            }
            case SpinBox sb: SaveType<double>.Save(id, sb.Value, broadcast); break;
            case CheckButton cb: SaveType<bool>.Save(id, cb.ButtonPressed, broadcast); break;
            case OptionButton ob: SaveType<int>.Save(id, ob.Selected, broadcast); break;
            default:
                GD.Print($"{control.GetType()} is not configured to save in UiSaver (can ignore if not dev)"); break;
        }
    }
    
    public void SaveControls()
    {
        foreach (var (rawId, control) in Controls) SaveControl(rawId, control, false);
    }

    public void BroadcastSaveControl(Control control)
    {
        if (!ControlPaths.TryGetValue(control.GetPath(), out var id)) return;
        SaveControl(id, control, true);
    }

    public void LoadControl(string rawId, Control control)
    {
        var id = GetId(rawId);
        switch (control)
        {
            case FoldableContainer fc: fc.Folded = SaveType<bool>.Load(id, fc.Folded); break;
            case SplitContainer sc: sc.SplitOffsets = [SaveType<int>.Load(id, sc.SplitOffsets[0])]; break;
            case TabContainer tc:
            {
                if (tc.GetTabCount() > 0)
                    tc.CurrentTab = Math.Clamp(SaveType<int>.Load(id, 0), 0, tc.GetTabCount() - 1);
                break;
            }
            case SpinBox sb: sb.Value = SaveType<double>.Load(id, sb.Value); break;
            case CheckButton cb: cb.ButtonPressed = SaveType<bool>.Load(id, cb.ButtonPressed); break;
            case OptionButton ob: ob.Selected = SaveType<int>.Load(id, ob.Selected); break;
            default:
                GD.Print($"{control.GetType()} is not configured to save in UiSaver (can ignore if not dev)"); break;
        }
    }
    
    public void LoadControls()
    {
        foreach (var (rawId, control) in Controls) LoadControl(rawId, control);
    }
    
    public string GetId(string id) => $"{Prefix}/{id}";
}