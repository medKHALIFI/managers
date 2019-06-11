using System;
using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The default interaction mode for Lite, keeping track of the last pressed world location
  /// as an example on how to keep track of mouse events.
  /// </summary>
  public class LiteMapSelectInteractionMode : MapSelectInteractionMode
  {
    /// <summary>
    /// The retained information for button down events
    /// </summary>
    private MapMouseEventArgs _mapButtonDownArgs;

    /// <summary>
    /// Override for the leftButtonDown event on the Map
    /// </summary>
    protected override void OnMouseLeftButtonDown(MapViewModel map, MapMouseEventArgs args)
    {
      _mapButtonDownArgs = args;

      base.OnMouseLeftButtonDown(map, args);
    }

    /// <summary>
    /// If we leave the map, make sure we reset any button down events
    /// </summary>
    protected override void OnMouseLeave(MapViewModel map, MapMouseEventArgs args)
    {
      _mapButtonDownArgs = null;
      base.OnMouseLeave(map, args);
    }

    /// <summary>
    /// Override for the leftButtonUp event on the Map
    /// </summary>
    protected override void OnMouseLeftButtonUp(MapViewModel map, MapMouseEventArgs args)
    {
      MapMouseEventArgs resultArgs = null;
      var downArgs = _mapButtonDownArgs;
      _mapButtonDownArgs = null;
      
      if (downArgs != null && args != null)
      {
        if (Math.Abs(downArgs.X - args.X) < 5 && Math.Abs(downArgs.Y - args.Y) < 5)
        {
          resultArgs = args;
        }
      }

      this.LastPressedMouseArgs = resultArgs;

      base.OnMouseLeftButtonUp(map, args);
    }

    /// <summary>
    /// Holds the last pressed world location, in case the distance between the
    /// mouse button down and up was within reach
    /// </summary>
    public MapMouseEventArgs LastPressedMouseArgs
    {
      get;
      private set;
    }
  }
}
