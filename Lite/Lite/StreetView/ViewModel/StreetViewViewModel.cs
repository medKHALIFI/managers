using System;
using System.Windows;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Threading;

namespace Lite
{
  public class StreetViewViewModel : ViewModelBase
  {
    #region PropertyNames
    /// <summary>
    /// The Street View interaction mode is active
    /// </summary>
    public static string StreetViewInteractionIsActivePropertyName = "StreetViewInteractionIsActive";

    /// <summary>
    /// Is the streetView interaction allowed
    /// </summary>
    public static string ViewVisibilityPropertyName = "ViewVisibility";

    /// <summary>
    /// Is the streetView interaction allowed
    /// </summary>
    public static string ViewIsEnabledPropertyName = "ViewIsEnabled";
    #endregion

    #region Private Fields

    /// <summary>
    /// Contains the streetview interaction mode
    /// </summary>
    private StreetViewInteractionMode _interactionMode;

    /// <summary>
    /// Lite map view model to operate on
    /// </summary>
    private LiteMapViewModel _mapViewModel;

    /// <summary>
    /// Is the street view interaction allowed
    /// </summary>
    private bool _viewIsVisible;

    /// <summary>
    /// Is the street view interaction allowed
    /// </summary>
    private bool _viewIsEnabled;

    /// <summary>
    /// Is the street view interaction mode active
    /// </summary>
    private bool _streetViewInteractionIsActive;

    /// <summary>
    /// A flag indicating whether we are in the process of setting up
    /// the interaction mode
    /// </summary>
    private bool _settingUpInteractionMode;

    /// <summary>
    /// Are the required streetview api's setup in the browser page
    /// </summary>
    private bool _isStreetViewInBrowserAvailable;

    #endregion

    #region Constructor

    /// <summary>
    /// Constructs a new viewmodel
    /// </summary>
    /// <param name="browserApi">the browser api for communicating with the browser</param>
    /// <param name="messenger">messenger</param>
    public StreetViewViewModel(Messenger messenger = null)
      : base(messenger)
    {
      _interactionMode = new StreetViewInteractionMode();

      // Attach event handlers
      _interactionMode.RequestShowStreetView += HandleShowStreetView;
      _interactionMode.RequestCheckStreetViewStatus += HandleCheckStreetViewStatus;

      if (!IsInDesignMode)
      {
        RegisterToMessenger();
        SetupBrowserApi();
      }
    }
    #endregion

    #region Setup

    /// <summary>
    /// Setup the browser api
    /// </summary>
    private void SetupBrowserApi()
    {
      // Check if the required apis are loaded and configured correctly in the encapsulated HTML
      _isStreetViewInBrowserAvailable = BrowserAPI.Instance.JSIsStreetViewAvailable();

      if (_isStreetViewInBrowserAvailable)
      {
        BrowserAPI.Instance.StreetViewStatusChanged += StreetViewStatusChanged;
      }
      else
      {
        // Do not use the tracelogger but write directly to the browser console (tracelogger is not setup yet)
        BrowserAPI.Instance.WriteToJSConsole(2, "Google Street View api not correctly setup in html, Street View functionality disabled in Lite");
      }
    }

    /// <summary>
    /// Get on the bus
    /// </summary>
    private void RegisterToMessenger()
    {
      // Register for any map changes
      Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>
        (this, m =>
        {
          CurrentMap = m.NewValue;
        });
    }
    #endregion

    #region Api
    /// <summary>
    ///  Gets or sets the current map to operate on
    /// </summary>
    public LiteMapViewModel CurrentMap
    {
      get { return _mapViewModel; }
      set
      {
        if (_mapViewModel != null && _mapViewModel.InteractionHandler != null)
        {
          _mapViewModel.LayerPropertyChanged -= _mapViewModel_LayerPropertyChanged;
          _mapViewModel.PropertyChanged -= _mapViewModel_PropertyChanged;
          _mapViewModel.InteractionHandler.PropertyChanged -= InteractionHandlerPropertyChanged;
        }

        _mapViewModel = value;

        if (_mapViewModel != null && _mapViewModel.InteractionHandler != null)
        {
          _mapViewModel.InteractionHandler.PropertyChanged += InteractionHandlerPropertyChanged;
          _mapViewModel.LayerPropertyChanged += _mapViewModel_LayerPropertyChanged;
          _mapViewModel.PropertyChanged += _mapViewModel_PropertyChanged;

          CheckVisibilityStates();
        }
      }
    }

    /// <summary>
    /// Gets or sets if the interaction mode is active
    /// </summary>
    public bool StreetViewInteractionIsActive
    {
      get { return _streetViewInteractionIsActive; }
      set
      {
        if (_streetViewInteractionIsActive != value)
        {
          _streetViewInteractionIsActive = value;

          RaisePropertyChanged(StreetViewInteractionIsActivePropertyName);

          SetupInteractionMode();
        }
      }
    }

