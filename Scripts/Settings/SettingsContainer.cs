using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Settings;

public partial class SettingsContainer : HSplitContainer
{
    public UISaver Saver;
    private List<VBoxContainer> Columns = [];

    public SettingsContainer AddSeparator(int col, bool isHorizontal = true) => AddSeparator(isHorizontal, col);

    public SettingsContainer AddSeparator(bool isHorizontal = true, int col = 0)
    {
        var column = this[col];
        column.AddChild(isHorizontal ? new HSeparator() : new VSeparator());
        return this;
    }

    public SettingsContainer AddText(string text, int col = 0)
    {
        Label label = new();
        label.Text = text;
        this[col].AddChild(label);
        return this;
    }

    public SettingsContainer AddCheckBox(string text, string saveId, bool def = false, int col = 0,
        Action<CheckBox>? extraConfig = null)
    {
        CheckBox checkBox = new();
        checkBox.Text = text;
        extraConfig?.Invoke(checkBox);
        Saver.BuildSavable(checkBox, saveId, def);
        this[col].AddChild(checkBox);
        return this;
    }

    public SettingsContainer AddColorChanger(string saveId, string text, int col = 0,
        Action<ColorPickerButton>? extraConfig = null)
    {
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
        
        SaveType<HexColor>.OnSaveEvent += (id, color) =>
        {
            if (id != ColorIdConstants.ConstantToId[colorConstant]) return;
            colorPicker.Color = color;
        };
        
        this[col].AddChild(CreateBoxWithLabel(colorPicker, text, true));
        return this;
    }

    public SettingsContainer AddLineEdit(string text, string saveId, bool useInputSubmit = true, string def = "",
        int columnIndex = 0, Action<LineEdit>? extraConfig = null)
    {
        LineEdit edit = new();
        edit.ExpandToTextLength = true;
        extraConfig?.Invoke(edit);

        edit.Text = SaveType<string>.Load(saveId, def);
        if (useInputSubmit) edit.TextSubmitted += s => SaveType<string>.Save(saveId, s, true);
        else edit.TextChanged += s => SaveType<string>.Save(saveId, s, true);

        this[columnIndex].AddChild(CreateBoxWithLabel(edit, text, false));
        return this;
    }

    public SettingsContainer AddSpinBox(string text, string saveId, double def = 0, int col = 0,
        Action<SpinBox>? extraConfig = null)
    {
        SpinBox box = new();
        extraConfig?.Invoke(box);
        Saver.BuildSavable(box, saveId, def);
        this[col].AddChild(CreateBoxWithLabel(box, text, true));
        return this;
    }

    public SettingsContainer AddButton(string text, Action clicked, int col = 0)
    {
        Button buttonAction = new();
        buttonAction.Text = text;
        buttonAction.Pressed += clicked;
        this[col].AddChild(buttonAction);
        return this;
    }

    public SettingsContainer AddBrowseFile(string text, FileDialog.FileModeEnum mode, string[] fileExt,
        string fileTarget = "", int col = 0, Action<Button, FileDialog>? extraConfig = null)
    {
        Button button = new();
        button.Text = text;

        FileDialog fileDialog = new();
        extraConfig?.Invoke(button, fileDialog);
        fileDialog.Visible = false;
        fileDialog.Access = FileDialog.AccessEnum.Filesystem;
        fileDialog.ShowHiddenFiles = true;
        fileDialog.UseNativeDialog = true;
        fileDialog.FileNameFilter = fileTarget;
        fileDialog.Filters = fileExt;
        fileDialog.FileMode = mode;

        button.Pressed += fileDialog.Show;

        this[col].AddChild(button);
        this[col].AddChild(fileDialog);
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