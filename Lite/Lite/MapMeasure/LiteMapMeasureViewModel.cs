using System.Windows;
using SpatialEye.Framework.Client;
using System.Collections.ObjectModel;
using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The measure viewModel provides access to the Measure interaction mode.
  /// The interaction mode can operate in different modes, the free measure
  /// mode and the dimensioning mode.
  /// </summary>
  public class LiteMapMeasureViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The default interaction mode is active
    /// </summary>
    public static string DefaultIsActivePropertyName = "DefaultIsActive";

    /// <summary>
    /// The visibility of the default mode, which is a Visibility dependent
    /// on the DefaultIsActive property.
    /// </summary>
    public static string DefaultModeVisibilityPropertyName = "DefaultModeVisibility";

    /// <summary>
    /// The free measure mode is active
    /// </summary>
    public static string MeasureFreeIsActivePropertyName = "MeasureFreeIsActive";

    /// <summary>
    /// The dimensioning measure mode is active
    /// </summary>
    public static string MeasureDimIsActivePropertyName = "MeasureDimIsActive";

    /// <summary>
    /// Is the submenu of the measurer active
    /// </summary>
    public static string SubMenuIsActivePropertyName = "SubMenuIsActive";

    #endregion

    #region Fields
    /// <summary>
    /// The mapViewModel we are acting upon
    /// </summary>
    private MapViewModel _mapViewModel;

    /// <summary>
    /// Is the default mode active
    /// </summary>
    private bool _defaultIsActive = true;

    /// <summary>
    /// Is the free measure mode active
    /// </summary>
    private bool _measureFreeIsActive;

    /// <summary>
    /// Is the dimensioning measure mode active
    /// </summary>
    private bool _measureDimIsActive;

    /// <summary>
    /// Do we want to show the default mode
    /// </summary>
    private bool _showDefaultMode;

    /// <summary>
    /// A flag indicating whether we are in the process of setting up
    /// the interaction mode
    /// </summary>
    private bool _settingUpInteractionMode;

    /// <summary>
    /// Are we active
    /// </summary>
    private bool _isActive;

    /// <summary>
    /// Is the measurer submenu active
    /// </summary>
    private bool _subMenuIsActive;

    /// <summary>
    /// Measure items
    /// </summary>
    private ObservableCollection<LiteMenuCategoryViewModel> _menuItems;
    #endregion

    #region Constructor
    /// <summary>
    /// The default constructor for the measure viewModel
    /// </summary>
    /// <param name="messenger">The messenger to register to</param>
    public LiteMapMeasureViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Register to the messenger, picking off the active map
      RegisterToMessenger();

      // Setup the menu commands
      SetupMenuCommands();

      // Create the interaction mode
      InteractionMode = new LiteMapMeasureInteractionMode() { InterruptMode = MapInteractionMode.InterruptModeType.DoNotInterrupt };
    }
    #endregion

    #region Messenger Registration
    /// <summary>
    /// Registration of the viewModel to the messenger
    /// </summary>
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


    #region Commands

    /// <summary>
    /// Set up the commands
    /// </summary>
    private void SetupMenuCommands()
    {
      // Setup the menu items
      var items = new ObservableCollection<LiteMenuCategoryViewModel>();

      var cat = new LiteMenuCategoryViewModel { Title = ApplicationResources.MapMeasurerModeCategoryHeader };
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapMeasureFreeMode, "MetroIcon.Content.MeasureFree")
      {
        Command = new RelayCommand(() => MeasureFreeIsActive = true),
      });
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapMeasureDimMode, "MetroIcon.Content.MeasureDim")
      {
        Command = new RelayCommand(() => MeasureDimIsActive = true),
      });

      items.Add(cat);

      MenuItems = items;
    }

    /// <summary>
    /// Let all commands check their own state
    /// </summary>
    private void CheckMenuItemStates()
    {
      foreach (var item in MenuItems)
      {
        item.CheckStates();
      }
    }

    #endregion

    #region CultureChanged

    /// <summary>
    /// Callback when the culture changes
    /// </summary>
    /// <param name="currentCultureInfo"></param>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      SetupMenuCommands();
    }

    #endregion

    #region Interaction State
    /// <summary>
    /// A flag specifying whether the default mode should be shown
    /// </summary>
    public bool ShowDefaultMode
    {
      get { return _showDefaultMode; }
      set
      {
        if (_showDefaultMode != value)
        {
          _showDefaultMode = value;
          RaisePropertyChanged(DefaultModeVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Holds the visibility of the default mode
    /// </summary>
    public Visibility DefaultModeVisibility
    {
      get { return _showDefaultMode ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Holds a flag indicating whether the default mode is active
    /// </summary>
    public bool DefaultIsActive
    {
      get { return _defaultIsActive; }
      set
      {
        if (_defaultIsActive != value)
        {
          // Switch off the submenu
          SubMenuIsActive = false;

          if (value)
          {
            // Set active
            _defaultIsActive = true;

            // Switching off the measure modes
            MeasureFreeIsActive = false;
            MeasureDimIsActive = false;
          }
          else if (MeasureFreeIsActive || MeasureDimIsActive)
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

    /// <summary>
    /// Holds a flag specifying whether the free measure mode is active
    /// </summary>
    public bool MeasureFreeIsActive
    {
      get { return _measureFreeIsActive; }
      set
      {
        if (_measureFreeIsActive != value)
        {
          // Switch off the submenu
          SubMenuIsActive = false;

          if (value)
          {
            // Set active
            _measureFreeIsActive = true;

            // Switching off others
            DefaultIsActive = false;
            MeasureDimIsActive = false;
          }
          else
          {
            _measureFreeIsActive = false;
            if (!MeasureDimIsActive)
            {
              // Switch off, switch the default one on
              DefaultIsActive = true;
            }
          }

          // Notify the change
          RaisePropertyChanged(MeasureFreeIsActivePropertyName);
          CheckMenuItemStates();

          if (_measureFreeIsActive)
          {
            // In case the mode has been switched on, set the new interaction mode
            SetInteractionMode();
          }
        }
      }
    }

    /// <summary>
    /// Holds a flag specifying whether the dimensioning mode is active
    /// </summary>
    public bool MeasureDimIsActive
    {
      get { return _measureDimIsActive; }
      set
      {
        if (_measureDimIsActive != value)
        {
          // Switch off the submenu
          SubMenuIsActive = false;

          if (value)
          {
            // Set active
            _measureDimIsActive = true;

            // Switching off others
            DefaultIsActive = false;
            MeasureFreeIsActive = false;
          }
          else
          {
            _measureDimIsActive = false;
            if (!MeasureFreeIsActive)
            {
              // Switched off, switch the default one on
              DefaultIsActive = true;
            }
          }

          // Notify the change
          RaisePropertyChanged(MeasureDimIsActivePropertyName);
          CheckMenuItemStates();

          if (_measureDimIsActive)
          {
            // In case the mode has been switched on, set the new interaction mode
            SetInteractionMode();
          }
        }
      }
    }
    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the menu items
    /// </summary>
    public ObservableCollection<LiteMenuCategoryViewModel> MenuItems
    {
      get { return _menuItems; }
      private set
      {
        _menuItems = value;
        RaisePropertyChanged();
      }
    }

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
    private LiteMapMeasureInteractionMode InteractionMode { get; set; }

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

          if (!DefaultIsActive)
          {
            // Switch the meaasurer (of the interaction mode) dependent on our flags.
            // Will result in the default measurer or the dimensioning one to be used.
            InteractionMode.SetMeasurer(MeasureDimIsActive);
          }

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
    #endregion

    #region Submenu
    /// <summary>
    /// Gets or sets the value if the measurer submenu is active
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
    #endregion
  }
}
