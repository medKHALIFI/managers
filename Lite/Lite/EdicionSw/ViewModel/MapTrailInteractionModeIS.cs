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

using Microsoft.Practices.ServiceLocation;
using SpatialEye.Framework.Geometry.Services;

namespace Lite
{
    /// <summary>
    /// The default interaction mode for Lite, keeping track of the last pressed world location
    /// as an example on how to keep track of mouse events.
    /// </summary>
    public class MapTrailInteractionModeIS : SpatialEye.Framework.Client.MapInteractionMode
    {
        /// <summary>
        /// The retained information for button down events
        /// </summary>
        private MapMouseEventArgs _mapButtonDownArgs;
        public const string LatitudPropertyName = "StrLatitud";
        public const string LongitudPropertyName = "StrLongitud";

        private string _strLatitud = string.Empty;
        private string _strLongitud = string.Empty;

        public MapTrailISViewModel _viewModel;

        /// <summary>
        /// The free measurer
        /// </summary>
        private LiteMapFreeMeasurerIS _freeMeasurer;

        /// <summary>
        /// The dimensioning measurer
        /// </summary>
        private LiteMapDimMeasurer _dimensioningMeasurer;

        /// <summary>
        /// The active measurer
        /// </summary>
        private LiteMapMeasurerBase _activeMeasurer;

        private MapInteractionLayer _interactionLayer;

        public double _lastMouseMoveX, _lastMouseMoveY;
        public double _mouseDownX, _mouseDownY;
          #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        public MapTrailInteractionModeIS(MapTrailISViewModel viewModel)
        {
            _viewModel = viewModel;
            ImageCursor = MapInteractionModeImageCursor.Measure;
            //ImageCursor = MapInteractionModeImageCursor.MeasureDimension;

            // Construct the measurers
            _dimensioningMeasurer = new LiteMapDimMeasurer();
            _freeMeasurer = new LiteMapFreeMeasurerIS();

            // Set a start measurer
            Measurer = _dimensioningMeasurer;
        }
        #endregion

        /*
        #region Set Mode
        /// <summary>
        /// Sets the active measurer for the interaction mode
        /// </summary>
        /// <param name="dimensioning">A flag specifying whether the dimensioning measurer should be set</param>
        internal void SetMeasurer(bool dimensioning)
        {
            ImageCursor = MapInteractionModeImageCursor.MeasureDimension;
        }
        #endregion
        */

        #region Set Mode
        /// <summary>
        /// Sets the active measurer for the interaction mode
        /// </summary>
        /// <param name="dimensioning">A flag specifying whether the dimensioning measurer should be set</param>
        internal void SetMeasurer(bool dimensioning)
        {
            if (dimensioning)
            {
                // Set the dimensioning measurer
                ImageCursor = MapInteractionModeImageCursor.MeasureDimension;
                Measurer = _dimensioningMeasurer;
            }
            else
            {
                // Set the free measurer
                ImageCursor = MapInteractionModeImageCursor.Measure;
                Measurer = _freeMeasurer;
            }
        }

