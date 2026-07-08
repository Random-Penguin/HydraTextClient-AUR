using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Settings;

public partial class SettingsContainer : HSplitContainer
{
    private List<VBoxContainer> Columns = [];

    public SettingsContainer AddSetting(SettingType type, string name, string saveId = "", object? def = null,
        int columnIndex = 0, Action<Control> extraConfig = null)
    {
        var column = this[columnIndex];
        switch (type)
        {
            case SettingType.SpinNumber:
                SpinBox box = new();
                extraConfig?.Invoke(box);

                box.Value = SaveType<double>.Load(saveId, def is null ? 0d : (double)def);
                box.ValueChanged += v => SaveType<double>.Save(saveId, v, true);

                column.AddChild(CreateBoxWithLabel(box, name, true));
                break;

            case SettingType.Input:
                LineEdit edit = new();
                edit.ExpandToTextLength = true;
                extraConfig?.Invoke(edit);

                edit.Text = SaveType<string>.Load(saveId, (string)def ?? "");
                edit.TextSubmitted += s => SaveType<string>.Save(saveId, s, true);

                column.AddChild(CreateBoxWithLabel(edit, name, false));
                break;

            case SettingType.HexColor:
                if (!ColorIdConstants.IdToConstant.TryGetValue(saveId, out var colorConstant))
                {
                    GD.PushWarning($"Save Id [{saveId}] has no color constant");
                    return this;
                }

                ColorPickerButton colorPicker = new();
                colorPicker.Text = "Color Picker Setting";
                extraConfig?.Invoke(colorPicker);

                colorPicker.Color = colorConstant.Color();
                colorPicker.PopupClosed += () => colorConstant.Save(colorPicker.Color);

                column.AddChild(CreateBoxWithLabel(colorPicker, name, true));
                break;
        }
        return this;
    }

    public SettingsContainer AddSeparator(bool isHorizontal = true, int columnIndex = 0)
    {
        var column = this[columnIndex];
        column.AddChild(isHorizontal ? new HSeparator() : new VSeparator());
        return this;
    }

    public BoxContainer CreateBoxWithLabel(Control obj, string text, bool isHorizontal)
    {
        BoxContainer container = isHorizontal ? new HBoxContainer() : new VBoxContainer();
        container.SizeFlagsHorizontal = SizeFlags.Expand;

        Label label = new();
        label.Text = isHorizontal ? $": {text}" : $"{text}:";

        if (!isHorizontal) container.AddChild(label);
        container.AddChild(obj);
        if (isHorizontal) container.AddChild(label);
        return container;
    }

    public VBoxContainer this[int columnIndex]
    {
        get
        {
            while (Columns.Count <= columnIndex)
            {
                ScrollContainer scroll = new();
                scroll.SetAnchorsPreset(LayoutPreset.FullRect);
                scroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;

                MarginContainer margin = new();
                margin.SetAnchorsPreset(LayoutPreset.FullRect);

                VBoxContainer column = new();
                column.SetAnchorsPreset(LayoutPreset.FullRect);
                column.SetHSizeFlags(SizeFlags.ExpandFill);
                column.SetVSizeFlags(SizeFlags.ExpandFill);

                margin.AddChild(column);
                scroll.AddChild(margin);
                AddChild(scroll);
                Columns.Add(column);
            }

            return Columns[columnIndex];
        }
    }
}

public enum SettingType { HexColor, Input, SpinNumber }