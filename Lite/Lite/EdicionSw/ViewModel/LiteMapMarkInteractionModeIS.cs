using System;
using System.Windows;
using System.Windows.Input;
using System.Globalization;
using System.Linq;
using SpatialEye.Framework.ServiceProviders;
using Microsoft.Practices.ServiceLocation;

using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Geometry.Services;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Maps;

using SpatialEye.Framework;
using System.Collections.Generic;
using System.Windows.Controls;
using SpatialEye.Framework.Units;

using System.Windows.Media;
using System.Windows.Shapes;



namespace Lite
{
    public class LiteMapMarkInteractionModeIS : SpatialEye.Framework.Client.MapInteractionMode
    {
        #region Fields
        //public const string LatitudPropertyName = "StrLatitud";
        //public const string LongitudPropertyName = "StrLongitud";

        private string _strLatitud = string.Empty;
        private string _strLongitud = string.Empty;
        private MapInteractionLayer _interactionLayer;
        internal List<FrameworkElement> _geometry = new List<FrameworkElement>();

        /*
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
        */

        /// <summary>
        /// Holds the last position of the mouse
        /// </summary>
        public double _lastMouseMoveX, _lastMouseMoveY;

        public LiteMapMarkISViewModel _viewModel;
        /// <summary>
        /// Holds the position of the mouse for the pressed event
        /// </summary>
        public double _mouseDownX, _mouseDownY;
        #endregion

        #region Constructor
        /// <summary>
        /// The default constructor
        /// </summary>
        public LiteMapMarkInteractionModeIS(LiteMapMarkISViewModel viewModel)
        {
            _viewModel = viewModel;
            ImageCursor = MapInteractionModeImageCursor.Measure;
            //ImageCursor = MapInteractionModeImageCursor.MeasureDimension;
        }
        #endregion

      

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
        /// Handler for mouse leftButton down events; records the active state of the mouse
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event arguments</param>
        protected override void OnMouseLeftButtonDown(MapViewModel sender, MapMouseEventArgs args)
        {
            IsMouseDown = true;
            _mouseDownX = args.X;
            _mouseDownY = args.Y;
            _lastMouseMoveX = args.X;
            _lastMouseMoveY = args.Y;

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

                _interactionLayer.Clear();
                _geometry.Clear();

                SpatialEye.Framework.Geometry.Coordinate co = new SpatialEye.Framework.Geometry.Coordinate(args.X, args.Y);
                Brush estilo = new SolidColorBrush(Colors.Red);
                _geometry.Add(AnnotationFor(Map.CoordinateSystem, co, 0.0, estilo, 20, 0, TextAlignment.Center, 0));

                var geometryService = ServiceLocator.Current.GetInstance<IGeometryService>();

                SpatialEye.Framework.Geometry.CoordinateSystems.EpsgCoordinateSystemReference WGS84CoordinateSystem = new SpatialEye.Framework.Geometry.CoordinateSystems.EpsgCoordinateSystemReference { SRId = 4326, Name = "WGS 84" };
                Map.PixelToWorldTransform.Convert(co);
                //var co2 = await GeometryManager.Instance.TransformAsync(co, Map.CoordinateSystem.EPSGCode, WGS84CoordinateSystem.SRId);
                var co2 = geometryService.TransformAsync(co, Map.CoordinateSystem.EPSGCode, WGS84CoordinateSystem.SRId).Result;

                _strLatitud = (co2.Y).ToString("00.0000000");
                _strLongitud = (co2.X).ToString("00.0000000");

               
                //Si es el primer trazo
                //if (_viewModel.Latitud == string.Empty && _viewModel.Longitud == string.Empty)
                //{
                    _viewModel.Latitud = _strLatitud.Replace(",",".");
                    _viewModel.Longitud = _strLongitud.Replace(",",".");
                //}
                //else
                //{
                //   _viewModel.Latitud =  _viewModel.Latitud + "," + _strLatitud;
                //    _viewModel.Longitud =  _viewModel.Longitud + "," + _strLongitud;
                //  }

                
                
                foreach (var element in _geometry)
                {
                    _interactionLayer.Add(element);
                }

               
            }

            // Mouse is no longer down
            IsMouseDown = false;

            // We have handled the event
            args.Handled = true;
            base.OnMouseLeftButtonUp(sender, args);
        }


#region Juan 

        /// <summary>
        /// Handle the double click event
        /// </summary>
        /// <param name="sender">The mapViewModel</param>
        /// <param name="args">The mouse event args</param>
        protected override void OnMouseLeftButtonDoubleClick(MapViewModel sender, MapMouseEventArgs args)
        {
            base.OnMouseLeftButtonDoubleClick(sender, args);

            _interactionLayer = args.InteractionLayer;

            // Detiene el trazo y limpia las geometrías
            _interactionLayer.Clear();
            _geometry.Clear();
           
            // We've handled the double click event
            args.Handled = true;
        }
        #endregion


        protected static FrameworkElement AnnotationFor(CoordinateSystem cs, Coordinate coord, double angle, Brush style, double size, double lengthInMeters, TextAlignment alignment, int extraDistance)
        {
            var realAngle = 180 * angle / Math.PI;
            var annotation = new TextBlock()
            {
                
                Text = "▼",
                FontSize = size,
                TextAlignment = TextAlignment.Left,
                Foreground = style,
                FontWeight = FontWeights.ExtraBold,
            };


            var x = coord.X - System.Math.Cos(angle) * extraDistance;
            var y = coord.Y - System.Math.Sin(angle) * extraDistance;

            var txfm = new System.Windows.Media.TransformGroup();
            txfm.Children.Add(new RotateTransform { Angle = realAngle });
            txfm.Children.Add(new TranslateTransform { X = coord.X - 5, Y = coord.Y - 5 });

            annotation.RenderTransform = txfm;
            return annotation;
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
        #endregion
    }

}
