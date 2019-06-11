using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Units;

namespace Lite
{
  /// <summary>
  /// The abstract base class for measurers to be used with the corresponding
  /// interaction mode.
  /// </summary>
  public abstract class LiteMapMeasurerBase
  {
    #region Delegates
    /// <summary>
    /// Delegate used for notifying changes in segment length
    /// </summary>
    internal delegate void SegmentLengthTextChangedDelegate(LiteMapMeasurerBase sender, string text);

    /// <summary>
    /// Delegate used for notifying changes in total length
    /// </summary>
    internal delegate void TotalLengthTextChangedDelegate(LiteMapMeasurerBase sender, string text);

    /// <summary>
    /// Delegate used for notifying changes in area 
    /// </summary>
    internal delegate void AreaTextChangedDelegate(LiteMapMeasurerBase sender, string text);

    /// <summary>
    /// Delegate used for notifying changes in geometry
    /// </summary>
    internal delegate void FeatureGeometryChangedDelegate(LiteMapMeasurerBase sender, List<FrameworkElement> geometry);
    #endregion

    #region Static Annotation Helpers
    /// <summary>
    /// A static helper for determining the length (in meters) between specified pixel coordinates
    /// </summary>
    protected static double LengthInMeters(CoordinateSystem cs, ITransform txfm, Coordinate pixelCd1, Coordinate pixelCd2)
    {
      Coordinate worldCd1 = pixelCd1.Transformed(txfm);
      Coordinate worldCd2 = pixelCd2.Transformed(txfm);

      var ls = new LineString(cs, worldCd1, worldCd2);

      var lengthType = cs.IsLocal ? LinearLineLengthType.Meter : LinearLineLengthType.Geodetic;

      return ls.LineLength(lengthType);
    }

    /// <summary>
    /// Transforms the length to a human readable string, making use of the active unit system
    /// </summary>
    protected static String LengthInMetersToString(double length)
    {
      return UnitSystem.Convert(length, "m").ToString();
    }

    /// <summary>
    /// Transforms the area to a human readable string, making use of the active unit system
    /// </summary>
    protected static String AreaInMetersToString(double area)
    {
      return UnitSystem.Convert(area, "m2").ToString();
    }

    /// <summary>
    /// Calculates the area contribution of the given ring
    /// </summary>
    protected static double CalculateAreaContribution(Ring closedRing)
    {
      double result = 0;

      if (closedRing != null && closedRing.IsClosed)
      {
        result = Math.Abs(closedRing.AreaContribution(SurfaceAreaType.Geodetic));
      }

      return result;
    }

    /// <summary>
    /// Creates a closed ring from the given coordinates
    /// </summary>
    /// <param name="coordinates">coordinates of the linestring</param>
    /// <param name="newCoord">optional coordinate to add</param>
    /// <returns>a new closed ring</returns>
    protected static Ring CreateClosedRing(CoordinateSystem cs, IList<Coordinate> coordinates, Coordinate newCoord = null)
    {
      Ring result = null;
      if (coordinates != null && coordinates.Count > 1)
      {
        LineString lineString = new LineString(cs, coordinates);

        if (newCoord != null)
        {
          lineString.Add(newCoord);
        }

        Ring ring = new Ring(lineString);
        ring.Close();

        result = ring;
      }

      return result;
    }

    /// <summary>
    /// Determines the point (as a polygon) at a specified coordinate
    /// </summary>
    protected static System.Windows.Shapes.Polygon PointPolygonFor(Coordinate coord, double angle, double size, Brush brush)
    {
      var polygon = new System.Windows.Shapes.Polygon();

      var cd1 = Coordinate.NewAtDistanceAngleFrom(coord, size, angle - Math.PI / 2.0 + Math.PI / 10);
      var cd2 = Coordinate.NewAtDistanceAngleFrom(coord, size, angle - Math.PI / 2.0 - Math.PI / 10);

      polygon.Points = new System.Windows.Media.PointCollection();
      polygon.Points.Add(new System.Windows.Point(coord.X, coord.Y));
      polygon.Points.Add(new System.Windows.Point(cd1.X, cd1.Y));
      polygon.Points.Add(new System.Windows.Point(cd2.X, cd2.Y));
      polygon.Points.Add(new System.Windows.Point(coord.X, coord.Y));

      polygon.Stroke = brush;
      polygon.Fill = brush;
      return polygon;
    }

    /// <summary>
    /// Creates the annotation for the specified coordinate, style and length
    /// </summary>
    protected static FrameworkElement AnnotationForLength(CoordinateSystem cs, Coordinate coord, double angle, Brush style, double size, double lengthInMeters, TextAlignment alignment, int extraDistance)
    {
      var realAngle = 180 * angle / Math.PI;
      var annotation = new TextBlock()
      {
        Text = LengthInMetersToString(lengthInMeters),
        FontSize = size,
        TextAlignment = alignment
      };

      var x = coord.X - Math.Cos(angle) * extraDistance;
      var y = coord.Y - Math.Sin(angle) * extraDistance;

      var txfm = new TransformGroup();
      txfm.Children.Add(new RotateTransform { Angle = realAngle });
      txfm.Children.Add(new TranslateTransform { X = x, Y = y });

      annotation.RenderTransform = txfm;
      return annotation;
    }

