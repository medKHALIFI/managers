using SpatialEye.Framework.GeoLocators.Services;
using SpatialEye.Framework.Maps.Services;

namespace Lite
{
  /// <summary>
  /// The openStreetMap Service Provider that is only used for its GeoLocator.
  /// The default MapService is unregistered, so this service provider does not
  /// provide OSM Maps. 
  /// </summary>
  public class LiteOsmServiceProvider : LiteWebServiceProviderBase
  {
    #region Constructor
    /// <summary>
    /// The default constructor of the OSM Service Provider
    /// </summary>
    public LiteOsmServiceProvider()
      : base("OpenStreetMap")
    {
      // Register the GeoLocator Service Agent
      this.ServiceLocator.Register<IGeoLocatorService, LiteOsmGeoLocatorServiceAgent>();

      // Remove the Agent for now, since all Tiles are provided by Reference Providers as being set up in the Main X&Y server
      this.ServiceLocator.Unregister<IMapService>();
    }
    #endregion
  }
}
