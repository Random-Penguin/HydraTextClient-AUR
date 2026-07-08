using System;
using Godot;
using HydraTextClient.Scripts.Discord;
using HydraTextClient.Scripts.Settings;

namespace HydraTextClient.Scripts.Controllers;

public partial class MainController : Control
{
    [Export] private PackedScene ErrorWindow;

    private Utility.Popups.ErrorDialog ErrorDialog;

    private static MainController Singleton;

    public static Theme GlobalTheme;

    public static event Action? OnEarlySave;
    public static event Action? OnLateSave;

    public override void _Ready()
    {
        GlobalTheme = Theme;
        GlobalThemeSettings.Init();
        Singleton = this;
    }

    public override void _Notification(int what)
    {
        if (what != NotificationWMCloseRequest) return;
        // DiscordIntegration.Discord.Dispose();
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
            ErrorDialog = ErrorWindow.Instantiate<Utility.Popups.ErrorDialog>();
            AddChild(ErrorDialog);
            ErrorDialog.Show();
            ErrorDialog.CloseRequested += () => ErrorDialog = null;
        }
        else ErrorDialog.AddText("\n\nExtra Error:\n");
        ErrorDialog.AddText(error);
    }

    public static void Save()
    {
        OnEarlySave?.Invoke();
        OnLateSave?.Invoke();
    }

    public static string GetTimestamp() => DateTime.Now.ToString("[HH:mm:ss]");
    public void UpdateDiscord() => DRPC.CheckDiscord();
}