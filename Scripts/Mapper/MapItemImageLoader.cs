using System.IO;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Mapper;

public class MapItemImageLoader(string path) : ImageLoader
{
    public override string ImageFolder => $"{path}/images";
    public override string NameModify(string name) => name.ToLower();
    public override string PathToNameModify(string path) => Path.GetFileNameWithoutExtension(path);
}