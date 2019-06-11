using System;
using System.Windows.Input;
using Microsoft.Practices.ServiceLocation;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Geometry.Services;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// The interaction mode for the street view interaction
  /// </summary>
  public class StreetViewInteractionMode : MapInteractionMode
  {
    #region Statics
    /// <summary>
    /// The maximum squared distance for determining whether the user has moved a number of pixels
    /// before retrieving street view information again
    /// </summary>
    private double _maxDistanceSquared = 3.0 * 3.0;
    #endregion

    #region Events
    /// <summary>
    /// Street view position event arguments to use within event handlers
    /// </summary>
    public class StreetViewPositionEventArgs
    {
      /// <summary>
      /// Coordinate
      /// </summary>
      public Coordinate Coordinate { get; set; }
    }

    /// <summary>
    /// Request for showing street view for the map position as specified by the args
    /// </summary>
    public delegate bool StreetViewMapMouseEventDelegate(StreetViewInteractionMode sender, StreetViewPositionEventArgs args);
    #endregion

    #region Fields
    /// <summary>
    /// The mouse hover location
    /// </summary>
    private double _mouseHoverX, _mouseHoverY;

    /// <summary>
    /// The cursor for available street view information
    /// </summary>
    private MapInteractionModeImageCursor _viewAvailableCursor;

    /// <summary>
    /// The cursor for street view information not available
    /// </summary>
    private MapInteractionModeImageCursor _viewUnavailableCursor;
    #endregion

    #region Constructor
    /// <summary>
    /// The default constructor
    /// </summary>
    public StreetViewInteractionMode()
    {
      Cursor = Cursors.None;

      // Setup the cursor when a streetview image is available
      _viewAvailableCursor = new MapInteractionModeImageCursor()
      {
        ImageSource = MapInteractionModeImageCursor.ImageSourceForUri(@"Lite;component/Lite/StreetView/Images/streetview.png"),
        XOffset = 16,
        YOffset = 31
      };

      // Setup the cursor when a streetview image is unavailable
      _viewUnavailableCursor = new MapInteractionModeImageCursor()
      {
        ImageSource = MapInteractionModeImageCursor.ImageSourceForUri(@"Lite;component/Lite/StreetView/Images/streetview_bw.png"),
        XOffset = 16,
        YOffset = 31
      };

      ImageCursor = _viewUnavailableCursor;

      // Clear hovering
      _mouseHoverX = -1;
      _mouseHoverY = -1;
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Has the new Coordinate moved enough since last time.
    /// </summary>
    private bool MovedEnoughSinceLast(double x, double y)
    {
      var diffX = x - _mouseHoverX;
      var diffY = y - _mouseHoverY;

      // Determine squared distance
      var distanceSquared = diffX * diffX + diffY * diffY;

      // Compare with max squared distance
      return distanceSquared >= _maxDistanceSquared;
    }

    /// <summary>
    /// Converts the coordinate to the right CS for use in StreetView
    /// </summary>
    /// <param name="coordinate">The coordinate to transform</param>
    /// <returns>The transformed coordinate</returns>
    private Coordinate MapCoordinateToWGS(Coordinate coordinate)
    {
      var geometryService = ServiceLocator.Current.GetInstance<IGeometryService>();
      return geometryService.TransformAsync(coordinate, 3785, 4326).Result;
    }
    #endregion

    #region Events
    /// <summary>
    /// The Request for showing street view event
    /// </summary>
    public event StreetViewMapMouseEventDelegate RequestShowStreetView;

    /// <summary>
    /// Raise the request to show street view at the specified location
    /// </summary>
    /// <param name="mouseEventArgs">The mouse event args</param>
    private void RaiseRequestShowStreetView(Coordinate coordinate)
    {
      var handler = RequestShowStreetView;
      if (handler != null)
      {
        handler(this, new StreetViewPositionEventArgs { Coordinate = MapCoordinateToWGS(coordinate) });
      }
    }

    /// <summary>
    /// An event that will be raised whenever there is a request to 
    /// </summary>
    public event StreetViewMapMouseEventDelegate RequestCheckStreetViewStatus;

    /// <summary>
    /// Raise a request to check the street view status at the specified position
    /// </summary>
    private void RaiseRequestCheckStreetViewStatus(Coordinate coordinate)
    {
      var handler = RequestCheckStreetViewStatus;
      if (handler != null)
      {
        handler(this, new StreetViewPositionEventArgs { Coordinate = MapCoordinateToWGS(coordinate) });
      }
    }
    #endregion

    #region Starting/Stopping
    /// <summary>
    /// Started
    /// </summary>
    protected override void OnStarted(System.Windows.Input.ModifierKeys modifierKeys)
    {
      base.OnStarted(modifierKeys);
    }

    /// <summary>
    /// Stopped
    /// </summary>
    protected override void OnStopped()
    {
      base.OnStopped();
    }
    #endregion

    #region Modifier Keys
    /// <summary>
    /// Do we act upon the specified modifier keys
    /// </summary>
    protected override bool UsesModifierKeys(ModifierKeys modifierKeys)
    {
      // Let no other mode kick in
      return true;
    }
    #endregion

    #region Interaction
    /// <summary>
    /// Key down event
    /// </summary>
    protected override void OnKeyDown(MapViewModel sender, MapKeyEventArgs args)
    {
      base.OnKeyDown(sender, args);

      var keys = args.Key;
    }

    /// <summary>
    /// Enter geometry
    /// </summary>
    protected override void OnMouseEnterGeometry(MapViewModel sender, MapMouseEventArgs args)
    {
      if (args.Source != null)
      {
        args.Source.Cursor = Cursors.None;
      }
      args.Handled = true;
    }

    /// <summary>
    /// Leave geometry
    /// </summary>
    protected override void OnMouseLeaveGeometry(MapViewModel sender, MapMouseEventArgs args)
    {
      if (args.Source != null)
      {
        args.Source.Cursor = null;
      }
      args.Handled = true;
    }

    /// <summary>
    /// Left Button down event
    /// </summary>
    protected override void OnMouseLeftButtonDown(MapViewModel sender, MapMouseEventArgs args)
    {
      _mouseHoverX = -1;
      _mouseHoverY = -1;

      args.Handled = true;
    }

    /// <summary>
    /// Mouse Move event
    /// </summary>
    protected override void OnMouseMove(MapViewModel sender, MapMouseEventArgs args)
    {
      if (_mouseHoverX >= 0 && MovedEnoughSinceLast(args.X, args.Y))
      {
        SetViewAvailability(false);
      }
    }

    /// <summary>
    /// Show street view on the requested coordinate
    /// </summary>
    protected override void OnMouseLeftButtonUp(MapViewModel sender, MapMouseEventArgs args)
    {
      base.OnMouseLeftButtonUp(sender, args);

      args.Handled = true;

      // Only show street view when there is a street view image
      if (CanViewImage)
      {
        // Request to show street view
        RaiseRequestShowStreetView(args.Location.Coordinate);

        // Cancel the current interaction mode
        this.Stop();
      }
    }

    protected override void OnMouseLeftButtonDoubleClick(MapViewModel sender, MapMouseEventArgs args)
    {
      base.OnMouseLeftButtonDoubleClick(sender, args);
      args.Handled = true;
    }

    protected override void OnMouseHover(MapViewModel sender, MapMouseEventArgs args)
    {
      base.OnMouseHover(sender, args);

      // Set hover position
      _mouseHoverX = args.X;
      _mouseHoverY = args.Y;

      // Raise a request to check the street view status
      RaiseRequestCheckStreetViewStatus(args.Location.Coordinate);
    }
    #endregion

    #region Touch Input
    /// <summary>
    /// Touch Down event
    /// </summary>
    protected override void OnTouchDown(MapViewModel sender, MapTouchEventArgs args)
    {
      _mouseHoverX = -1;
      _mouseHoverY = -1;

      args.Handled = true;
    }

    /// <summary>
    /// Touch Move event
    /// </summary>
    protected override void OnTouchMove(MapViewModel sender, MapTouchEventArgs args)
    {
      if (MovedEnoughSinceLast(args.X, args.Y))
      {
        SetViewAvailability(false);

        _mouseHoverX = args.X;
        _mouseHoverY = args.Y;

        // Raise a request to check the street view status
        RaiseRequestCheckStreetViewStatus(args.Location.Coordinate);
      }
    }

    /// <summary>
    /// Touch Up event
    /// </summary>
    protected override void OnTouchUp(MapViewModel sender, MapTouchEventArgs args)
    {
      args.Handled = true;

      // Only show street view when there is a street view image
      if (CanViewImage)
      {
        // Request to show street view
        RaiseRequestShowStreetView(args.Location.Coordinate);
      }

      // Cancel the current interaction mode
      this.Stop();
    }
    #endregion

    #region Public Api
    /// <summary>
    /// Update the interaction mode
    /// </summary>
    /// <param name="available"></param>
    internal void SetViewAvailability(Boolean available)
    {
      var newCursor = (available) ? _viewAvailableCursor : _viewUnavailableCursor;

      if (ImageCursor != newCursor)
      {
        ImageCursor = newCursor;
      }
    }

    /// <summary>
    /// Can a street view image be viewed
    /// </summary>
    internal bool CanViewImage
    {
      get { return ImageCursor == _viewAvailableCursor; }
    }

    #endregion
  }
}
