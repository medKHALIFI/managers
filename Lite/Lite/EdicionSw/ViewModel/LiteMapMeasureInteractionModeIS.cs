using System;
using System.Windows;
using System.Windows.Input;

using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Client;


using SpatialEye.Framework.Maps;
using SpatialEye.Framework.Geometry.Services;
using SpatialEye.Framework;
using System.Collections.Generic;
using System.Windows.Controls;
using SpatialEye.Framework.Units;

using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Practices.ServiceLocation;

namespace Lite
{
    /// <summary>
    /// The interaction mode for measuring on the map. Is capable of handling
    /// multiple measurers that do the actual work. One of the measurers will
    /// be active.
    /// </summary>
    public class LiteMapMeasureInteractionModeIS : SpatialEye.Framework.Client.MapInteractionMode
    {
        #region Fields

        public const string LatitudPropertyName = "StrLatitud";
        public const string LongitudPropertyName = "StrLongitud";

        private string _strLatitud = string.Empty;
        private string _strLongitud = string.Empty;
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

        /// <summary>
        /// The interaction layer to draw on
        /// </summary>
        private MapInteractionLayer _interactionLayer;

        /// <summary>
        /// Holds the last position of the mouse
        /// </summary>
        public double _lastMouseMoveX, _lastMouseMoveY;

        public LiteMapMeasureISViewModel _viewModel;

        /// <summary>
        /// Holds the position of the mouse for the pressed event
        /// </summary>
        public double _mouseDownX, _mouseDownY;
        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        public LiteMapMeasureInteractionModeIS(LiteMapMeasureISViewModel viewModel)
        {
            _viewModel = viewModel;
            // Sets the active image cursor of the interaction mode
            ImageCursor = MapInteractionModeImageCursor.MeasureDimension;

            // Construct the measurers
            _dimensioningMeasurer = new LiteMapDimMeasurer();
            _freeMeasurer = new LiteMapFreeMeasurerIS();

            // Set a start measurer
            Measurer = _dimensioningMeasurer;
        }
        #endregion

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

        #region Starting/Stopping
        /// <summary>
        /// The interaction mode has started
        /// </summary>
        /// <param name="modifierKeys">The modifier keys that could have led to starting the mode</param>
        protected override void OnStarted(System.Windows.Input.ModifierKeys modifierKeys)
        {
            base.OnStarted(modifierKeys);

            // Pick up the active coordinate system of the map
            _activeMeasurer.CoordinateSystem = Map.CoordinateSystem;
            _activeMeasurer.PixelToWorldTransform = Map.PixelToWorldTransform;
        }

        /// <summary>
        /// This interaction mode has stopped; clears the active measurer
        /// </summary>
        protected override void OnStopped()
        {
            base.OnStopped();

            _activeMeasurer.Clear();
        }
        #endregion

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

        #region Interaction
        /// <summary>
        /// Handler for key pressed events
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The keyEvent args</param>
        protected override void OnKeyDown(MapViewModel sender, MapKeyEventArgs args)
        {
            // Default behavior
            base.OnKeyDown(sender, args);

            // Get the keys
            var keys = args.Key;

            // Act upon the different keys
            if (keys == Key.C)
            {
                // C for Close
                _activeMeasurer.CloseMeasurer();
                args.Handled = true;
            }
            else if (keys == Key.Enter)
            {
                // End the measurer
                _activeMeasurer.EndMeasuring();
                args.Handled = true;
            }
            else if (keys == Key.Delete || keys == Key.Back)
            {
                // Remove the last coordinate of the active measurer
                _activeMeasurer.DeleteLastCoordinate();
                args.Handled = true;

                /////////////////////////
                //Si se dehizo el ultimo trazo eliminar la ultima coordenada pulsada y que se almacena en las _str

                
                //Deshacer las Latitudes
                int intUltimaLat = _viewModel.Latitud.LastIndexOf(',');
                if (intUltimaLat != -1)
                {
                    int sizeUltimaLat = _viewModel.Latitud.Length;
                    string strUltimaLat = _viewModel.Latitud;
                    _viewModel.Latitud = strUltimaLat.Remove(intUltimaLat, (sizeUltimaLat - intUltimaLat));
                }
                else
                {
                    _viewModel.Latitud = string.Empty;
                }


                //Deshacer las Longitudes
                int intUltimaLon = _viewModel.Longitud.LastIndexOf(',');
                if (intUltimaLon != -1)
                {
                    int sizeUltimaLon = _viewModel.Longitud.Length;
                    string strUltimaLon = _viewModel.Longitud;
                    _viewModel.Longitud = strUltimaLon.Remove(intUltimaLon, (sizeUltimaLon - intUltimaLon));
                }
                else
                {
                    _viewModel.Longitud = string.Empty;
                }

            /////////////////////


            }
        }

