using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.ServiceProviders.XY;

namespace Lite
{
  /// <summary>
  /// A simple Tile based service provider that has extra capabilities for determining 
  /// rerouting information
  /// </summary>
  public abstract class LiteWebServiceProviderBase : TileServiceProvider
  {
    #region Constructor
    /// <summary>
    /// Constructs the service provider for the specified name
    /// </summary>
    /// <param name="name">The name of the service provider</param>
    public LiteWebServiceProviderBase(string name)
      : base(name)
    { }
    #endregion

    #region Reroute Provider
    /// <summary>
    /// The X&Y provider to use for rerouting
    /// </summary>
    public XYServiceProvider RerouteGeoLocatorVia { get; set; }
    #endregion
  }
}

