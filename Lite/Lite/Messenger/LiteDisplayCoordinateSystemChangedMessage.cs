using System;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Geometry.CoordinateSystems;

namespace Lite
{
  /// <summary>
  /// Message for indicating change in Display CS
  /// </summary>
  public class LiteDisplayCoordinateSystemChangedMessage : MessageBase
  {
    /// <summary>
    /// Constructor for the change message
    /// </summary>
    /// <param name="sender">The originator of the activation</param>
    public LiteDisplayCoordinateSystemChangedMessage(Object sender, EpsgCoordinateSystemReference displayCs)
    {
      DisplayCoordinateSystem = displayCs;
    }

    /// <summary>
    /// The new Display CS
    /// </summary>
    public EpsgCoordinateSystemReference DisplayCoordinateSystem
    {
      get;
      private set;
    }
  }
}