    protected static FrameworkElement AnnotationForArea(CoordinateSystem cs, Coordinate coord, Brush style, double size, double areaInMeters)
    {
      var annotation = new TextBlock()
      {
        Text = AreaInMetersToString(areaInMeters),
        FontSize = size,
        TextAlignment = TextAlignment.Center
      };

      var x = coord.X;
      var y = coord.Y;

      var txfm = new TransformGroup();
      txfm.Children.Add(new TranslateTransform { X = x, Y = y });

      annotation.RenderTransform = txfm;

      return annotation;
    }
    #endregion

    #region Static fields
    /// <summary>
    /// The zero length string
    /// </summary>
    private static string _lengthZeroString = UnitSystem.Convert(0.0, "m").ToString();
    #endregion

    #region Fields
    /// <summary>
    /// The resulting geometry
    /// </summary>
    private List<FrameworkElement> _geometry;

    /// <summary>
    /// The list of coordinates
    /// </summary>
    private List<Coordinate> _coordinates = new List<Coordinate>();

    /// <summary>
    /// The last known mouse position
    /// </summary>
    private Coordinate _lastMouseCoordinate;

    /// <summary>
    /// The last coordinate
    /// </summary>
    private Coordinate _lastCoordinate;

    /// <summary>
    /// The meters per pixel
    /// </summary>
    private double _metersPerPixel;

    /// <summary>
    /// The cs units per pixel
    /// </summary>
    private double _CSUnitsPerPixel;
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    internal LiteMapMeasurerBase()
    {
      Initialize();
    }
    #endregion

    #region
    protected abstract void Initialize();

    /// <summary>
    /// Sets the geometry
    /// </summary>
    protected List<FrameworkElement> FeatureGeometry
    {
      get { return _geometry; }
      set
      {
        _geometry = value;
        RaiseFeatureGeometryChanged(_geometry);
      }
    }

    /// <summary>
    /// Holds the coordinates
    /// </summary>
    protected IList<Coordinate> Coordinates
    {
      get { return _coordinates; }
    }

    /// <summary>
    /// Holds the last coordinate
    /// </summary>
    protected Coordinate LastCoordinate
    {
      get { return _lastCoordinate; }
      set { _lastCoordinate = value; }
    }

    /// <summary>
    /// Holds the last mouse coordinate
    /// </summary>
    protected Coordinate LastMouseCoordinate
    {
      get { return _lastMouseCoordinate; }
      set { _lastMouseCoordinate = value; }
    }

    /// <summary>
    /// Returns the display coordinate system
    /// </summary>
    internal CoordinateSystem CoordinateSystem { get; set; }

    /// <summary>
    /// Returns the display coordinate system
    /// </summary>
    internal SpatialEye.Framework.Geometry.ITransform PixelToWorldTransform { get; set; }

    /// <summary>
    /// Raises that segment length has changed
    /// </summary>
    protected void RaiseStatusSegmentLengthChanged(string text)
    {
      var handler = SegmentLengthTextChanged;
      if (handler != null)
      {
        handler(this, text);
      }
    }

    /// <summary>
    /// Raises that segment length has changed
    /// </summary>
    protected void RaiseStatusSegmentLengthChanged(double length)
    {
      RaiseStatusSegmentLengthChanged(LengthInMetersToString(length));
    }

    /// <summary>
    /// Raises that total length has changed
    /// </summary>
    protected void RaiseEmptySegmentLengthTextChanged()
    {
      var handler = SegmentLengthTextChanged;
      if (handler != null)
      {
        handler(this, _lengthZeroString);
      }
    }

    /// <summary>
    /// Raises that total length has changed
    /// </summary>
    protected void RaiseTotalLengthChanged(string text)
    {
      var handler = TotalLengthTextChanged;
      if (handler != null)
      {
        handler(this, text);
      }
    }

    /// <summary>
    /// Raises that segment length has changed
    /// </summary>
    protected void RaiseTotalLengthChanged(double length)
    {
      RaiseTotalLengthChanged(LengthInMetersToString(length));
    }

    /// <summary>
    /// Raises that total length has changed
    /// </summary>
    protected void RaiseEmptyTotalLengthChanged()
    {
      var handler = TotalLengthTextChanged;
      if (handler != null)
      {
        handler(this, _lengthZeroString);
      }
    }

    /// <summary>
    /// Raises that area has changed
    /// </summary>
    protected void RaiseAreaChanged(string text)
    {
      var handler = AreaTextChanged;
      if (handler != null)
      {
        handler(this, text);
      }
    }

    /// <summary>
    /// Raises that the area to be displayed has changed. Interprets the specified
    /// area as an area in m2.
    /// </summary>
    protected void RaiseAreaChanged(double area)
    {
      RaiseAreaChanged(UnitSystem.Convert(area, "m2").ToString());
    }