    /// <summary>
    /// Is the view enabled
    /// </summary>
    public bool ViewIsEnabled
    {
      get { return _viewIsEnabled; }
      set
      {
        if (_viewIsEnabled != value)
        {
          LiteTraceLogger.Instance.WriteVerbose(String.Format("Google Street View function is {0}enabled", (value) ? "" : "not "));

          _viewIsEnabled = value;
          RaisePropertyChanged(ViewIsEnabledPropertyName);
        }
      }

    }


    /// <summary>
    /// Is the street view interaction allowed
    /// </summary>
    public bool ViewIsVisible
    {
      get { return _viewIsVisible; }
      set
      {
        if (_viewIsVisible != value)
        {
          LiteTraceLogger.Instance.WriteVerbose(String.Format("Google Street View function is {0}visible", (value) ? "" : "not "));

          _viewIsVisible = value;
          RaisePropertyChanged(ViewVisibilityPropertyName);

          if (!_viewIsVisible)
          {
            // Make sure we're no longer active
            StreetViewInteractionIsActive = false;
          }
        }
      }
    }

    /// <summary>
    /// Returns the visibility of the street view ui
    /// </summary>
    public Visibility ViewVisibility
    {
      get { return _viewIsVisible ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Check Street View if there is an image for the given coord
    /// </summary>
    /// <param name="coord">Coord to check</param>
    /// <returns>true if request has succeeded</returns>
    public bool HandleCheckStreetViewStatus(StreetViewInteractionMode sender, StreetViewInteractionMode.StreetViewPositionEventArgs args)
    {
      var wgsCoordinate = args.Coordinate;
      return BrowserAPI.Instance.CheckStreetViewStatus(wgsCoordinate.X, wgsCoordinate.Y);
    }

    /// <summary>
    /// Show Street View for the given coord
    /// </summary>
    /// <param name="coord">Coord for display</param>
    /// <returns>true if request has succeeded</returns>
    public bool HandleShowStreetView(StreetViewInteractionMode sender, StreetViewInteractionMode.StreetViewPositionEventArgs args)
    {
      var wgsCoordinate = args.Coordinate;
      return BrowserAPI.Instance.ShowStreetView(wgsCoordinate.X, wgsCoordinate.Y);
    }
    #endregion

    #region Interaction Mode
    /// <summary>
    /// Setup the interaction mode
    /// </summary>
    private void SetupInteractionMode()
    {
      if (CurrentMap != null)
      {
        _settingUpInteractionMode = true;

        var newMode = (StreetViewInteractionIsActive) ? _interactionMode : null;

        UIDispatcher.BeginInvoke(() =>
        {
          // Set (or reset) the interaction mode
          CurrentMap.InteractionHandler.CurrentDynamicInteractionMode = newMode;
        });

        _settingUpInteractionMode = false;
      }
    }

    /// <summary>
    /// Check whether street view is allowed
    /// </summary>
    private void CheckVisibilityStates()
    {
      var visible = _isStreetViewInBrowserAvailable && LiteClientSettingsViewModel.Instance.AllowStreetView && _mapViewModel != null;
      var enabled = false;
      if (visible)
      {
        MapLayerViewModel googleLayer = null;
        foreach (var layer in _mapViewModel.Layers)
        {
          if (layer.LayerDefinition != null && layer.LayerDefinition is GoogleMapLayerDefinition)
          {
            googleLayer = layer;
            break;
          }
        }

        visible = googleLayer != null;

        if (visible)
        {
          enabled = googleLayer.IsOn && googleLayer.Opacity > 0.0;
        }
      }

      ViewIsVisible = visible;
      ViewIsEnabled = enabled;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback from the webapi containing the requested street view status
    /// </summary>
    private void StreetViewStatusChanged(object sender, BrowserFunctionStatusEventArgs e)
    {
      _interactionMode.SetViewAvailability(e.Succes);
    }

    /// <summary>
    /// Callback from the interaction handler when a property has changed
    /// </summary>
    private void InteractionHandlerPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      // if interaction is cancelled set street
      if (!_settingUpInteractionMode && e.PropertyName == MapInteractionHandler.ActiveInteractionModePropertyName)
      {
        if (this.CurrentMap.InteractionHandler.CurrentDynamicInteractionMode == null)
        {
          StreetViewInteractionIsActive = false;
        }
      }
    }

    /// <summary>
    /// Callback for Change in a property of a mapView's layer
    /// </summary>
    void _mapViewModel_LayerPropertyChanged(MapViewModel map, MapLayerViewModel layer, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (layer.LayerDefinition != null && layer.LayerDefinition is GoogleMapLayerDefinition)
      {
        // A Google layer has changed
        CheckVisibilityStates();
      }
    }

    /// <summary>
    /// Change in a property of the map view
    /// </summary>
    void _mapViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == MapViewModel.LayersPropertyName)
      {
        CheckVisibilityStates();
      }
    }
    #endregion
  }
}

