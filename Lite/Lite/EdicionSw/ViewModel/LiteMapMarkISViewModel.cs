using System.Windows;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Maps;
using System.Linq;
using SpatialEye.Framework.Geometry.CoordinateSystems;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Redlining;

namespace Lite
{
    public class LiteMapMarkISViewModel : ViewModelBase
    {
        #region Property Names
        public static string DefaultIsActivePropertyName = "DefaultIsActive";
        public const string MarcaActivaPropertyName = "MarcaActiva";
        public const string LatitudPropertyName = "Latitud";
        public const string LongitudPropertyName = "Longitud";

        #endregion

        #region Fields
        private MapViewModel _mapViewModel;
        private bool _defaultIsActive = true;
        private bool _marcaActiva;
        private bool _settingUpInteractionMode;
        private bool _isActive;
        private string _latitud = "";
        private string _longitud = "";
        #endregion

        #region Constructor
        public LiteMapMarkISViewModel(Messenger messenger = null)
            : base(messenger)
        {
            RegisterToMessenger();
            InteractionMode = new LiteMapMarkInteractionModeIS(this) { InterruptMode = MapInteractionMode.InterruptModeType.AllowInterrupt };

            // J.A Set up the commands to craete/interact with the trail
            //SetupCommands();
        }
        #endregion

      
        #region Messenger Registration

        private void RegisterToMessenger()
        {
            if (IsInDesignMode)
            {
                // Skip when in design mode
                return;
            }

            Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>
              (this, m =>
              {
                  // In case the active map has changed, pick it up
                  CurrentMap = m.NewValue;
              });
        }

        #endregion


        #region Properties
        /// <summary>
        /// Holds the current map the measure should operate on
        /// </summary>
        public MapViewModel CurrentMap
        {
            get { return _mapViewModel; }
            set
            {
                if (_mapViewModel != null)
                {
                    // Make sure we no longer register to changes of the map and its interaction handler
                    _mapViewModel.PropertyChanged -= MapViewModel_PropertyChanged;
                    if (_mapViewModel.InteractionHandler != null)
                    {
                        _mapViewModel.InteractionHandler.PropertyChanged -= InteractionHandler_PropertyChanged;

                    }
                }

                _mapViewModel = value;

                if (_mapViewModel != null)
                {
                    // Register to change notification of the map and its interaction handler
                    _mapViewModel.PropertyChanged += MapViewModel_PropertyChanged;
                    if (_mapViewModel.InteractionHandler != null)
                    {
                        _mapViewModel.InteractionHandler.PropertyChanged += InteractionHandler_PropertyChanged;
                    }
                }

                // Start with the default mode being active
                DefaultIsActive = true;
            }
        }

        /// <summary>
        /// Returns the current interaction handler (the interaction handler of the current map).
        /// </summary>
        private MapInteractionHandler CurrentInteractionHandler
        {

            get { return CurrentMap != null ? CurrentMap.InteractionHandler : null; }

        }

