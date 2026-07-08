using Godot;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class LineEditHide : LineEdit
{
    [Export] private bool NumberOnly;
    [Export] private CheckButton Hide;
    private string LastText = "";

    public override void _Ready()
    {
        if (Hide is not null)
        {
            TogglePassword(Hide.ButtonPressed);
        }
        
        if (!NumberOnly) return;
        TextChanged += s =>
        {
            if (!NumberOnly) return;
            if (s.Trim() == "" || s.IsValidInt())
            {
                LastText = $"{(int.TryParse(s.Trim(), out var port) ? port : 12345)}";
                return;
            }

            Text = LastText;
        };
    }

    public void TogglePassword(bool toggle) => Secret = toggle;
}