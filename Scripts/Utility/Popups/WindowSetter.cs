using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class WindowSetter : Window
{
    [Export] public bool DisableMin = true;
    [Export] public bool DisableMax = false;
    [Export] public bool BlockParent = true;
    [Export] public bool OnTop = false;
    [Export] public bool ToQueueFree = true;
    [Export] public bool ClickAnywhereToClose = false;
    [Export] public WindowInitialPosition WindowPosition = WindowInitialPosition.CenterMainWindowScreen;
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
        InitialPosition = WindowPosition;
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