using System;
using System.Windows;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Maps;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The LiteNewUserMapViewModel holds the logic for adding a New Map to the available Maps (viewModel).
  /// </summary>
  public class LiteNewUserMapViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The Add Map bits are visible when the user requests to show the Add Map View
    /// </summary>
    public const string AddMapViewVisiblePropertyName = "AddMapViewVisible";

    /// <summary>
    /// The corresponding visibility that is dependent on the flag
    /// </summary>
    public const string AddMapViewVisibilityPropertyName = "AddMapViewVisibility";

    /// <summary>
    /// New Label
    /// </summary>
    public const string NewMapLabelPropertyName = "NewMapLabel";

    /// <summary>
    /// The Map Name
    /// </summary>
    public const string MapNamePropertyName = "MapName";
    #endregion

    #region Private Fields
    /// <summary>
    /// Is the add Map functionality visible
    /// </summary>
    private Boolean _addMapViewVisible;

    /// <summary>
    /// The map name
    /// </summary>
    private string _mapName;
    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="messenger"></param>
    public LiteNewUserMapViewModel(LiteMapsViewModel maps, Messenger messenger = null)
      : base(messenger)
    {
      this.Maps = maps;

      _addMapViewVisible = false;

      MapDefinitionViewModel = new MapDefinitionComboBoxViewModel(messenger);
      MapDefinitionViewModel.ServiceProviderGroupTypeProperties.AllowedGroupTypes = new ServiceProviderGroupType[] { ServiceProviderGroupType.Business, ServiceProviderGroupType.Analysis };
      MapDefinitionViewModel.PropertyChanged += MapDefinitionViewModel_PropertyChanged;

      // If there is a single Analysis, hide it (it will be the same name as the Analysis itself)
      MapDefinitionViewModel.DatumProperties.HideWhenSingleDatumFor = (serviceProvider, serviceProviderGroupType) => serviceProviderGroupType == ServiceProviderGroupType.Analysis;
      MapDefinitionViewModel.MapDefinitionsFilter = this.MapDefinitionFilter;

      SetCurrentCultureLabels();
      SetupCommands();
    }

    /// <summary>
    /// Current culture has changed
    /// </summary>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      base.OnCurrentCultureChanged(currentCultureInfo);

      SetCurrentCultureLabels();
    }

    /// <summary>
    /// Set the labels for the current culture
    /// </summary>
    private void SetCurrentCultureLabels()
    {
      MapDefinitionViewModel.ServiceProviderProperties.Name = ApplicationResources.MapsNewMapServiceProvider;
      MapDefinitionViewModel.ServiceProviderGroupTypeProperties.Name = ApplicationResources.MapsNewMapServiceProviderGroupType;
      MapDefinitionViewModel.ServiceProviderGroupProperties.Name = ApplicationResources.MapsNewMapServiceProviderMap;
      MapDefinitionViewModel.DatumProperties.Name = ApplicationResources.MapsNewMapServiceProviderMap;

      // New Map Label
      RaisePropertyChanged(NewMapLabelPropertyName);
    }
    #endregion

    #region Commands
    /// <summary>
    /// The Add Map Command
    /// </summary>
    public RelayCommand AddMapCommand { get; private set; }

    /// <summary>
    /// The Show Add Map View command
    /// </summary>
    public RelayCommand ShowAddMapViewCommand { get; private set; }

    /// <summary>
    /// The command for hiding the active map view
    /// </summary>
    public RelayCommand HideAddMapViewCommand { get; private set; }

    /// <summary>
    /// Setup the commands for this viewmodel
    /// </summary>
    private void SetupCommands()
    {
      AddMapCommand = new RelayCommand(AddMap, () => { return MapDefinitionViewModel.SelectedMapDefinition != null; });
      ShowAddMapViewCommand = new RelayCommand(ShowAddMap, () => { return !AddMapViewVisible && CanAddMap; });
      HideAddMapViewCommand = new RelayCommand(HideAddMap, () => { return AddMapViewVisible; });
    }

    /// <summary>
    /// Check the executable state of the commands
    /// </summary>
    private void CheckCommands()
    {
      AddMapCommand.RaiseCanExecuteChanged();
      ShowAddMapViewCommand.RaiseCanExecuteChanged();
      HideAddMapViewCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Returns a flag indicating whether we can display the lot
    /// </summary>
    public bool CanAddMap
    {
      get { return MapDefinitionViewModel.ActiveDatums != null && MapDefinitionViewModel.ActiveDatums.Count > 0; }
    }

    /// <summary>
    /// Adds the selected map to the map
    /// </summary>
    private void AddMap()
    {
      var newMapDefinition = this.MapDefinitionViewModel.SelectedMapDefinition;
      if (newMapDefinition != null)
      {
        this.Maps.AddNewMap(newMapDefinition, this.MapName);
      }

      // Force the checking of the filter, since it has changed
      // Automatically force hiding the panel
      Refilter(true);
    }

    /// <summary>
    /// Shows the Add Map (view); callback from the ShowAddMapViewCommand
    /// </summary>
    private void ShowAddMap()
    {
      if (CanAddMap)
      {
        this.MapName = ApplicationResources.Map;

        AddMapViewVisible = true;
      }
    }

    /// <summary>
    /// Hides the Add Map (view); callback from the HideAddMapViewCommand
    /// </summary>
    private void HideAddMap()
    {
      AddMapViewVisible = false;
    }
    #endregion

    #region API
    /// <summary>
    /// A filter that should return a value to indicate which definitions are fine to
    /// built a new with
    /// </summary>
    bool MapDefinitionFilter(MapDefinition definition)
    {
      if (definition.ServiceProvider == null || definition.ServiceProvider.ProviderType != ServiceProviderType.XY)
      {
        // Any non-XY map-definition is not a candidate for defining a new Map on
        return false;
      }

      if (definition.IsMulti || !definition.IsGeographic)
      {
        // Multi-maps and local maps are not candidates
        return false;
      }

      return true;
    }

    /// <summary>
    /// Callback when a Map definition property changes
    /// </summary>
    void MapDefinitionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == MapDefinitionComboBoxViewModel.SelectedMapDefinitionPropertyName)
      {
        CheckCommands();
      }
    }

    /// <summary>
    /// Refilters the list of available map definitions
    /// </summary>
    internal void Refilter()
    {
      Refilter(false);
    }

    /// <summary>
    /// Refilters the list of available map definitions
    /// </summary>
    private void Refilter(bool forceHideAddMap)
    {
      MapDefinitionViewModel.Refilter();

      CheckCommands();

      if (forceHideAddMap || !CanAddMap)
      {
        HideAddMap();
      }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Holds the current map
    /// </summary>
    private LiteMapsViewModel Maps { get; set; }

    /// <summary>
    /// Holds the view model with all map Definitions that can be added
    /// </summary>
    public MapDefinitionComboBoxViewModel MapDefinitionViewModel { get; private set; }

    /// <summary>
    /// Returns the label for 'Map:'
    /// </summary>
    public string NewMapLabel
    {
      get { return ApplicationResources.Map; }
    }

    /// <summary>
    /// The map name
    /// </summary>
    public string MapName
    {
      get { return _mapName; }
      set
      {
        if (_mapName != value)
        {
          _mapName = value;
          RaisePropertyChanged(MapNamePropertyName);
        }
      }
    }

    /// <summary>
    /// Is the add map view visible
    /// </summary>
    public bool AddMapViewVisible
    {
      get { return _addMapViewVisible; }
      set
      {
        if (value != _addMapViewVisible)
        {
          _addMapViewVisible = value;

          CheckCommands();

          RaisePropertyChanged(AddMapViewVisiblePropertyName);
          RaisePropertyChanged(AddMapViewVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Is the add map view area visible
    /// </summary>
    public Visibility AddMapViewVisibility
    {
      get { return (AddMapViewVisible) ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
