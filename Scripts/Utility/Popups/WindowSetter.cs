using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class WindowSetter : Window
{
    [Export] private bool DisableMin = true;
    [Export] private bool DisableMax = false;
    [Export] private bool BlockParent = true;
    [Export] private bool OnTop = false;
    [Export] private bool ToQueueFree = true;
    [Export] private bool ClickAnywhereToClose = false;
    
    public WindowSetter()
    {
        PopupWindow = ClickAnywhereToClose;
        AlwaysOnTop = OnTop;
        Visible = false;
        WrapControls = true;
        Transient = true;
        Exclusive = BlockParent;
        MinimizeDisabled = DisableMin;
        MaximizeDisabled = DisableMax;
        ForceNative = true;
        Transparent = true;
        InitialPosition = WindowInitialPosition.CenterMainWindowScreen;
        CloseRequested += Close;
        Theme = MainController.GlobalTheme;
    }

    public void Close()
    {
        CallDeferred("hide");
        if (!ToQueueFree) return;
        GetParent().CallDeferred("remove_child", this);
        CallDeferred("queue_free");
    }
}