using SpatialEye.Framework.Client;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lite
{
  /// <summary>
  /// The view to use for displaying the Hovered Feature Geometry information.
  /// It is tied in with a LiteMapHoverViewModel that reacts to HoveredFeatureGeometry
  /// events that are sent by the active LiteMapViewModel in case Client-side 
  /// geometry is hovered. This only has effect in case client-side
  /// </summary>
  public class LiteMapHoverView : MapFeatureGeometryNotificationControl
  {
    /// <summary>
    /// The default constructor
    /// </summary>
    public LiteMapHoverView()
    {
      // Track the mouse
      this.MouseTrackingMode = SpatialEye.Framework.Client.MouseTrackingMode.Follow;
    }
  }
}