        /// <summary>
        /// Handler for mouse leftButton down events; records the active state of the mouse
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event arguments</param>
        protected override void OnMouseLeftButtonDown(MapViewModel sender, MapMouseEventArgs args)
        {
            _interactionLayer = args.InteractionLayer;

            if (_activeMeasurer.CanMoveScreen)
            {
                IsMouseDown = true;
                _mouseDownX = args.X;
                _mouseDownY = args.Y;
                _lastMouseMoveX = args.X;
                _lastMouseMoveY = args.Y;
            }
        }

        /// <summary>
        /// Handler for mouse move events; notifies the active measurer of this
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event arguments</param>
        protected override void OnMouseMove(MapViewModel sender, MapMouseEventArgs args)
        {
            _interactionLayer = args.InteractionLayer;

            if (IsMouseDown)
            {
                // In case the mouse is down (and we are moving), we need
                // to move all coordinates of the active measurer
                if (_activeMeasurer != null)
                {
                    var diffX = args.X - _lastMouseMoveX;
                    var diffY = args.Y - _lastMouseMoveY;

                    _activeMeasurer.MoveAll(diffX, diffY);

                    _lastMouseMoveX = args.X;
                    _lastMouseMoveY = args.Y;
                }
            }

            var mouseX = args.X;
            var mouseY = args.Y;

            // Now notify the measurer of the move
            _activeMeasurer.MoveCoordinate(new SpatialEye.Framework.Geometry.Coordinate(args.X, args.Y));

            // In case we are near the boundary of the (physical) map, we can automatically
            // move the map to make space for some new measure points
            int testPixels = 10;
            double spacingFactor = 10.0;
            if (_activeMeasurer.CanMoveScreen & !IsMovingMap)
            {
                bool doMove = false;
                double measurerToRight = 0.0;
                double measurerToBottom = 0.0;
                double measurerXDistance = 0.0;
                double measurerYDistance = 0.0;

                if (mouseX < testPixels + LeftMargin)
                {
                    // Near the left of the Map, move Map to the Right
                    doMove = true;
                    measurerToRight = sender.DeviceWidth / spacingFactor;
                    measurerXDistance = -sender.Envelope.Width / spacingFactor;
                }
                else if (mouseY < testPixels + TopMargin)
                {
                    // Near the top of the Map, move Map to the Bottom
                    doMove = true;
                    measurerToBottom = sender.DeviceHeight / spacingFactor;
                    measurerYDistance = sender.Envelope.Height / spacingFactor;
                }
                else if (mouseX >= sender.DeviceWidth - testPixels - RightMargin)
                {
                    // Near the right of the Map, move Map to the Left
                    doMove = true;
                    measurerToRight = -sender.DeviceWidth / spacingFactor;
                    measurerXDistance = sender.Envelope.Width / spacingFactor;
                }
                else if (mouseY >= sender.DeviceHeight - testPixels - BottomMargin)
                {
                    // Near the bottom of the Map, map to the Top
                    doMove = true;
                    measurerToBottom = -sender.DeviceHeight / spacingFactor;
                    measurerYDistance = -sender.Envelope.Height / spacingFactor;
                }

                if (doMove)
                {
                    // Move the lot
                    IsMovingMap = true;
                    _activeMeasurer.MoveAll(measurerToRight, measurerToBottom);
                    sender.SetCentre(new Coordinate(sender.Centre.X + measurerXDistance, sender.Centre.Y + measurerYDistance), false);
                }
                else if (!sender.IsAnimating)
                {
                    IsMovingMap = false;
                }
            }
            else if (!sender.IsAnimating)
            {
                IsMovingMap = false;
            }

            args.Handled = !IsMouseDown;
            base.OnMouseMove(sender, args);
        }

