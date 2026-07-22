using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utility.Loaders;

public abstract class ImageLoader
{
    public abstract string ImageFolder { get; }
    public virtual bool LoadSubDirectories { get; }
    public event Action? OnReloadImages;
    private Dictionary<string, ImageTexture> Images = [];

    protected ImageLoader() => ReloadImages();

    public void ReloadImages()
    {
        if (!Directory.Exists(ImageFolder)) Directory.CreateDirectory(ImageFolder);
        LoadDirectory(ImageFolder);
        ReloadImagesResolved();
        OnReloadImages?.Invoke();
    }

    private void LoadDirectory(string dir)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            try
            {
                var fileName = NameModify(PathToNameModify(file));
                if (Images.ContainsKey(fileName)) continue;
                ImageWasSet(file, fileName, Images[fileName] = ImageTexture.CreateFromImage(Image.LoadFromFile(file)));
            }
            catch (Exception e) { MainController.ShowError($"Error Loading File [{file}]", e); }
        }

        foreach (var subDir in Directory.GetDirectories(dir)) LoadDirectory(subDir);
    }

    public bool TryGet(string name, out ImageTexture img) => Images.TryGetValue(NameModify(name), out img);

    public Texture2D GetOrDef(string name, Texture2D def)
        => !Images.TryGetValue(NameModify(name), out var value) ? def : value;

    public Dictionary<string, ImageTexture> GetImages() => Images;
    public ImageTexture GetImage(string name) => Images[NameModify(name)];
    public abstract void ReloadImagesResolved();
    public abstract void ImageWasSet(string path, string image, ImageTexture img);
    public virtual string NameModify(string name) => name;
    public virtual string PathToNameModify(string path) => Path.GetFileNameWithoutExtension(path);
}