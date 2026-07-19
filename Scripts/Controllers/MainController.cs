using System;
using System.IO;
using Godot;
using HydraTextClient.Scripts.Consoles.Godot;
using HydraTextClient.Scripts.Discord;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Controllers;

public partial class MainController : Control
{
    public const string WindowSaveId = "window_nodes/MAIN_WINDOW";
    public const string WindowBackGroundImage = "Theme/BackgroundImage";
    public const string WindowBackGroundImageAlpha = "Theme/BackgroundImageAlpha";

    [Export] private PackedScene ErrorWindow;
    [Export] private PackedScene ItemFilterWindow;
    [Export] private PackedScene ItemFilterDisplay;
    [Export] private LoggerLabel GDLogger;
    [Export] private TextureRect BackgroundImage;
    [Export] private SettingsPorter Porter;

    private ErrorDialog ErrorDialog;

    private static MainController Singleton;

    public static Theme GlobalTheme;

    public static event Action? OnSave;
    public static event Action? OnExit;

    public override void _EnterTree()
    {
        Singleton = this;
        GDLogger.Init();
        OS.AddLogger(GDLogger.Logger);
        GlobalTheme = Theme;

        var window = GetWindow();
        window.Size = SaveType<Vector2I>.Load($"{WindowSaveId}_size", window.Size);
        window.Position = SaveType<Vector2I>.Load($"{WindowSaveId}_pos", window.Position);
        window.SizeChanged += () => SaveType<Vector2I>.Save($"{WindowSaveId}_size", window.Size, true);

        var mainBackgroundBox = (StyleBoxFlat)GetThemeStylebox("panel");
        mainBackgroundBox.BgColor = ColorIdConstants.ColorConstant.UiBackground.Load();

        var mainPopupBox = (StyleBoxFlat)GlobalTheme.GetStylebox("panel", "Panel");
        mainPopupBox.BgColor = ColorIdConstants.ColorConstant.PopupBackground.Load();

        SaveType<HexColor>.OnSaveEvent += (id, val) =>
        {
            if (!ColorIdConstants.IdToConstant.TryGetValue(id, out var constant)) return;
            switch (constant)
            {
                case ColorIdConstants.ColorConstant.UiBackground: mainBackgroundBox.BgColor = val; break;
                case ColorIdConstants.ColorConstant.PopupBackground: mainPopupBox.BgColor = val; break;
            }
        };

        LoadBackgroundImage(SaveType<string>.Load(WindowBackGroundImage, "", false));
        LoadBackgroundImageTransparency(SaveType<double>.Load(WindowBackGroundImageAlpha, 255));

        SaveType<string>.OnSaveEvent += (id, val) =>
        {
            if (id is not WindowBackGroundImage) return;
            LoadBackgroundImage(val);
        };

        SaveType<double>.OnSaveEvent += (id, val) =>
        {
            if (id is not WindowBackGroundImageAlpha) return;
            LoadBackgroundImageTransparency(val);
        };
    }

    public override void _Ready()
    {
        DRPC.Init();
        GlobalThemeSettings.Init();
        
        if (SaveType<bool>.Load("Main/HasPorted", !File.Exists(Directories.LegacyData))) return;
        Porter.Startup();
        Porter.Show();
    }

    public override void _Notification(int what)
    {
        if (what != NotificationWMCloseRequest) return;
        SaveType<Vector2I>.Save($"{WindowSaveId}_pos", GetWindow().Position, true);
        Save();
        OnExit?.Invoke();
    }

    public void LoadBackgroundImage(string path)
    {
        if (path is "" || !File.Exists(path)) return;
        BackgroundImage.Texture = ImageTexture.CreateFromImage(Image.LoadFromFile(path));
    }
    
    public void LoadBackgroundImageTransparency(double val)
    {
        var color = BackgroundImage.Modulate;
        color.A = (int)Math.Clamp(val, 0, 255)/255f;
        BackgroundImage.Modulate = color;
    }

    public static void ShowError(string message, Exception e) => ShowError($"{message}\n{e.Message}\n{e.StackTrace}");
    public static void ShowError(Exception e) => ShowError($"{e.Message}\n{e.StackTrace}");
    public static void ShowError(string[] error) => ShowError(string.Join('\n', error));
    public static void ShowError(string error) => Singleton.CallDeferred("CreateErrorDialogue", error);

    public void CreateErrorDialogue(string error)
    {
        if (ErrorDialog is null || !IsInstanceValid(ErrorDialog) || ErrorDialog.IsQueuedForDeletion())
        {
            ErrorDialog = ErrorWindow.Instantiate<ErrorDialog>();
            AddChild(ErrorDialog);
            ErrorDialog.Show();
            ErrorDialog.CloseRequested += () => ErrorDialog = null;
        }
        else ErrorDialog.AddText("\n\nExtra Error:\n");
        ErrorDialog.AddText(error);
        GD.PrintErr(error);
    }

    public static void ShowItemFilter() => Singleton.CallDeferred("CreateItemFilterDialogue", (string[])["", "", "0"]);
    public static void ShowItemFilter(string[] args) => Singleton.CallDeferred("CreateItemFilterDialogue", args);

    public void CreateItemFilterDialogue(string[] args)
    {
        var filter = ItemFilterWindow.Instantiate<ItemFilter>();
        AddChild(filter);
        filter.SetFilter(args[0], args[1], args[2]);
        filter.Show();
    }

    public static void Save() => OnSave?.Invoke();
    public static string GetTimestamp() => DateTime.Now.ToString("[HH:mm:ss]");
    public static void SetAlwaysOnTop(bool val) => Singleton.GetWindow().AlwaysOnTop = val;
    public void UpdateDiscord() => DRPC.CheckDiscord();
}