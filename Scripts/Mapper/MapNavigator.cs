using Godot;

namespace HydraTextClient.Scripts.Mapper;

public partial class MapNavigator : ScrollContainer
{
    [Export] public MapControl Container;
    
    public void SetImage(Texture2D map)
    {
        Container.MapImage.Texture = map;
        Container.ResetZoom();
    }
}