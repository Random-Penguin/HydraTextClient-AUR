using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utility.Loaders;

public class EmoteLoader : ImageLoader
{
    public static EmoteLoader Singleton = new EmoteLoader(); 
    
    public override string ImageFolder => Directories.Emotes;
    public override void ReloadImagesResolved() => GD.Print("Loading Emotes");

    public override void ImageWasSet(string path, string image, ImageTexture img)
        => GD.Print($"Loaded image [{path.Replace(ImageFolder, ".")}] as emote [{image}]");

    public override string NameModify(string name) => name.ToLower();
    public override string PathToNameModify(string path) => Path.GetFileNameWithoutExtension(path);
}