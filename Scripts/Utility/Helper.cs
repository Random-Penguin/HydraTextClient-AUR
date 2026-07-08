using Archipelago.MultiClient.Net.Enums;
using Godot;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Utility;

public static class Helper
{
    public static void SetFontSize(this Control control, int fontSize, string name = "font_size")
    {
        if (control.HasThemeFontSizeOverride(name)) control.RemoveThemeFontSizeOverride(name);
        control.AddThemeFontSizeOverride(name, fontSize);
    }

    public static void Increment(this SpinBox spinBox, double step = 0)
        => spinBox.SetValue(spinBox.Value + (step is 0 ? spinBox.Step : step));

    public static void Decrement(this SpinBox spinBox, double step = 0)
        => spinBox.Increment(-(step is 0 ? spinBox.Step : step));

    public static void AppendText(this LineEdit edit, string text) => edit.Text += text;

    public static Color GetColorFromItemFlag(this ItemFlags itemFlags)
    {
        if (itemFlags.HasFlag(ItemFlags.Advancement)) return ProgressiveItemColor.Color();
        if (itemFlags.HasFlag(ItemFlags.NeverExclude)) return UsefulItemColor.Color();
        if (itemFlags.HasFlag(ItemFlags.Trap)) return TrapItemColor.Color();
        return NormalItemColor.Color();
    }
}