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
    /// The measurer that is used for measuring in free mode, allowing the user
    /// to place points anywhere and displaying the lengths between those points.
    /// </summary>
    public class LiteMapFreeMeasurerIS : LiteMapMeasurerBase
    {
        #region Static

        /// <summary>
        /// The line style we use when rendering the curves added
        /// </summary>
        private static Brush _lineStyle;

        /// <summary>
        /// The line style we use when rendering the curves added
        /// </summary>
        private static Brush _grayLineStyle;

        /// <summary>
        /// The annotation style we use when rendering the length of a segment
        /// </summary>
        private static Brush _segmentAnnotationStyle;

        /// <summary>
        /// The annotation style we use when rendering the length of a segment
        /// </summary>
        private static Brush _totalAnnotationStyle;

        /// <summary>
        /// The annotation style we use when rendering the length of a segment
        /// </summary>
        private static Brush _areaAnnotationStyle;

        static LiteMapFreeMeasurerIS()
        {
            // The styles
            _lineStyle = new SolidColorBrush(Colors.Red);
            _grayLineStyle = new SolidColorBrush(Colors.Red);
            _segmentAnnotationStyle = new SolidColorBrush(Colors.Orange);
            _totalAnnotationStyle = new SolidColorBrush(Colors.Orange);
            _areaAnnotationStyle = new SolidColorBrush(Colors.Orange);
        }
        #endregion

        #region Internal Class
        /// <summary>
        /// A private helper class, holding one measured element
        /// </summary>
        private class FreeElement
        {
            /// <summary>
            /// The coordinate system, to be pushed down to individual elements
            /// </summary>
            internal CoordinateSystem CoordinateSystem;

            /// <summary>
            /// The coordinate system, to be pushed down to individual elements
            /// </summary>
            internal ITransform Transform;

            /// <summary>
            /// The start coordinate (start of base, or intermediate point of previous measure element)
            /// </summary>
            internal List<Coordinate> _coordinates = new List<Coordinate>();

            /// <summary>
            /// The curves (output)
            /// </summary>
            internal List<FrameworkElement> _geometry = new List<FrameworkElement>();

            /// <summary>
            /// The CS Units per pixel
            /// </summary>
            internal double _csUnitsPerPixel;

            /// <summary>
            /// Returns the number of units per pixel
            /// </summary>
            protected double CSUnitsForPixels(int pixels)
            {
                return pixels; // *CSUnitsPerPixel;
            }

            /// <summary>
            /// Are we completely done measuring
            /// </summary>
            private bool _isFinalized;

            /// <summary>
            /// All Curves building up the 
            /// </summary>
            internal IList<FrameworkElement> Geometry()
            {
                return _geometry;
            }

            internal void RecalculateCurvesAndAnnotations()
            {
                // Display the Along-side segment text (in direction of elements)
                double displayAlongSideTextMinDistance = CSUnitsForPixels(40);
                double displayTotalTextMinDistance = CSUnitsForPixels(60);
                double totalAlongLengthInMeters = 0.0;

                // The offset in pixels above the lines
                double offsetPixelsInCSUnits = CSUnitsForPixels(2);
                double totalTextOffsetPixelsInCSUnits = CSUnitsForPixels(15);

                _geometry.Clear();

                for (int nr = 0; nr < _coordinates.Count - 1; nr++)
                {
                    var isLast = nr >= _coordinates.Count - 2;
                    var style = _isFinalized || !isLast ? _lineStyle : _grayLineStyle;
                    var fromCoordinate = _coordinates[nr];
                    var toCoordinate = _coordinates[nr + 1];

                    var line = new Line { X1 = fromCoordinate.X, Y1 = fromCoordinate.Y, X2 = toCoordinate.X, Y2 = toCoordinate.Y, Stroke = style };
                    _geometry.Add(line);

                    var alongLengthInCSUnits = fromCoordinate.DistanceTo(toCoordinate);
                    
                    //Código genera las distancias
                    var alongLengthInMeters = LiteMapMeasurerBase.LengthInMeters(CoordinateSystem, Transform, fromCoordinate, toCoordinate);

                    totalAlongLengthInMeters += alongLengthInMeters;

                    if (alongLengthInCSUnits > displayAlongSideTextMinDistance)
                    {
                        //No son necesarias las distancias
                        /*
                        var baseToIntermediateCoordinate = fromCoordinate.Middle(toCoordinate);
                        double baseToIntermediateCoordinateAngle = MathSE.NormalizedAngle(fromCoordinate.AngleTo(toCoordinate));

                        //TextAlignment alignment = TextAlignment.Bottom;
                        TextAlignment alignment = TextAlignment.Center;

                        if (baseToIntermediateCoordinateAngle > MathSE.HalfPI && baseToIntermediateCoordinateAngle < 3 * MathSE.HalfPI)
                        {
                            baseToIntermediateCoordinateAngle += MathSE.PI;
                        }

                        // Get the text slightly up (couple of pixels), to make sure it is drawn loose from the line
                        double upAngle = baseToIntermediateCoordinateAngle + MathSE.HalfPI;
                        baseToIntermediateCoordinate = Coordinate.NewAtDistanceAngleFrom(baseToIntermediateCoordinate, offsetPixelsInCSUnits, upAngle);

                        
                        _geometry.Add(AnnotationFor(this.CoordinateSystem, baseToIntermediateCoordinate, baseToIntermediateCoordinateAngle, _segmentAnnotationStyle, 9, alongLengthInMeters, alignment, 15));


                        //Este código genera los totales de la linea, se comento por que no se requiere
                        if (alongLengthInCSUnits > displayTotalTextMinDistance)
                        {

                            // Now do the total Length at the coordinates themselves (but within the loop, so only when length of previous is long enough)
                            if (isLast && !toCoordinate.Equals(_coordinates[0]))
                            {
                                // If this is the last coordinate; just place the thingie at a decent offset
                                var direction = fromCoordinate.AngleTo(toCoordinate);
                                var position = Coordinate.NewAtDistanceAngleFrom(toCoordinate, totalTextOffsetPixelsInCSUnits, direction);

                                _geometry.Add(AnnotationFor(this.CoordinateSystem, position, 0.0, _totalAnnotationStyle, 9, totalAlongLengthInMeters, alignment, 0));
                            }
                            else
                            {
                                // Not the last coordinate - so we need to calculate a decent position for the coordinate
                                // at the intersection of the two segments it is placed between
                                var nextCoordinate = isLast ? _coordinates[1] : _coordinates[nr + 2];

                                // Only place the lot, when the next coordinate is suffiently away as well
                                var alongLengthNextInCSUnits = toCoordinate.DistanceTo(nextCoordinate);
                                if (alongLengthNextInCSUnits > displayTotalTextMinDistance)
                                {
                                    var angleBetween = (fromCoordinate - toCoordinate).AngleWith(nextCoordinate - toCoordinate);
                                    int extra = 25;
                                    // Make sure we get the right direction
                                    if (nextCoordinate.SideOf(fromCoordinate, toCoordinate) == GeometrySide.Left)
                                    {
                                        angleBetween = -angleBetween;
                                        extra = 0;
                                    }

                                    var direction = fromCoordinate.AngleTo(toCoordinate) + (angleBetween / 2.0);
                                    var position = Coordinate.NewAtDistanceAngleFrom(toCoordinate, totalTextOffsetPixelsInCSUnits, direction);

                                    _geometry.Add(AnnotationFor(this.CoordinateSystem, position, 0.0, _totalAnnotationStyle, 9, totalAlongLengthInMeters, alignment, extra));
                                }
                            }
                        }*/


                    }
                } // For-loop with coords

                if (_coordinates.Count > 2)
                {
                    // We could display the area of the measurer
                }
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

                    RecalculateCurvesAndAnnotations();
                    changed = true;
                }
                return changed;
            }

            /// <summary>
            /// Can we add a coordinate
            /// </summary>
            /// <returns></returns>
            internal bool CanAddCoordinate()
            {
                return true;
            }

            /// <summary>
            /// Do we have coordinates
            /// </summary>
            internal bool HasCoordinates
            {
                get { return _coordinates.Count > 0; }
            }

            /// <summary>
            /// Added a coordinate
            /// </summary>
            internal void AddedCoordinate(IList<Coordinate> coordinates, Coordinate lastCoordinate)
            {
                // Add the coordinate
                _coordinates.Add(new Coordinate(lastCoordinate.X, lastCoordinate.Y));

                if (_coordinates.Count == 1)
                {
                    // Make sure we've got a line straight away
                    _coordinates.Add(new Coordinate(lastCoordinate.X, lastCoordinate.Y));
                }

                RecalculateCurvesAndAnnotations();
            }

            /// <summary>
            /// Moved a coordinate
            /// </summary>
            internal void MovedCoordinate(IList<Coordinate> coordinates, Coordinate lastCoordinate)
            {
                // Move the last coordinate
                if (_coordinates.Count > 1)
                {
                    var myLast = _coordinates[_coordinates.Count - 1];
                    if (lastCoordinate != null)
                    {
                        myLast.X = lastCoordinate.X;
                        myLast.Y = lastCoordinate.Y;
                    }
                }

                RecalculateCurvesAndAnnotations();
            }

            /// <summary>
            /// Moved a coordinate
            /// </summary>
            internal void MovedAll(IList<Coordinate> coordinates, Coordinate lastCoordinate)
            {
                try
                {
                    for (int nr = 0; nr < coordinates.Count; nr++)
                    {
                        _coordinates[nr] = coordinates[nr];
                    }
                }
                catch { }
                MovedCoordinate(coordinates, lastCoordinate);
            }

            /// <summary>
            /// Deleted a coordinate
            /// </summary>
            internal void DeletedCoordinate(IList<Coordinate> coordinates, Coordinate lastCoordinate)
            {
                // Deleted the last coordinate
                if (_coordinates.Count == 2)
                {
                    _coordinates.Clear();
                }
                else if (_coordinates.Count > 0)
                {
                    _coordinates.RemoveAt(_coordinates.Count - 1);
                }

                RecalculateCurvesAndAnnotations();
            }

            /// <summary>
            /// Remove the last element
            /// </summary>
            internal void FinalizeElements()
            {
                if (!_isFinalized)
                {

                    _isFinalized = true;

                    if (_coordinates.Count > 2)
                    {
                        _coordinates.RemoveAt(_coordinates.Count - 1);
                    }

                    // Maybe remove the last one?
                    RecalculateCurvesAndAnnotations();
                }
            }

            /// <summary>
            /// Close the element
            /// </summary>
            internal void Close()
            {
                if (!_isFinalized)
                {
                    _isFinalized = true;

                    if (_coordinates.Count > 3)
                    {
                        _coordinates[_coordinates.Count - 1] = _coordinates[0];
                    }

                    RecalculateCurvesAndAnnotations();
                }
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// All dimension elements
        /// </summary>
        private List<FreeElement> _elements = new List<FreeElement>();
        #endregion

        #region ABC Implementation
        /// <summary>
        /// Initialize the measurer by creating a new list of elements
        /// </summary>
        protected override void Initialize()
        {
            _elements.Add(new FreeElement());
        }
        /// <summary>
        /// The active collection
        /// </summary>
        private FreeElement ActiveCollection
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
        /// Start the measurer
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
        /// Add a world coordinate to the measurer
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
        /// Move the last coordinate added
        /// </summary>
        /// <param name="worldCoord">The new position</param>
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
        /// Can we move the screen (the map)
        /// </summary>
        internal override bool CanMoveScreen
        {
            get { return (_elements.Count == 1 && ActiveCollection != null && ActiveCollection.HasCoordinates && ActiveCollection.CanAddCoordinate() && !ActiveCollection.IsFinalized); }
        }

        /// <summary>
        /// Move the coordinate
        /// </summary>
        internal override void MoveAll(double divX, double divY)
        {
            if (ActiveCollection.CanAddCoordinate() && !ActiveCollection.IsFinalized)
            {
                if (ActiveCollection != null && Coordinates.Count > 0)
                {
                    ActiveCollection.Transform = this.PixelToWorldTransform;

                    for (int nr = 0; nr < Coordinates.Count; nr++)
                    {
                        var cd = Coordinates[nr];
                        Coordinates[nr] = new Coordinate(cd.X + divX, cd.Y + divY);
                    }

                    ActiveCollection.MovedAll(Coordinates, LastMouseCoordinate);
                    CreateFeatureGeometry();
                }
            }
        }

        /// <summary>
        /// Deletes the last coordinate
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
        /// Closes the measurer
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

                    var element = new FreeElement();
                    element.OnCSUnitsPerPixelChanged(this.CSUnitsPerPixel);
                    _elements.Add(element);

                    CreateFeatureGeometry();
                }
                else
                {
                    Clear();
                }
            }
            else
            {
                ActiveCollection.Close();
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

            var element = new FreeElement();
            element.OnCSUnitsPerPixelChanged(this.CSUnitsPerPixel);
            _elements.Add(element);
        }

        /// <summary>
        /// Clear the contents
        /// </summary>
        internal override void Clear()
        {
            _elements.Clear();

            var element = new FreeElement();
            element.OnCSUnitsPerPixelChanged(this.CSUnitsPerPixel);
            _elements.Add(element);

            base.Clear();
        }

        /// <summary>
        /// Handles the change of the units per pixel
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
    }
}
