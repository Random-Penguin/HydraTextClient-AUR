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
    private bool Added;

    [Signal] public delegate void CloseCalledEventHandler();

    public WindowSetter() => Visible = false;

    public override void _EnterTree()
    {
        PopupWindow = ClickAnywhereToClose;
        AlwaysOnTop = OnTop;
        Visible = false;
        WrapControls = true;
        Transient = !OnTop;
        Exclusive = BlockParent;
        MinimizeDisabled = DisableMin;
        MaximizeDisabled = DisableMax;
        ForceNative = true;
        Transparent = true;
        InitialPosition = WindowInitialPosition.CenterMainWindowScreen;
        Theme = MainController.GlobalTheme;
        if (Added) return;
        CloseRequested += Close;
        Added = true;
    }

    public void Close()
    {
        CallDeferred("hide");
        EmitSignalCloseCalled();
        if (!ToQueueFree) return;
        GetParent().CallDeferred("remove_child", this);
        CallDeferred("queue_free");
    }
}