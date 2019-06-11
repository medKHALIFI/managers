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

using SpatialEye.Framework.Redlining;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Features.Recipe;
using Lite.Resources.Localization;
using System.Collections.Generic;

namespace Lite
{
    public class MapTrailISViewModel :ViewModelBase
    {
             #region Property Names

    /// <summary>
    /// Is the submenu of the trail active
    /// </summary>
    public static string SubMenuIsActivePropertyName = "SubMenuIsActive";
    public static string DefaultIsActivePropertyName = "DefaultIsActive";
    public const string MarcaActivaPropertyName = "MarcaActiva";
    public const string LatitudPropertyName = "Latitud";
    public const string LongitudPropertyName = "Longitud";
    #endregion

    #region Fields
    /// <summary>
    /// The mapView to use
    /// </summary>
    private MapViewModel _mapView;
    
    /// <summary>
    /// Is the trail submenu active
    /// </summary>
    private bool _subMenuIsActive;
    private bool _defaultIsActive = true;
    private bool _marcaActiva;
    private bool _settingUpInteractionMode;
    private bool _isActive;
    private string _latitud = "";
    private string _longitud = "";
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the MapTrail ViewModel and attaches it to the specified messenger
    /// </summary>
    /// <param name="messenger"></param>
    public MapTrailISViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Attach to the messenger; reacting to view model changes
        AttachToMessenger();

      // Set up the commands to craete/interact with the trail
      SetupCommands();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      // Set up a handler for active mapView changes
        RegisterToMessenger();
       //this.Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, change => MapView = change.NewValue);

      InteractionMode = new MapTrailInteractionModeIS(this) { InterruptMode = MapInteractionMode.InterruptModeType.AllowInterrupt };
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
              MapView = m.NewValue;
          });
    }

    #endregion

    #region Commands
    /// <summary>
    /// Set up the commands
    /// </summary>
    private void SetupCommands()
    {
        CreateTrailPolygonCommand = new RelayCommand(() => NewTrail(new RedliningPolygon(CoordinateSystem)), CanAddNewElement);

    }

    /// <summary>
    /// Let all commands check their own state
    /// </summary>
    private void CheckCommands()
    {
      CreateTrailPolygonCommand.RaiseCanExecuteChanged();
   
    }
    
    /// <summary>
    /// Can we add a new element
    /// </summary>
    private bool CanAddNewElement()
    {
      return MapView != null && MapView.TrailLayer != null && MapView.CoordinateSystem != null;
    }
    #endregion

    #region Trail Creation Helpers
    /// <summary>
    /// Create a new element for the Trail Layer
    /// </summary>
    /// <param name="element">The element to add</param>
    /// 

    private void NewTrail(RedliningElement element)
    {
      // Switch off the submenu
      //SubMenuIsActive = false;        

      //Desactivar, si  ya no genera la imagen solo el triangulo rojo
      MapView.TrailLayer.NewElement(element);
      //MarcaActiva = true;

       //Activa el triangulo rojo
      //MainPage objMarca = new MainPage();
      //objMarca.chkMarca2.IsChecked=true ;    

    }

    #endregion


    #region Properties
    /// <summary>
    /// Internal property to help trail construction be less verbose
    /// </summary>
    private CoordinateSystem CoordinateSystem
    {
      get { return MapView != null ? MapView.CoordinateSystem : null; }
    }


    public RelayCommand CreateTrailPolygonCommand { get; private set; }



    /// <summary>
    /// The map view we are currently creating trails for
    /// </summary>
    public MapViewModel MapView
    {
      get { return _mapView; }
      set
      {

          if (_mapView != null)
          {
              // Make sure we no longer register to changes of the map and its interaction handler
              _mapView.PropertyChanged -= MapViewModel_PropertyChanged;
              if (_mapView.InteractionHandler != null)
              {
                  _mapView.InteractionHandler.PropertyChanged -= InteractionHandler_PropertyChanged;

              }
          }

          if (_mapView != value)
          {
              _mapView = value;

              CheckCommands();
          }

          _mapView = value;

          if (_mapView != null)
          {
              // Register to change notification of the map and its interaction handler
              _mapView.PropertyChanged += MapViewModel_PropertyChanged;
              if (_mapView.InteractionHandler != null)
              {
                  _mapView.InteractionHandler.PropertyChanged += InteractionHandler_PropertyChanged;
              }
          }

          // Start with the default mode being active
          DefaultIsActive = true;
      }
    }
    #endregion

    private MapInteractionHandler CurrentInteractionHandler
    {

        get { return MapView != null ? MapView.InteractionHandler : null; }

    }

        
    void MapViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == MapViewModel.EnvelopePropertyName)
        {
            // The extent of the map has changed; in case this is not in a controlled fashion
            // we switch to the default interaction mode. This leafs any measure mode that
            // was active
            if (!InteractionMode.IsMovingMap && !InteractionMode.IsMouseDown)
            {
                //DefaultIsActive = true;
            }
        }
    }

    void InteractionHandler_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // In case we are not setting up the map and there is no active mode, switch to the default one
        if (!_settingUpInteractionMode && e.PropertyName == MapInteractionHandler.ActiveInteractionModePropertyName)
        {
            if (this.MapView.InteractionHandler.CurrentDynamicInteractionMode == null)
            {
                //this.DefaultIsActive = true;
            }
        }
    }
        
    #region Submenu
    /// <summary>
    /// Gets or sets the value if the trail submenu is active
    /// </summary>
    public bool SubMenuIsActive
    {
      get { return _subMenuIsActive; }
      set
      {
        if (value != _subMenuIsActive)
        {
          _subMenuIsActive = value;
          RaisePropertyChanged(SubMenuIsActivePropertyName);
        }
      }
    }

    private MapTrailInteractionModeIS InteractionMode { get; set; }

    private void SetInteractionMode()
    {

        if (MapView != null)
        {
            _settingUpInteractionMode = true;
            try
            {
                // Get the current interaction mode
                var currentMode = CurrentInteractionHandler.CurrentDynamicInteractionMode;
                CurrentInteractionHandler.CurrentDynamicInteractionMode = null;

                var newMode = !DefaultIsActive ? InteractionMode : null;


                // Set (or reset) the interaction mode
                MapView.InteractionHandler.CurrentDynamicInteractionMode = newMode;

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
            var currentMode = MapView.InteractionHandler.CurrentDynamicInteractionMode;

            if (currentMode == InteractionMode)
            {
                _isActive = false;
                // Set (or reset) the interaction mode
                MapView.InteractionHandler.CurrentDynamicInteractionMode = null;
            }
        }
    }
    #endregion

    /// <summary>

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
                    //Comentar por que quita los valores
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

                    CreateTrailPolygonCommand = new RelayCommand(() => NewTrail(new RedliningPolygon(CoordinateSystem)), CanAddNewElement);

                    CheckCommands();
                    NewTrail(new RedliningPolygon(CoordinateSystem));

                    // In case the mode has been switched on, set the new interaction mode
                    SetInteractionMode();
                }
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

    }
}
