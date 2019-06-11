using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

using SpatialEye.Framework;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// The measurer that is used for measuring in dimensioning mode, allowing the 
  /// user to place points in a dimensioning fashion
  /// </summary>
  public class LiteMapDimMeasurer : LiteMapMeasurerBase
  {
    #region Static
    /// <summary>
    /// The line style we use when rendering the curves added
    /// </summary>
    private static Brush _baseLineStyle;

    /// <summary>
    /// The line style we use when rendering the curves added
    /// </summary>
    private static Brush _measureLineStyle;

    /// <summary>
    /// The line style we use when rendering the curves added
    /// </summary>
    private static Brush _measureInvalidLineStyle;

    /// <summary>
    /// The line style we use when rendering the curves added
    /// </summary>
    private static Brush _baseHelperLineStyle;

    /// <summary>
    /// The line style we use when rendering the curves added
    /// </summary>
    private static Brush _dimPointStyle;

    /// <summary>
    /// The annotation style we use when rendering the length of a segment
    /// </summary>
    private static Brush _measureAnnotationStyle;

    static LiteMapDimMeasurer()
    {
      // The styles
      _baseLineStyle = new SolidColorBrush(Colors.Gray);
      _baseHelperLineStyle = new SolidColorBrush(Color.FromArgb(127, 120, 130, 140));

      _measureLineStyle = new SolidColorBrush(Colors.Black);
      _measureInvalidLineStyle = new SolidColorBrush(Colors.Red);

      _measureAnnotationStyle = new SolidColorBrush(Colors.Black);

      _dimPointStyle = new SolidColorBrush(Colors.Black);
    }
    #endregion

    #region private helper class
    /// <summary>
    /// The abstract base class for elements that are part of a dimension
    /// </summary>
    private abstract class DimElement
    {
      /// <summary>
      /// The coordinate system, for creation of curves/points/annotations
      /// </summary>
      internal CoordinateSystem CoordinateSystem;

      /// <summary>
      /// The transform of the element
      /// </summary>
      internal ITransform Transform;

      /// <summary>
      /// The start coordinate (start of base, or intermediate point of previous measure element)
      /// </summary>
      internal Coordinate Start;

      /// <summary>
      /// The end coordinate of the element (end of base, or actual end of measure element)
      /// </summary>
      internal Coordinate End;

      /// <summary>
      /// The angle from start to end (helper
      /// </summary>
      internal double Angle;

      /// <summary>
      /// The curves (output)
      /// </summary>
      internal List<System.Windows.Shapes.Line> Curves;

      /// <summary>
      /// The annotations output
      /// </summary>
      internal List<FrameworkElement> Annotations;

      /// <summary>
      /// The points output
      /// </summary>
      internal List<System.Windows.Shapes.Polygon> Points;

      /// <summary>
      /// Implemented by base/measure subclasses
      /// </summary>
      /// <param name="hasMeasureElementes"></param>
      internal abstract void RecalculateCurvesAndAnnotations(bool hasMeasureElementes);

      /// <summary>
      /// The CS Units per pixel
      /// </summary>
      internal double CSUnitsPerPixel;

      /// <summary>
      /// Returns the number of units per pixel
      /// </summary>
      protected double CSUnitsForPixels(int pixels)
      {
        return pixels;
      }
    }

    /// <summary>
    /// The base element class that is a helper class for the first two points in the dimension.
    /// The first two points determine the start point (second point) and the direction (first 
    /// point to second point).
    /// </summary>
    private class DimBaseElement : DimElement
    {
      #region Fields
      /// <summary>
      /// Is the collection cleared
      /// </summary>
      private bool _isCleared;
      #endregion

      internal override void RecalculateCurvesAndAnnotations(bool hasMeasureElements)
      {
        Curves = new List<Line>();
        Annotations = new List<FrameworkElement>();
        Points = new List<System.Windows.Shapes.Polygon>();

        Angle = Start.AngleTo(End);

        // Calculate the total distances 
        var totalDistance = 2000;

        // The total start (the far-away start point)
        var TotalStart = Coordinate.NewAtDistanceAngleFrom(Start, totalDistance, Angle);

        // The total end (the far-away end point)
        var totalEnd = Coordinate.NewAtDistanceAngleFrom(Start, totalDistance, Angle + Math.PI);

        if (!_isCleared)
        {
          if (!hasMeasureElements && Start.DistanceTo(End) > 3)
          {
            // Only add the base helper in case there are no measure elements set up
            Curves.Add(new Line() { X1 = TotalStart.X, Y1 = TotalStart.Y, X2 = totalEnd.X, Y2 = totalEnd.Y, Stroke = _baseHelperLineStyle });
          }

          // Do the simple direct base line
          var fullLs = !hasMeasureElements ? _baseLineStyle : _baseHelperLineStyle;
          Curves.Add(new Line() { X1 = Start.X, Y1 = Start.Y, X2 = End.X, Y2 = End.Y, Stroke = fullLs });
        }
      }

      /// <summary>
      /// Clears the geometry of the base-line, which is no longer needed
      /// </summary>
      internal void ClearGeometry()
      {
        _isCleared = true;
        if (Curves != null)
        {
          Curves.Clear();
        }
      }
    }

    /// <summary>
    /// The measure element class that is a helper class for actual measurements
    /// in the dimensioning.
    /// </summary>
    private class DimMeasureElement : DimElement
    {
      /// <summary>
      /// References to the base element
      /// </summary>
      internal DimBaseElement Base;

      /// <summary>
      /// Is this the first measure element
      /// </summary>
      internal bool First;

      /// <summary>
      /// The intermediate coordinate (which is the orthogonal coordinate/intersection of straights)
      /// </summary>
      internal Coordinate Intermediate;

      /// <summary>
      /// Is the measure element valid
      /// </summary>
      internal bool Valid;

      /// <summary>
      /// Calculate the intermediate coordinate
      /// </summary>
      private void CalculateIntermediate()
      {
        double angle = Base.Angle + MathSE.HalfPI;
        try
        {
          // Interaction calculation using cross products 
          var a = Base.Start;
          var b = Base.End;
          var c = Coordinate.NewAtDistanceAngleFrom(End, 50, angle);
          var d = Coordinate.NewAtDistanceAngleFrom(End, 50, angle + Math.PI);
          var l1 = new HCoordinate(a).HCross(new HCoordinate(b));
          var l2 = new HCoordinate(c).HCross(new HCoordinate(d));
          Intermediate = new Coordinate(l1.HCross(l2));
        }
        catch
        {
          // Backstop in case of not being able to calculate the intersection point
          Intermediate = Start;
        }
      }

      /// <summary>
      /// Recalculate the output of the curves
      /// </summary>
      internal override void RecalculateCurvesAndAnnotations(bool hasMeasureElements)
      {
        // We only check for valid elements in case we are more away than so many pixels 
        double checkValidMinDistance = CSUnitsForPixels(5);

        // Display the Along-side segment text (in direction of base), when more than so many pixels long
        double displayAlongSideTextMinDistance = CSUnitsForPixels(65);

        // Display the Cumulative text when the segment is longer than the specified pixels
        double displayAlongSideEndTextMinDistance = CSUnitsForPixels(100);

        // Display the orthogonal text when the ortho-length is more than the specified pixels
        double displayOrthogonalTextMinDistance = CSUnitsForPixels(65);

        // Display the Ortho Total when the Along-side Distance (the segment in base-direction)
        // is longer than the given length. For clarity to the end-user this could be set to the
        // same value as displaying the segment-length itself but can be done earlier if required.
        double displayOrthogonalEndTextMinAlongDistance = CSUnitsForPixels(50);

        // Keep Ortho-Total-MinOrth-Distance the same as the ortho-distance itself, 
        // so total-ortho text does only appear in case the ortho text itself is displayed.
        // If not, the user might confuse this one with the ortho-value itself (since it is
        // displayed and the ortho-text maybe is not yet).
        double displayOrthogonalEndTextMinOrthogonalDistance = CSUnitsForPixels(65);

        // Display the points in case the length of the corresponding segment is longer than the
        // specified pixels
        double displayPointsMinDistance = CSUnitsForPixels(24);

        // Display the points in case the length of the corresponding segment is longer than the
        // specified pixels. This applies to drawing ortho-points in case the along-side segment
        // is longer than the specified pixels.
        double displayOtherPointsMinAlongDistance = CSUnitsForPixels(15);

        // The offset in pixels above the lines (for the segment and ortho texts, not the totals).
        double offsetPixelsInCSUnits = CSUnitsForPixels(2);

        // Initialize the output geometry
        Curves = new List<Line>();
        Annotations = new List<FrameworkElement>();
        Points = new List<System.Windows.Shapes.Polygon>();

        // Start by calculating the intermediate (intersection) point.
        CalculateIntermediate();

        // Setting up the along curve (the one going straight again and again)
        var alongCurve = new Curve(new LineString(CoordinateSystem, Start, Intermediate));
        var alongLine = new Line { X1 = Start.X, Y1 = Start.Y, X2 = Intermediate.X, Y2 = Intermediate.Y, Stroke = _measureLineStyle };
        var alongLengthInCSUnits = Start.DistanceTo(Intermediate);
        var alongLengthInMeters = LiteMapMeasurerBase.LengthInMeters(CoordinateSystem, Transform, Start, Intermediate);

        // Setting up the orthogonal curve
        var orthogonalCurve = new Curve(new LineString(CoordinateSystem, Intermediate, End));
        var orthogonalLine = new Line { X1 = Intermediate.X, Y1 = Intermediate.Y, X2 = End.X, Y2 = End.Y, Stroke = _measureLineStyle };
        var orthogonalLengthInCSUnits = Intermediate.DistanceTo(End);
        var orthogonalLengthInMeters = LiteMapMeasurerBase.LengthInMeters(CoordinateSystem, Transform, Intermediate, End);

        // Check whether we are Valid (not going in the wrong direction).
        // This applies not to the first point, that can be used to determine the direction, but applies
        // to subsequent points only.
        // We still do use the valid-flag for the first element though, just to indicate that we need to reverse the lot
        Valid = true;
        if (alongLengthInCSUnits > checkValidMinDistance && Math.Abs(Base.Angle - Start.AngleTo(Intermediate)) > 1)
        {
          if (!First)
          {
            alongLine.Stroke = _measureInvalidLineStyle;
            orthogonalLine.Stroke = _measureInvalidLineStyle;
          }
          Valid = false;
        }

        // Add the curves to the output. These are the along-curve (in the base-line's direction) and the ortho-curve
        Curves.Add(alongLine);
        Curves.Add(orthogonalLine);

        // In case we are not valid do not draw the result, unless we are the first measure-element.
        // In that case we can act as any other, since the first element can be used to determine direction
        if (!Valid && !First)
        {
          return;
        }

        // The point drawing (dimension endpoints)
        // We can choose to only draw points when all points are capable of being shown (direct and ortho)
        // This leads to a neater way of display the dimensions.
        bool drawPointsDirect = alongLengthInCSUnits > displayPointsMinDistance;
        bool drawPointsOrtho = orthogonalLengthInCSUnits > displayPointsMinDistance && alongLengthInCSUnits > displayOtherPointsMinAlongDistance;
        bool drawPoints = drawPointsDirect && drawPointsOrtho;

        // Draw the DimPoints on the direct line, either make dependent on 
        // - drawPointsDirect (these can be drawn)
        // - drawPoints       (direct + ortho points can be drawn)
        if (drawPointsDirect)
        {
          double ptAngle = Start.AngleTo(Intermediate);

          Points.Add(PointPolygonFor(Start, ptAngle + MathSE.HalfPI, 10, _dimPointStyle));
          Points.Add(PointPolygonFor(Intermediate, ptAngle - MathSE.HalfPI, 10, _dimPointStyle));
        }

        // Draw the DimPoints on the Orthogonal line, either make dependent on 
        // - drawPointsOrtho (these can be drawn)
        // - drawPoints      (ortho + direct points can be drawn)
        if (drawPointsOrtho)
        {
          double ptAngle2 = End.AngleTo(Intermediate);

          Points.Add(PointPolygonFor(Intermediate, ptAngle2 - MathSE.HalfPI, 10, _dimPointStyle));
          Points.Add(PointPolygonFor(End, ptAngle2 + MathSE.HalfPI, 10, _dimPointStyle));
        }

        // Draw the Text in the direct part (segment length)
        if (alongLengthInCSUnits > displayAlongSideTextMinDistance)
        {
          var baseToIntermediateCoordinate = Start.Middle(Intermediate);
          double baseToIntermediateCoordinateAngle = MathSE.NormalizedAngle(Start.AngleTo(Intermediate));

          //TextAlignment alignment = TextAlignment.Bottom;
          TextAlignment alignment = TextAlignment.Center;

          if (baseToIntermediateCoordinateAngle > MathSE.HalfPI && baseToIntermediateCoordinateAngle < 3 * MathSE.HalfPI)
          {
            baseToIntermediateCoordinateAngle += MathSE.PI;
          }

          // Get the text slightly up (couple of pixels), to make sure it is drawn loose from the line
          double upAngle = baseToIntermediateCoordinateAngle + MathSE.HalfPI;
          baseToIntermediateCoordinate = Coordinate.NewAtDistanceAngleFrom(baseToIntermediateCoordinate, offsetPixelsInCSUnits, upAngle);

          Annotations.Add(AnnotationForLength(this.CoordinateSystem, baseToIntermediateCoordinate, baseToIntermediateCoordinateAngle, _measureAnnotationStyle, 9, alongLengthInMeters, alignment, 15));
        }

        // Draw the orthogonal Text in the orthogonal part
        if (orthogonalLengthInCSUnits > displayOrthogonalTextMinDistance)
        {
          var baseToIntermediateCoordinate = Intermediate.Middle(End);
          var baseToIntermediateCoordinateAngle = MathSE.NormalizedAngle(Intermediate.AngleTo(End));
          //var alignment = TextAlignment.Bottom;
          var alignment = TextAlignment.Center;

          if (baseToIntermediateCoordinateAngle > MathSE.HalfPI && baseToIntermediateCoordinateAngle < 3 * MathSE.HalfPI)
          {
            baseToIntermediateCoordinateAngle += MathSE.PI;
          }

          double upAngle = baseToIntermediateCoordinateAngle + MathSE.HalfPI;
          baseToIntermediateCoordinate = Coordinate.NewAtDistanceAngleFrom(baseToIntermediateCoordinate, offsetPixelsInCSUnits, upAngle);

          Annotations.Add(AnnotationForLength(this.CoordinateSystem, baseToIntermediateCoordinate, baseToIntermediateCoordinateAngle, _measureAnnotationStyle, 9, orthogonalLengthInMeters, alignment, 15));
        }

        // Draw the segment End and Orthogonal End Texts
        // The total length will not be displayed for the first segment (same value)
        bool allowLengthTotalUnitsDisplay = !First && (alongLengthInCSUnits > displayAlongSideEndTextMinDistance);

        // The ortho length will be displayed in case 
        // - the orthogonal value itself is displayed (to make sure the values are understood)
        // - it is the first segment or the length of the along-side is long enough (for the first segment, there is nothing 'in the way')
        bool allowOrthoTotalUnitsDisplay = orthogonalLengthInCSUnits > displayOrthogonalEndTextMinOrthogonalDistance && (alongLengthInCSUnits > displayOrthogonalEndTextMinAlongDistance || First);
        if (allowLengthTotalUnitsDisplay || allowOrthoTotalUnitsDisplay)
        {
          // Get the text alignments, by checking in which sub-quadrants (divided in 8) each elements' lies
          // Dependent on these directions, the most appropriate text-alignment will be picked.
          // This kind of behavior should go with quadrant behavior in core. Should be a one-liner.
          var alignments = new TextAlignment[] 
            { 
              TextAlignment.Left, 
              TextAlignment.Left, 
              TextAlignment.Center,
              TextAlignment.Right,
              TextAlignment.Right,
              TextAlignment.Right,
              TextAlignment.Center,
              TextAlignment.Left
            };

          // Do total length, but not for the first dimension-line
          var directAlignment = TextAlignment.Center;
          var orthoAlignment = TextAlignment.Center;

          double angleToUse = MathSE.NormalizedAngle(End.AngleTo(Intermediate));
          double directAngle = MathSE.NormalizedAngle(angleToUse + (Math.PI / 8));
          for (int nr = 0; nr < 8; nr++)
          {
            if (directAngle < ((nr + 1) * Math.PI) / 4)
            {
              directAlignment = alignments[nr];
              orthoAlignment = alignments[(nr + 4) % 8];
              break;
            }
          }

          // Get some extra space/distance from the linears and segment texts
          double extraOrthoDistanceInCSUnits = CSUnitsForPixels(10);
          if (allowLengthTotalUnitsDisplay)
          {
            // Total length
            Curve allSegmentsCurve = new Curve(CoordinateSystem, new LineString(CoordinateSystem, Base.End, Intermediate));
            var allSegmentsLengthInMeters = LiteMapMeasurerBase.LengthInMeters(CoordinateSystem, Transform, Base.End, Intermediate);

            // Place the texts at intersections (cumulative totals at end of segments)
            Coordinate alongTotalCoordinate = Coordinate.NewAtDistanceAngleFrom(Intermediate, extraOrthoDistanceInCSUnits, angleToUse);
            // Annotations.Add(AnnotationFor(this.CoordinateSystem, alongTotalCoordinate, 0.0, _measureAnnotationStyle, 14, allSegmentsLengthInMeters, directAlignment));
          }

          if (allowOrthoTotalUnitsDisplay)
          {
            // Place the 'total' text at the end of each point that is actually dimensioned
            var orthoTotalCoordinate = Coordinate.NewAtDistanceAngleFrom(End, extraOrthoDistanceInCSUnits, angleToUse + Math.PI);
            var orthoTotalLength = alongLengthInMeters + orthogonalLengthInMeters;

            // No-op for now
            // We could use this to display the orto total text
          }
        }
      }
    }

    /// <summary>
    /// A collection of dimension elements
    /// </summary>
    private class DimElementCollection
    {
      #region MyRegion
      /// <summary>
      /// The coordinate system, to be pushed down to individual elements
      /// </summary>
      internal CoordinateSystem CoordinateSystem;

      /// <summary>
      /// The coordinate system, to be pushed down to individual elements
      /// </summary>
      internal ITransform Transform;

      /// <summary>
      /// The base element
      /// </summary>
      private DimBaseElement _baseElement;

      /// <summary>
      /// All measure elements
      /// </summary>
      private List<DimMeasureElement> _measureElements = new List<DimMeasureElement>();

      /// <summary>
      /// The cs units per pixel
      /// </summary>
      private double _csUnitsPerPixel;

      /// <summary>
      /// Is the measure-element finalized
      /// </summary>
      private bool _isFinalized;
      #endregion

      #region API
      /// <summary>
      /// All Curves building up the 
      /// </summary>
      internal IList<FrameworkElement> Geometry()
      {
        // Sets up all geometry
        var geometry = new List<FrameworkElement>();
        if (_baseElement != null)
        {
          foreach (var shape in _baseElement.Curves) geometry.Add(shape);
        }

        foreach (var element in this._measureElements)
        {
          foreach (var shape in element.Annotations) geometry.Add(shape);
        }

        foreach (var element in this._measureElements)
        {
          foreach (var shape in element.Curves) geometry.Add(shape);
        }

        foreach (var element in this._measureElements)
        {
          foreach (var shape in element.Points) geometry.Add(shape);
        }

        return geometry;
      }

      /// <summary>
      /// Reverses the role of the base element
      /// </summary>
      internal void Reverse()
      {
        if (_measureElements.Count == 1)
        {
          var start = _baseElement.Start;
          var end = _baseElement.End;
          var distance = start.DistanceTo(end);
          var angle = start.AngleTo(end);
          var opposite = Coordinate.NewAtDistanceAngleFrom(start, distance * 2, angle);

          _baseElement.Start = opposite;
          _baseElement.RecalculateCurvesAndAnnotations(true);
        }
      }

      /// <summary>
      /// Recalculates the curves and annotations
      /// </summary>
      internal void RecalculateCurvesAndAnnotations()
      {
        _baseElement.RecalculateCurvesAndAnnotations(true);
      }

      /// <summary>
      /// Can a new coordinate be added (is the last measure element valid/not red)
      /// </summary>
      /// <returns></returns>
      internal bool CanAddCoordinate()
      {
        if (_measureElements.Count > 0)
        {
          // Only allowed in case the last measure element is valid
          return _measureElements[_measureElements.Count - 1].Valid || _measureElements.Count == 1;
        }

        // Backstop, in case there are no measure elements yet. In that case, is always valid
        return true;
      }

      /// <summary>
      /// Returns a flag indicating whether there are coordinates
      /// </summary>
      internal bool HasCoordinates
      {
        get { return _measureElements.Count > 0; }
      }

      /// <summary>
      /// Added a coordinate
      /// </summary>
      internal void AddedCoordinate(IList<Coordinate> coordinates, Coordinate lastCoordinate)
      {
        if (coordinates.Count == 1)
        {
          // Create the new dimBaseElement, which holds the direction
          _baseElement = new DimBaseElement() { Start = lastCoordinate, End = lastCoordinate, Transform = this.Transform, CoordinateSystem = this.CoordinateSystem, CSUnitsPerPixel = _csUnitsPerPixel };

          // Force recalculation
          _baseElement.RecalculateCurvesAndAnnotations(false);
          _measureElements.Clear();
        }
        else if (coordinates.Count == 2)
        {
          // Set the base element's last coordinate
          if (_baseElement != null)
          {
            _baseElement.End = lastCoordinate;
            _baseElement.RecalculateCurvesAndAnnotations(true);
          }
          _measureElements.Clear();
        }
        else
        {
          // Add extra elements
          if (_measureElements.Count > 0)
          {
            // Reverse the lot in case the added coordinate actually is invalid
            // (invalid means it is placed before the last coordinate).
            if (_measureElements.Count == 1 && !_measureElements[0].Valid)
            {
              Reverse();
            }

            var last = _measureElements[_measureElements.Count - 1];

            last.End = lastCoordinate;
            last.RecalculateCurvesAndAnnotations(true);
          }

          // Add the new element
          Coordinate lastElementCoordinate = _measureElements.Count > 0 ? _measureElements[_measureElements.Count - 1].Intermediate : _baseElement.End;
          bool first = _measureElements.Count == 0;
          var newMeasureElement = new DimMeasureElement() { Base = _baseElement, Start = lastElementCoordinate, End = lastCoordinate, Transform = this.Transform, CoordinateSystem = this.CoordinateSystem, First = first, CSUnitsPerPixel = _csUnitsPerPixel };
          newMeasureElement.RecalculateCurvesAndAnnotations(true);
          _measureElements.Add(newMeasureElement);
        }
      }

      /// <summary>
      /// Moved a coordinate
      /// </summary>
      internal void MovedCoordinate(IList<Coordinate> coordinates, Coordinate lastCoordinate)
      {
        if (coordinates.Count <= 1)
        {
          // We are moving the base element around
          if (_baseElement != null)
          {
            _baseElement.End = lastCoordinate;
            _baseElement.RecalculateCurvesAndAnnotations(false);
          }
        }
        else
        {
          if (coordinates.Count == 2)
          {
            _baseElement.RecalculateCurvesAndAnnotations(true);
          }

          // 2 Coordinates: 1 Base
          // 3 Coordinates: 1 Dim element
          // 4 Coordinates: 2 Dim elements, etc.
          int coordsCount = coordinates.Count;
          int measureCount = _measureElements.Count;

          if (measureCount < coordsCount - 1)
          {
            // Not enough measure stuff
            Coordinate lastElementCoordinate = _measureElements.Count > 0 ? _measureElements[_measureElements.Count - 1].Intermediate : _baseElement.End;
            bool first = _measureElements.Count == 0;
            var newMeasureElement = new DimMeasureElement() { Base = _baseElement, Start = lastElementCoordinate, End = lastCoordinate, Transform = this.Transform, CoordinateSystem = this.CoordinateSystem, First = first, CSUnitsPerPixel = _csUnitsPerPixel };
            newMeasureElement.RecalculateCurvesAndAnnotations(true);
            _measureElements.Add(newMeasureElement);
          }
          else
          {
            var element = _measureElements[measureCount - 1];
            element.End = lastCoordinate;
            element.RecalculateCurvesAndAnnotations(true);
          }
        }
      }

      /// <summary>
      /// Deleted a coordinate
      /// </summary>
      internal void DeletedCoordinate(IList<Coordinate> coordinates, Coordinate lastCoordinate)
      {
        if (coordinates.Count == 0)
        {
          // No elements left
          _baseElement = null;
          _measureElements.Clear();
        }
        else if (coordinates.Count == 1)
        {
          // Set the base elements last coordinate
          _baseElement.End = lastCoordinate;
          _baseElement.RecalculateCurvesAndAnnotations(false);
          _measureElements.Clear();
        }
        else
        {
          // Remove the last coordinates
          while (_measureElements.Count > coordinates.Count - 1)
          {
            _measureElements.RemoveAt(_measureElements.Count - 1);
          }

          int coordsCount = coordinates.Count;
          int measureCount = _measureElements.Count;

          if (measureCount < coordsCount - 1)
          {
            // Not enough measure stuff
            var newMeasureElement = new DimMeasureElement() { Base = _baseElement, End = lastCoordinate, Transform = this.Transform, CoordinateSystem = this.CoordinateSystem, CSUnitsPerPixel = _csUnitsPerPixel };
            newMeasureElement.RecalculateCurvesAndAnnotations(true);
            _measureElements.Add(newMeasureElement);
          }
          else
          {
            var element = _measureElements[measureCount - 1];
            element.End = lastCoordinate;
            element.RecalculateCurvesAndAnnotations(true);
          }
        }
      }

      /// <summary>
      /// Clear the elements
      /// </summary>
      internal void Clear()
      {
        _baseElement = null;
        _measureElements.Clear();
      }

      /// <summary>
      /// Returns a flag indicating this collection is finalized
      /// </summary>
      internal bool IsFinalized { get { return _isFinalized; } }

      /// <summary>
      /// Called whenever the number of meters per pixel have changed
      /// </summary>
      internal bool OnCSUnitsPerPixelChanged(double csUnitsPerPixel)
      {
        bool changed = false;
        if (Math.Abs(_csUnitsPerPixel - csUnitsPerPixel) > 0)
        {
          _csUnitsPerPixel = csUnitsPerPixel;
          if (_baseElement != null)
          {
            _baseElement.CSUnitsPerPixel = csUnitsPerPixel;
            _baseElement.RecalculateCurvesAndAnnotations(_measureElements.Count > 0);
          }

          foreach (var element in _measureElements)
          {
            element.CSUnitsPerPixel = csUnitsPerPixel;
            element.RecalculateCurvesAndAnnotations(_measureElements.Count > 0);
          }
          changed = true;
        }
        return changed;
      }

      /// <summary>
      /// Returns a flag indicating whether this element is valid
      /// </summary>
      public bool Valid
      {
        get { return _measureElements == null || _measureElements.Count == 0 || _measureElements[_measureElements.Count - 1].Valid; }
      }

      /// <summary>
      /// Remove the last element
      /// </summary>
      internal void FinalizeElements()
      {
        _isFinalized = true;
        if (_measureElements.Count > 1)
        {
          _measureElements.RemoveAt(_measureElements.Count - 1);
        }

        if (_baseElement != null)
        {
          _baseElement.ClearGeometry();
        }
      }
      #endregion

    }
    #endregion

    #region Fields
    /// <summary>
    /// All dimension elements
    /// </summary>
    private List<DimElementCollection> _elements = new List<DimElementCollection>();
    #endregion

    #region Internal
    /// <summary>
    /// Intialize the measurer
    /// </summary>
    protected override void Initialize()
    {
      _elements.Add(new DimElementCollection());
    }
    #endregion

    #region Behavior
    /// <summary>
    /// The active collection
    /// </summary>
    private DimElementCollection ActiveCollection
    {
      get
      {
        return _elements[_elements.Count - 1];
      }
    }

    /// <summary>
    /// Create the feature geometry
    /// </summary>
    private void CreateFeatureGeometry()
    {
      var featureGeometry = new List<FrameworkElement>();

      foreach (var collection in _elements)
      {
        foreach (var geometry in collection.Geometry())
        {
          featureGeometry.Add(geometry);
        }
      }

      this.FeatureGeometry = featureGeometry;
    }

    /// <summary>
    /// Start the dim measuring
    /// </summary>
    internal override void Start()
    {
      base.Start();

      foreach (var collection in _elements)
      {
        collection.OnCSUnitsPerPixelChanged(CSUnitsPerPixel);
      }
    }

    /// <summary>
    /// Add the specified world coordinate 
    /// </summary>
    internal override void AddCoordinate(Coordinate worldCoord)
    {
      if (ActiveCollection.CanAddCoordinate())
      {
        var coordinates = this.Coordinates;

        if (LastCoordinate == null)
        {
          ActiveCollection.CoordinateSystem = this.CoordinateSystem;
          ActiveCollection.Transform = this.PixelToWorldTransform;
          coordinates.Clear();
        }

        var coordinateToAdd = worldCoord.Clone();
        coordinates.Add(coordinateToAdd);

        ActiveCollection.AddedCoordinate(coordinates, coordinateToAdd);
        LastCoordinate = coordinateToAdd;

        CreateFeatureGeometry();
      }
    }

    /// <summary>
    /// Move the last world coordinate to the specified location
    /// </summary>
    internal override void MoveCoordinate(Coordinate worldCoord)
    {
      if (LastCoordinate != null)
      {
        var coordinates = this.Coordinates;

        LastMouseCoordinate = worldCoord.Clone();

        ActiveCollection.MovedCoordinate(coordinates, LastMouseCoordinate);

        CreateFeatureGeometry();
      }
    }

    /// <summary>
    /// Can we move the screen (map)
    /// </summary>
    internal override bool CanMoveScreen
    {
      get { return (_elements.Count == 1 && ActiveCollection != null && !ActiveCollection.IsFinalized); }
    }

    /// <summary>
    /// Move the coordinate
    /// </summary>
    /// <param name="worldCoord"></param>
    internal override void MoveAll(double divX, double divY)
    {
      if (ActiveCollection != null && ActiveCollection.CanAddCoordinate() && ActiveCollection.Valid)
      {
        var oldCoordinates = new List<Coordinate>(Coordinates);

        for (int nr = 0; nr < Coordinates.Count; nr++)
        {
          DeleteLastCoordinate();
        }

        var coordinates = Coordinates;
        coordinates.Clear();

        ActiveCollection.Clear();
        ActiveCollection.CoordinateSystem = this.CoordinateSystem;
        ActiveCollection.Transform = this.PixelToWorldTransform;

        foreach (var coord in oldCoordinates)
        {
          var cd = new Coordinate(coord.X + divX, coord.Y + divY);

          coordinates.Add(cd);

          ActiveCollection.MovedCoordinate(coordinates, cd);
          ActiveCollection.AddedCoordinate(coordinates, cd);
        }

        if (LastMouseCoordinate != null)
        {
          MoveCoordinate(LastMouseCoordinate);
        }

        CreateFeatureGeometry();
      }
    }

    /// <summary>
    /// Callback for deletion of the last coordinate action
    /// </summary>
    internal override void DeleteLastCoordinate()
    {
      if (!ActiveCollection.IsFinalized && LastCoordinate != null)
      {
        var coordinates = Coordinates;
        if (coordinates.Count > 0)
        {
          Coordinate last = LastMouseCoordinate;

          coordinates.RemoveAt(coordinates.Count - 1);

          ActiveCollection.DeletedCoordinate(coordinates, last);

          CreateFeatureGeometry();
        }
      }
    }

    /// <summary>
    /// Close the trail
    /// </summary>
    internal override void CloseMeasurer()
    {
      if (Coordinates.Count == 0)
      {
        // No Coordinates; so not drawing; clear everything
        Clear();
      }
      else if (Coordinates.Count <= 2)
      {
        if (_elements.Count > 1)
        {
          Coordinates.Clear();
          LastCoordinate = null;
          LastMouseCoordinate = null;
          _elements.RemoveAt(_elements.Count - 1);

          var newCollection = new DimElementCollection();
          newCollection.OnCSUnitsPerPixelChanged(this.CSUnitsPerPixel);
          _elements.Add(newCollection);

          CreateFeatureGeometry();
        }
        else
        {
          Clear();
        }
      }
      else
      {
        EndMeasuring();

        Coordinates.Clear();
        LastCoordinate = null;
        LastMouseCoordinate = null;
      }
    }

    /// <summary>
    /// End the measuring
    /// </summary>
    internal override void EndMeasuring()
    {
      base.EndMeasuring();

      ActiveCollection.FinalizeElements();
      CreateFeatureGeometry();

      var newCollection = new DimElementCollection();
      newCollection.OnCSUnitsPerPixelChanged(this.CSUnitsPerPixel);
      _elements.Add(newCollection);
    }

    /// <summary>
    /// Clear the contents
    /// </summary>
    internal override void Clear()
    {
      foreach (var collection in _elements)
      {
        collection.Clear();
      }

      _elements.Clear();

      var newCollection = new DimElementCollection();
      newCollection.OnCSUnitsPerPixelChanged(this.CSUnitsPerPixel);
      _elements.Add(newCollection);

      base.Clear();
    }

    /// <summary>
    /// Callback for changes in units per pixel
    /// </summary>
    internal override void OnCSUnitsPerPixelChanged(double csUnitsPerPixel)
    {
      foreach (var collection in _elements)
      {
        if (collection.OnCSUnitsPerPixelChanged(csUnitsPerPixel))
        {
          CreateFeatureGeometry();
        }
      }
    }
    #endregion

    #region Public Api

    /// <summary>
    /// Gets the resource key describing this mode
    /// </summary>
    public override String DescriptionResourceKey
    {
      get
      {
        return "MapMeasureDimModeDescription";
      }
    }

    #endregion

  }
}
