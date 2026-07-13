using Godot;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class WindowSetter : Window
{
    [Export] private bool DisableMinMax = true;
    [Export] private bool BlockParent = true;
    
    public WindowSetter()
    {
        Visible = false;
        WrapControls = true;
        Transient = true;
        Exclusive = BlockParent;
        MinimizeDisabled = DisableMinMax;
        MaximizeDisabled = DisableMinMax;
        ForceNative = true;
        InitialPosition = WindowInitialPosition.CenterMainWindowScreen;
        CloseRequested += Close;
    }

    public void Close()
    {
        CallDeferred("hide");
        GetParent().CallDeferred("remove_child", this);
        CallDeferred("queue_free");
    }
}