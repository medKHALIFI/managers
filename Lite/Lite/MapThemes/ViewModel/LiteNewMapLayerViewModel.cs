using System;
using System.Windows;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Maps;
using SpatialEye.Framework.Geometry;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The LiteNewMapLayerViewModel holds the logic for adding a New MapLayer to
  /// the current Map (viewModel).
  /// </summary>
  public class LiteNewMapLayerViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The Add MapLayer bits are visible when the user requests to show the Add Map Layer View
    /// </summary>
    public const string AddMapLayerViewVisiblePropertyName = "AddMapLayerViewVisible";

    /// <summary>
    /// The corresponding visibility that is dependent on the flag
    /// </summary>
    public const string AddMapLayerViewVisibilityPropertyName = "AddMapLayerViewVisibility";
    #endregion

    #region Private Fields
    /// <summary>
    /// Is the add MapLayer visible
    /// </summary>
    private Boolean _addMapLayerViewVisible;
    #endregion

    #region Constructors

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="messenger"></param>
    public LiteNewMapLayerViewModel(LiteMapViewModel mapToAddTo, Messenger messenger = null)
      : base(messenger)
    {
      this.CurrentMap = mapToAddTo;

      MapLayerDefinitionViewModel = new MapLayerDefinitionComboBoxViewModel(messenger);
      MapLayerDefinitionViewModel.ServiceProviderGroupTypeProperties.AllowedGroupTypes = new ServiceProviderGroupType[] { ServiceProviderGroupType.Business, ServiceProviderGroupType.Analysis, ServiceProviderGroupType.AddIn };
      MapLayerDefinitionViewModel.PropertyChanged += MapLayerDefinitionViewModel_PropertyChanged;

      // If there is a single Analysis Layer, hide it (it will be the same name as the Analysis itself)
      MapLayerDefinitionViewModel.DatumProperties.HideWhenSingleDatumFor = (serviceProvider, serviceProviderGroupType) => serviceProviderGroupType == ServiceProviderGroupType.Analysis;
      MapLayerDefinitionViewModel.MapLayerDefinitionsFilter = this.MapLayerDefinitionFilter;

      SetCurrentCultureLabels();

      SetupCommands();

      // Set the visibility of the AddMapLayer possibility for easy binding
      this.AddMapLayerVisibility = CurrentMap.MapType == LiteMapType.User ? Visibility.Visible : Visibility.Collapsed;
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
      MapLayerDefinitionViewModel.ServiceProviderProperties.Name = ApplicationResources.LayersNewLayerServiceProvider;
      MapLayerDefinitionViewModel.ServiceProviderGroupTypeProperties.Name = ApplicationResources.LayersNewLayerServiceProviderGroupType;
      MapLayerDefinitionViewModel.ServiceProviderGroupProperties.Name = ApplicationResources.LayersNewLayerServiceProviderLayer;
      MapLayerDefinitionViewModel.DatumProperties.Name = ApplicationResources.LayersNewLayerServiceProviderLayer;
    }
    #endregion

    #region Commands
    /// <summary>
    /// The Add Map Layer Command
    /// </summary>
    public RelayCommand AddMapLayerCommand { get; private set; }

    /// <summary>
    /// The Show Add Map LayerView command
    /// </summary>
    public RelayCommand ShowAddMapLayerViewCommand { get; private set; }

    /// <summary>
    /// The command for hiding the active map layer view
    /// </summary>
    public RelayCommand HideAddMapLayerViewCommand { get; private set; }
    #endregion

    #region API
    /// <summary>
    /// Setup the commands for this viewmodel
    /// </summary>
    private void SetupCommands()
    {
      AddMapLayerCommand = new RelayCommand(() => { AddMapLayer(); }, () => { return MapLayerDefinitionViewModel.SelectedMapLayerDefinition != null; });
      ShowAddMapLayerViewCommand = new RelayCommand(ShowAddMapLayer, () => { return !AddMapLayerViewVisible && CanAddMapLayer; });
      HideAddMapLayerViewCommand = new RelayCommand(HideAddMapLayer, () => { return AddMapLayerViewVisible; });
    }

    /// <summary>
    /// Check the executable state of the commands
    /// </summary>
    private void CheckCommands()
    {
      AddMapLayerCommand.RaiseCanExecuteChanged();
      ShowAddMapLayerViewCommand.RaiseCanExecuteChanged();
      HideAddMapLayerViewCommand.RaiseCanExecuteChanged();
    }


    /// <summary>
    /// Returns a flag indicating whether we can display the lot
    /// </summary>
    public bool CanAddMapLayer
    {
      get { return MapLayerDefinitionViewModel.ActiveDatums != null && MapLayerDefinitionViewModel.ActiveDatums.Count > 0; }
    }

    /// <summary>
    /// Adds the selected maplayer to the map
    /// </summary>
    private void AddMapLayer()
    {
      var newLayerDefinition = this.MapLayerDefinitionViewModel.SelectedMapLayerDefinition;
      if (newLayerDefinition != null)
      {
        var mapLayerViewModel = MapLayerViewModel.ViewModelFor(newLayerDefinition);

        mapLayerViewModel.IsOn = true;
        bool ontop = newLayerDefinition.IsSelectable;

        if (ontop)
        {
          this.CurrentMap.Layers.Insert(0, mapLayerViewModel);
        }
        else
        {
          this.CurrentMap.Layers.Add(mapLayerViewModel);
        }
      }

      // Force the checking of the filter, since it has changed
      // Automatically force hiding the panel
      Refilter(true);
    }

    /// <summary>
    /// Shows the Add Map Layer (view); callback from the ShowAddMapLayerViewCommand
    /// </summary>
    private void ShowAddMapLayer()
    {
      if (CanAddMapLayer)
      {
        AddMapLayerViewVisible = true;
      }
    }

    /// <summary>
    /// Hides the Add Map Layer (view); callback from the HideAddMapLayerViewCommand
    /// </summary>
    private void HideAddMapLayer()
    {
      AddMapLayerViewVisible = false;
    }

    /// <summary>
    /// A filter that should return a value to indicate which layers have been already been added
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    bool MapLayerDefinitionFilter(MapLayerDefinition layer)
    {
      Universe allowedUniverse = null;
      if (CurrentMap != null)
      {
        var world = CurrentMap.World;
        var uni = world != null ? world.Universe : null;
        bool csIsLocal = uni != null ? uni.HasLocalCoordinateSystems : false;

        if (CurrentMap.World != null && CurrentMap.World.Universe != null && CurrentMap.World.Universe.IsMultiWorld)
        {
          allowedUniverse = CurrentMap.World.Universe;
        }

        var layerUniverse = layer != null ? layer.Universe : null;
        var layerUniverseIsLocal = layerUniverse != null && layerUniverse.HasLocalCoordinateSystems;

        if (allowedUniverse != null && (layerUniverse == null || layerUniverse != allowedUniverse))
        {
          // There is a specific universe involved
          return false;
        }

        if (csIsLocal != layerUniverseIsLocal)
        {
          // Local/Geographic mismatch
          return false;
        }

        foreach (var layerViewModel in CurrentMap.Layers)
        {
          if (layerViewModel.LayerDefinition != null && layerViewModel.LayerDefinition.ServiceProviderGroup.Equals(layer.ServiceProviderGroup) && layerViewModel.LayerDefinition.Name.Equals(layer.Name))
          {
            return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Callback when a Maplayer definition property changes
    /// </summary>
    void MapLayerDefinitionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == MapLayerDefinitionComboBoxViewModel.SelectedMapLayerDefinitionPropertyName)
      {
        CheckCommands();
      }
    }

    /// <summary>
    /// Refilters the list of available map layer definitions
    /// </summary>
    internal void Refilter()
    {
      Refilter(false);
    }

    /// <summary>
    /// Refilters the list of available map layer definitions
    /// </summary>
    private void Refilter(bool forceHideAddMapLayer)
    {
      MapLayerDefinitionViewModel.Refilter();

      CheckCommands();

      if (forceHideAddMapLayer || !CanAddMapLayer)
      {
        HideAddMapLayer();
      }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Holds the current map
    /// </summary>
    private LiteMapViewModel CurrentMap { get; set; }

    /// <summary>
    /// The visibility to add a layer
    /// </summary>
    public Visibility AddMapLayerVisibility { get; private set; }

    /// <summary>
    /// Holds the view model with all mapLayer Definitions that can be added
    /// </summary>
    public MapLayerDefinitionComboBoxViewModel MapLayerDefinitionViewModel { get; private set; }

    /// <summary>
    /// Is the add map layer view visible
    /// </summary>
    public bool AddMapLayerViewVisible
    {
      get { return _addMapLayerViewVisible; }
      set
      {
        if (value != _addMapLayerViewVisible)
        {
          _addMapLayerViewVisible = value;

          CheckCommands();

          RaisePropertyChanged(AddMapLayerViewVisiblePropertyName);
          RaisePropertyChanged(AddMapLayerViewVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Is the add map layer view area visible
    /// </summary>
    public Visibility AddMapLayerViewVisibility
    {
      get { return (AddMapLayerViewVisible) ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