        /// <summary>
        /// Property change handler for changes in Map properties
        /// </summary>
        /// <param name="sender">The map</param>
        /// <param name="e">The property changed arguments</param>
        void MapViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == MapViewModel.EnvelopePropertyName)
            {
                // The extent of the map has changed; in case this is not in a controlled fashion
                // we switch to the default interaction mode. This leafs any measure mode that
                // was active
                if (!InteractionMode.IsMovingMap && !InteractionMode.IsMouseDown)
                {
                    DefaultIsActive = true;
                }
            }
        }

        /// <summary>
        /// Property change handler for changes in the map's interaction handler
        /// </summary>
        /// <param name="sender">The interaction handler</param>
        /// <param name="e">The property changed arguments</param>
        void InteractionHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // In case we are not setting up the map and there is no active mode, switch to the default one
            if (!_settingUpInteractionMode && e.PropertyName == MapInteractionHandler.ActiveInteractionModePropertyName)
            {
                if (this.CurrentMap.InteractionHandler.CurrentDynamicInteractionMode == null)
                {
                    this.DefaultIsActive = true;
                }
            }
        }
        #endregion

        #region Interaction mode
        /// <summary>
        /// The intercation mode, handling all user interaction with the map for the active measurer
        /// </summary>
        private LiteMapMarkInteractionModeIS InteractionMode { get; set; }

        /// <summary>
        /// Set the active interaction mode dependent on our active mode
        /// </summary>
        private void SetInteractionMode()
        {

            if (CurrentMap != null)
            {
                _settingUpInteractionMode = true;
                try
                {
                    // Get the current interaction mode
                    var currentMode = CurrentInteractionHandler.CurrentDynamicInteractionMode;
                    CurrentInteractionHandler.CurrentDynamicInteractionMode = null;

                    var newMode = !DefaultIsActive ? InteractionMode : null;


                    // Set (or reset) the interaction mode
                    CurrentMap.InteractionHandler.CurrentDynamicInteractionMode = newMode;

                    // Indicates whether we are currently active
                    _isActive = newMode != null;
                }
                finally
                {
                    _settingUpInteractionMode = false;
                }
            }
        }

        /// <summary>
        /// Resets the interaction mode, in case we are active.
        /// </summary>
        private void ResetInteractionMode()
        {
            if (_isActive)
            {
                var currentMode = CurrentMap.InteractionHandler.CurrentDynamicInteractionMode;

                if (currentMode == InteractionMode)
                {
                    _isActive = false;
                    // Set (or reset) the interaction mode
                    CurrentMap.InteractionHandler.CurrentDynamicInteractionMode = null;
                }
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Holds the top margin of the map where the measurer should not be active.
        /// This allows for extra overlap of the map bar
        /// </summary>
        public int TopMargin
        {
            get { return InteractionMode.TopMargin; }
            set { InteractionMode.TopMargin = value; }
        }

        /// <summary>
        /// Holds the bottom margin of the map where the measurer should not be active.
        /// </summary>
        public int BottomMargin
        {
            get { return InteractionMode.BottomMargin; }
            set { InteractionMode.BottomMargin = value; }
        }

        /// <summary>
        /// Holds the left margin of the map where the measurer should not be active.
        /// </summary>
        public int LeftMargin
        {
            get { return InteractionMode.LeftMargin; }
            set { InteractionMode.LeftMargin = value; }
        }

        /// <summary>
        /// Holds the right margin of the map where the measurer should not be active.
        /// </summary>
        public int RightMargin
        {
            get { return InteractionMode.RightMargin; }
            set { InteractionMode.RightMargin = value; }
        }

        public string Latitud
        {
            get { return _latitud; }
            set
            {
                if (value != _latitud)
                {
                    _latitud = value;
                    RaisePropertyChanged(LatitudPropertyName);

                }
            }
        }
        public string Longitud
        {
            get { return _longitud; }
            set
            {
                if (value != _longitud)
                {
                    _longitud = value;
                    RaisePropertyChanged(LongitudPropertyName);
                }
            }
        }
        #endregion

        #region Interaction State

        public bool DefaultIsActive
        {
            get { return _defaultIsActive; }
            set
            {
                if (_defaultIsActive != value)
                {
                    if (value)
                    {
                        // Set active
                        _defaultIsActive = true;

                        // Switching off the measure modes
                        MarcaActiva = false;

                        //Si desactiva limpiar latitud y longitud
                        Latitud = string.Empty;
                        Longitud = string.Empty;
                    }
                    else if (MarcaActiva)
                    {
                        // Switch off
                        _defaultIsActive = false;
                    }

                    // Raise the appropriate change
                    RaisePropertyChanged(DefaultIsActivePropertyName);

                    if (_defaultIsActive)
                    {
                        // Set the interaction mode (if it was turned on)
                        SetInteractionMode();
                    }
                }
            }
        }
        public bool MarcaActiva
        {
            get { return _marcaActiva; }
            set
            {
                if (_marcaActiva != value)
                {
                    if (value)
                    {
                        // Set active
                        _marcaActiva = true;
                        // Switching off others
                        DefaultIsActive = false;
                    }
                    else
                    {
                        _marcaActiva = false;
                        //_latitud = "";
                        //_longitud = "";
                        DefaultIsActive = true;
                    }

                    // Notify the change
                    RaisePropertyChanged(MarcaActivaPropertyName);

                    if (_marcaActiva)
                    {
                        // In case the mode has been switched on, set the new interaction mode
                        SetInteractionMode();
                    }
                }
            }
        }
        #endregion

    }


}