        /// <summary>
        /// Holds the active measurer 
        /// </summary>
        private LiteMapMeasurerBase Measurer
        {
            get
            {
                return _activeMeasurer;
            }
            set
            {
                if (_activeMeasurer != value)
                {
                    if (_activeMeasurer != null)
                    {
                        // Stop watching the active measurer's geometry changes
                        _activeMeasurer.FeatureGeometryChanged -= MeasurerFeatureGeometryChanged;
                    }

                    _activeMeasurer = value;
                    IsMovingMap = true;

                    if (_activeMeasurer != null)
                    {
                        // Start handling the new measurer's geometry changes
                        _activeMeasurer.FeatureGeometryChanged += MeasurerFeatureGeometryChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Handler for changes in the active measurer's geometry
        /// </summary>
        /// <param name="sender">The sender of the changed geometry</param>
        /// <param name="geometry">The new geometry</param>
        void MeasurerFeatureGeometryChanged(LiteMapMeasurerBase sender, System.Collections.Generic.List<FrameworkElement> geometry)
        {
            if (_interactionLayer != null)
            {
                _interactionLayer.Clear();

                foreach (var element in geometry)
                {
                    _interactionLayer.Add(element);
                }
            }
        }
        #endregion

        /*
        #region Starting/Stopping
        /// <summary>
        /// The interaction mode has started
        /// </summary>
        /// <param name="modifierKeys">The modifier keys that could have led to starting the mode</param>
        protected override void OnStarted(System.Windows.Input.ModifierKeys modifierKeys)
        {
            base.OnStarted(modifierKeys);
        }

        /// <summary>
        /// This interaction mode has stopped; clears the active measurer
        /// </summary>
        protected override void OnStopped()
        {
            base.OnStopped();

            //_activeMeasurer.Clear();
        }
        #endregion
        */

        #region Modifier Keys
        /// <summary>
        /// Returns true, indicating that this mode uses the specified modifier keys.
        /// This makes sure no other interaction mode will kick in (because of modifier
        /// keys) when this mode is active.
        /// </summary>
        /// <param name="modifierKeys">The modifier keys pressed</param>
        /// <returns>A flag specifying whether this mode uses the specified keys</returns>
        protected override bool UsesModifierKeys(ModifierKeys modifierKeys)
        {
            // Let no other mode kick in
            return true;
        }


        #endregion

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
        protected override async void OnMouseLeftButtonUp(MapViewModel sender, MapMouseEventArgs args)
        {
            /*MapMouseEventArgs resultArgs = null;
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

            base.OnMouseLeftButtonUp(map, args);*/


            _interactionLayer = args.InteractionLayer;


            // By default, add the coordinate if the mouse was not down
            bool addCoordinate = !IsMouseDown;

            if (IsMouseDown)
            {
                // Only add the coordinate, when there is enough spacing between coordinates
                addCoordinate = Math.Abs(_lastMouseMoveX - _mouseDownX) < 3 && Math.Abs(_lastMouseMoveY - _mouseDownY) < 3;
            }

            // Now, actually add the coordinate
            if (addCoordinate)
            {
                //_activeMeasurer.AddCoordinate(new SpatialEye.Framework.Geometry.Coordinate(args.X, args.Y));

                //_interactionLayer.Clear();
                //_geometry.Clear();

                SpatialEye.Framework.Geometry.Coordinate co = new SpatialEye.Framework.Geometry.Coordinate(args.X, args.Y);

                //Brush estilo = new SolidColorBrush(Colors.Red);

                //_geometry.Add(AnnotationFor(Map.CoordinateSystem, co, 0.0, estilo, 20, 0, TextAlignment.Center, 0));

                var geometryService = ServiceLocator.Current.GetInstance<IGeometryService>();

                SpatialEye.Framework.Geometry.CoordinateSystems.EpsgCoordinateSystemReference WGS84CoordinateSystem = new SpatialEye.Framework.Geometry.CoordinateSystems.EpsgCoordinateSystemReference { SRId = 4326, Name = "WGS 84" };
                Map.PixelToWorldTransform.Convert(co);
                //var co2 = await GeometryManager.Instance.TransformAsync(co, Map.CoordinateSystem.EPSGCode, WGS84CoordinateSystem.SRId);
                var co2 = geometryService.TransformAsync(co, Map.CoordinateSystem.EPSGCode, WGS84CoordinateSystem.SRId).Result;

                _strLatitud = (co2.Y).ToString("00.0000000");
                _strLongitud = (co2.X).ToString("00.0000000");


                //Si es el primer trazo
                if (_viewModel.Latitud == string.Empty && _viewModel.Longitud == string.Empty)
                {
                    _viewModel.Latitud = _strLatitud.Replace(",",".");
                    _viewModel.Longitud = _strLongitud.Replace(",",".");
                }
                else
                {
                    _viewModel.Latitud = _viewModel.Latitud + "," + _strLatitud.Replace(",",".");
                    _viewModel.Longitud = _viewModel.Longitud + "," + _strLongitud.Replace(",",".");

                }


                /*
                foreach (var element in _geometry)
                {
                    _interactionLayer.Add(element);
                }*/


            }

            // Mouse is no longer down
            IsMouseDown = false;

            // We have handled the event
            args.Handled = true;
            base.OnMouseLeftButtonUp(sender, args);
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

        #region Properties
        /// <summary>
        /// A flag indicating whether the mouse is pressed
        /// </summary>
        internal bool IsMouseDown;

        /// <summary>
        /// A flag indicating whether the map is being moved
        /// </summary>
        internal bool IsMovingMap;

        /// <summary>
        /// Holds the top margin of the map where the measurer should not be active.
        /// This allows for extra overlap of the map bar
        /// </summary>
        internal int TopMargin { get; set; }

        /// <summary>
        /// Holds the bottom margin of the map where the measurer should not be active.
        /// </summary>
        internal int BottomMargin { get; set; }

        /// <summary>
        /// Holds the left margin of the map where the measurer should not be active.
        /// </summary>
        internal int LeftMargin { get; set; }

        /// <summary>
        /// Holds the right margin of the map where the measurer should not be active.
        /// </summary>
        internal int RightMargin { get; set; }

        public string strLatitud
        {
            get { return _strLatitud; }
            set
            {
                if (value != _strLatitud)
                {
                    _strLatitud = value;
                    RaisePropertyChanged(LatitudPropertyName);
                }
            }
        }
        public string strLongitud
        {
            get { return _strLongitud; }
            set
            {
                if (value != _strLongitud)
                {
                    _strLongitud = value;
                    RaisePropertyChanged(LongitudPropertyName);
                }
            }
        }
        #endregion

    }
}
