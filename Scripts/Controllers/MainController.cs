using System;
using Godot;
using HydraTextClient.Scripts.Discord;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Controllers;

public partial class MainController : Control
{
    [Export] private PackedScene ErrorWindow;
    [Export] private PackedScene ItemFilterWindow;  

    private ErrorDialog ErrorDialog;

    private static MainController Singleton;

    public static Theme GlobalTheme;

    public static event Action? OnLateSave;

    public override void _Ready()
    {
        DRPC.Init();
        GlobalTheme = Theme;
        GlobalThemeSettings.Init();
        Singleton = this;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationWMCloseRequest) return;
        Save();
    }

    public static void ShowError(string message, Exception e) => ShowError($"{message}\n{e.Message}\n{e.StackTrace}");
    public static void ShowError(Exception e) => ShowError($"{e.Message}\n{e.StackTrace}");
    public static void ShowError(string[] error) => ShowError(string.Join('\n', error));
    public static void ShowError(string error) => Singleton.CallDeferred("CreateErrorDialogue", error);

    public void CreateErrorDialogue(string error)
    {
        if (ErrorDialog is null)
        {
            ErrorDialog = ErrorWindow.Instantiate<ErrorDialog>();
            AddChild(ErrorDialog);
            ErrorDialog.Show();
            ErrorDialog.CloseRequested += () => ErrorDialog = null;
        }
        else ErrorDialog.AddText("\n\nExtra Error:\n");
        ErrorDialog.AddText(error);
    }

    public static void ShowItemFilter() => Singleton.CallDeferred("CreateItemFilterDialogue", ["", "", "0"]);
    public static void ShowItemFilter(string[] args) => Singleton.CallDeferred("CreateItemFilterDialogue", args);
    
    public void CreateItemFilterDialogue(string[] args)
    {
        var filter = ItemFilterWindow.Instantiate<ItemFilter>();
        AddChild(filter);
        filter.SetFilter(args[0], args[1], args[2]);
        filter.Show();
    }

    public static void Save() => OnLateSave?.Invoke();
    public static string GetTimestamp() => DateTime.Now.ToString("[HH:mm:ss]");
    public void UpdateDiscord() => DRPC.CheckDiscord();
}