        /// <summary>
        /// The mouse enters the Map
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event args</param>
        protected override void OnMouseEnter(MapViewModel sender, MapMouseEventArgs args)
        {
            base.OnMouseEnter(sender, args);
            IsMouseDown = false;
        }

        /// <summary>
        /// The mouse leaves the Map
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event args</param>
        protected override void OnMouseLeave(MapViewModel sender, MapMouseEventArgs args)
        {
            base.OnMouseLeave(sender, args);
            IsMouseDown = false;
        }

        /// <summary>
        /// The mouse leftButton is up
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event args</param>
        /*
        protected override void OnMouseLeftButtonUp(MapViewModel sender, MapMouseEventArgs args)
        {
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
                _activeMeasurer.AddCoordinate(new SpatialEye.Framework.Geometry.Coordinate(args.X, args.Y));
            }

            // Mouse is no longer down
            IsMouseDown = false;

            // We have handled the event
            args.Handled = true;
            base.OnMouseLeftButtonUp(sender, args);
        }
        */
        protected override async void OnMouseLeftButtonUp(MapViewModel sender, MapMouseEventArgs args)
        {
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
                _activeMeasurer.AddCoordinate(new SpatialEye.Framework.Geometry.Coordinate(args.X, args.Y));

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
        /// Handle the double click event
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event args</param>
        protected override void OnMouseLeftButtonDoubleClick(MapViewModel sender, MapMouseEventArgs args)
        {
            base.OnMouseLeftButtonDoubleClick(sender, args);

            // Stop measuring
            _activeMeasurer.EndMeasuring();

            

            // We've handled the double click event
            args.Handled = true;
        }

        /// <summary>
        /// Handle the mouse wheel event
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event args</param>
        protected override void OnMouseWheel(MapViewModel sender, MapMouseWheelEventArgs args)
        {
            base.OnMouseWheel(sender, args);

            // Eat the event - we do not allow zooming or anything else to be carried out
            // by other handler
            args.Handled = true;
        }
        #endregion

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


        #region Touch
        /// <summary>
        /// Entering touch events
        /// </summary>
        protected override void OnTouchEnter(MapViewModel sender, MapTouchEventArgs args)
        {
            OnMouseEnter(sender, new MapMouseEventArgs(args));
            args.Handled = true;
        }

        /// <summary>
        /// Leaving touch
        /// </summary>
        protected override void OnTouchLeave(MapViewModel sender, MapTouchEventArgs args)
        {
            OnMouseLeave(sender, new MapMouseEventArgs(args));
        }

        /// <summary>
        /// Touch Down event
        /// </summary>
        protected override void OnTouchDown(MapViewModel sender, MapTouchEventArgs args)
        {
            // Simulate a move
            var subArgs = new MapMouseEventArgs(args);
            OnMouseLeftButtonDown(sender, subArgs);
            args.Handled = subArgs.Handled;
        }

        /// <summary>
        /// Touch Up event
        /// </summary>
        protected override void OnTouchUp(MapViewModel sender, MapTouchEventArgs args)
        {
            var subArgs = new MapMouseEventArgs(args);
            OnMouseLeftButtonUp(sender, subArgs);
            args.Handled = subArgs.Handled;
        }

        /// <summary>
        /// Touch Double Tap
        /// </summary>
        protected override void OnTouchDoubleTap(MapViewModel sender, MapTouchEventArgs args)
        {
            var subArgs = new MapMouseEventArgs(args);
            this.OnMouseLeftButtonDoubleClick(sender, subArgs);
            args.Handled = subArgs.Handled;
        }
        #endregion
    }
}
