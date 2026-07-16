using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Settings;

public partial class SettingsContainer : HSplitContainer
{
    private List<VBoxContainer> Columns = [];

    public SettingsContainer AddSetting(SettingType type, string name, string saveId = "", object? def = null,
        int columnIndex = 0, Action<Node[]> extraConfig = null)
    {
        var column = this[columnIndex];
        switch (type)
        {
            case SettingType.CheckBox:
                CheckBox checkBox = new();
                checkBox.Text = name;
                extraConfig?.Invoke([checkBox]);
                checkBox.ButtonPressed = SaveType<bool>.Load(saveId, def is not null && (bool)def);
                checkBox.Toggled += b => SaveType<bool>.Save(saveId, b, true);  
                column.AddChild(checkBox);
                break;
            case SettingType.BrowsFile:
                Button button = new();
                button.Text = name;

                FileDialog fileDialog = new();
                fileDialog.Visible = false;
                fileDialog.Access = FileDialog.AccessEnum.Filesystem;
                fileDialog.ShowHiddenFiles = true;
                fileDialog.UseNativeDialog = true;
                extraConfig?.Invoke([fileDialog, button]);

                button.Pressed += fileDialog.Show;

                column.AddChild(button);
                column.AddChild(fileDialog);
                break;
            case SettingType.ButtonAction:
                Button buttonAction = new();
                buttonAction.Text = name;
                extraConfig?.Invoke([buttonAction]);
                column.AddChild(buttonAction);
                break;
            case SettingType.SpinNumber:
                SpinBox box = new();
                extraConfig?.Invoke([box]);

                box.Value = SaveType<double>.Load(saveId, def is null ? box.MinValue : (double)def);
                box.ValueChanged += v => SaveType<double>.Save(saveId, v, true);

                column.AddChild(CreateBoxWithLabel(box, name, true));
                break;

            case SettingType.Input_Submitted or SettingType.Input_TextChange:
                LineEdit edit = new();
                edit.ExpandToTextLength = true;
                extraConfig?.Invoke([edit]);

                edit.Text = SaveType<string>.Load(saveId, (string)def ?? "");
                if (type is SettingType.Input_Submitted)
                    edit.TextSubmitted += s => SaveType<string>.Save(saveId, s, true);
                else edit.TextChanged += s => SaveType<string>.Save(saveId, s, true);

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
                extraConfig?.Invoke([colorPicker]);

                colorPicker.Color = colorConstant.Color();
                colorPicker.PopupClosed += () => colorConstant.Save(colorPicker.Color);

                SaveType<HexColor>.OnSaveEvent += (id, color) =>
                {
                    if (id != ColorIdConstants.ConstantToId[colorConstant]) return;
                    colorPicker.Color = color;
                };

                column.AddChild(CreateBoxWithLabel(colorPicker, name, true));
                break;
            case SettingType.Text:
                Label label = new();
                label.Text = name;
                column.AddChild(label);
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
    
    public SettingsContainer AddText(string text, int columnIndex = 0)
    {
        return AddSetting(SettingType.Text, text, columnIndex:columnIndex);
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

public enum SettingType
{
    HexColor, Input_Submitted, Input_TextChange,
    SpinNumber, BrowsFile, ButtonAction,
    Text, CheckBox
}