    /// <summary>
    /// Raises that the area to be displayed has changed. Interpretes all coordinates
    /// in the current display coordinate system (as specified by the func).
    /// </summary>
    protected void RaiseAreaChanged(IList<Coordinate> coordinates, Coordinate newCoord)
    {
      var handler = AreaTextChanged;
      if (handler != null)
      {
        if (coordinates != null && coordinates.Count > 1)
        {
          LineString lineString = new LineString(CoordinateSystem, coordinates);

          if (newCoord != null)
          {
            lineString.Add(newCoord);
          }

          Ring ring = new Ring(lineString);
          ring.Close();

          RaiseAreaChanged(Math.Abs(ring.AreaContribution(SurfaceAreaType.Geodetic)));
        }
        else
        {
          RaiseAreaChanged("-");
        }
      }
    }

    /// <summary>
    /// Raises a change in styled feature geometry, to be rendered by the measure component
    /// </summary>
    private void RaiseFeatureGeometryChanged(List<FrameworkElement> geometry)
    {
      var handler = FeatureGeometryChanged;
      if (handler != null)
      {
        handler(this, geometry);
      }
    }
    #endregion

    #region Properties and Events
    /// <summary>
    /// The event raised when status segment length text has changed
    /// </summary>
    internal event SegmentLengthTextChangedDelegate SegmentLengthTextChanged;

    /// <summary>
    /// The event raised when status total length text has changed
    /// </summary>
    internal event TotalLengthTextChangedDelegate TotalLengthTextChanged;

    /// <summary>
    /// The event raised when status area text has changed
    /// </summary>
    internal event AreaTextChangedDelegate AreaTextChanged;

    /// <summary>
    /// The event raised whenever the feature geometry has changed
    /// </summary>
    internal event FeatureGeometryChangedDelegate FeatureGeometryChanged;
    #endregion

    #region Public API

    /// <summary>
    /// Gets the resource key describing this mode
    /// </summary>
    public virtual String DescriptionResourceKey { get { return null; } }

    /// <summary>
    /// Start the interaction
    /// </summary>
    internal virtual void Start()
    {
      // Start with an empty element
      Clear();
    }

    /// <summary>
    /// Start the interaction
    /// </summary>
    internal virtual void Stop()
    {
      // End with an empty element
      Clear();
    }

    /// <summary>
    /// Clears the active elements
    /// </summary>
    internal virtual void Clear()
    {
      Coordinates.Clear();
      LastCoordinate = null;
      LastMouseCoordinate = null;
      this.FeatureGeometry = new List<FrameworkElement>();

      RaiseStatusSegmentLengthChanged("");
      RaiseTotalLengthChanged("");
      RaiseAreaChanged("");
    }

    /// <summary>
    /// The meters per pixel
    /// </summary>
    internal double MetersPerPixel
    {
      get
      {
        return _metersPerPixel;
      }
      set
      {
        _metersPerPixel = value;
        OnMetersPerPixelChanged(_metersPerPixel);
      }
    }

    /// <summary>
    /// The CS Units per pixel
    /// </summary>
    internal double CSUnitsPerPixel
    {
      get
      {
        return _CSUnitsPerPixel;
      }
      set
      {
        _CSUnitsPerPixel = value;
        OnCSUnitsPerPixelChanged(_CSUnitsPerPixel);
      }
    }

    /// <summary>
    /// Returns the length in meters for a number of pixels
    /// </summary>
    internal double MetersForPixels(int pixels)
    {
      return pixels * _metersPerPixel;
    }

    /// <summary>
    /// Returns the length in Display CS Units to be used that 
    /// correspond to a number of pixels
    /// </summary>
    internal double CSUnitsForPixels(int pixels)
    {
      return pixels * _CSUnitsPerPixel;
    }

    /// <summary>
    /// Add a coordinate
    /// </summary>
    internal abstract void AddCoordinate(Coordinate worldCoord);

    /// <summary>
    /// Move the coordinate
    /// </summary>
    /// <param name="worldCoord"></param>
    internal abstract void MoveCoordinate(Coordinate worldCoord);

    /// <summary>
    /// Move the coordinate
    /// </summary>
    /// <param name="worldCoord"></param>
    internal abstract void MoveAll(double divX, double divY);

    /// <summary>
    /// Deletes the last coordinate
    /// </summary>
    internal abstract void DeleteLastCoordinate();

    /// <summary>
    /// Close the trail
    /// </summary>
    internal abstract void CloseMeasurer();

    /// <summary>
    /// End the measuring
    /// </summary>
    internal virtual void EndMeasuring()
    {
      LastCoordinate = null;
    }

    internal virtual bool CanMoveScreen
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// The meters per pixel changed
    /// </summary>
    internal virtual void OnMetersPerPixelChanged(double metersPerPixel)
    { }

    /// <summary>
    /// The cs units per pixel changed
    /// </summary>
    internal virtual void OnCSUnitsPerPixelChanged(double csUnitsPerPixel)
    { }
    #endregion
  }
}
