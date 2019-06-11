using System.Collections.Generic;

using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// Returns a Lite ThemeLayerViewModel that represents a Layer element
  /// in the Themes view model
  /// </summary>
  public class LiteMapThemeLayerViewModel : MapThemeLayerViewModel
  {
    #region Constructor
    /// <summary>
    /// Creates the Lite ThemeLayerViewModel
    /// </summary>
    public LiteMapThemeLayerViewModel(MapLayerViewModel mapLayerViewModel)
      : base(mapLayerViewModel)
    { }
    #endregion

    #region Element Creation
    /// <summary>
    /// Should return all individual parts of the displayed layer in the Themes
    /// </summary>
    protected override IList<MapThemeLayerElementViewModel> NewThemeLayerElements(MapViewModel map, MapLayerViewModel layer)
    {
      // Get the base presentation for the Layer (style bits, parameter bits, mode bits)
      var elements = base.NewThemeLayerElements(map, layer);

      var geometryLayer = layer as FeatureGeometryMapLayerViewModel;
      if (geometryLayer != null)
      {
        elements.Insert(0, new LiteMapThemeLayerFeatureGeometryElement(map, geometryLayer));
      }

      return elements;
    }
    #endregion
  }
}
