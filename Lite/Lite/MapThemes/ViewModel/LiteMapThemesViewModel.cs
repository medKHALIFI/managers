using System.Windows;

using SpatialEye.Framework.Client;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The Lite ThemesViewModel holds the logic for controlling visibility/opacity
  /// of individual layers as well as the selected mode for each layer.
  /// It allows for adding themes via the logic as exposed via the LiteNewMapLayerViewModel
  /// that is exposed via this viewModel.
  /// Most of the logic is inherited from the MapThemesViewModel that is set up in the
  /// clientToolkit
  /// </summary>
  public class LiteMapThemesViewModel : MapThemesViewModel
  {
    #region Property Names
    /// <summary>
    /// Holds the (property name) of the NewMapLayer ViewModel
    /// </summary>
    public const string NewMapLayerViewModelPropertyName = "NewMapLayerViewModel";

    /// <summary>
    /// The description of the Map Layers label
    /// </summary>
    public const string MapLayersDescriptionPropertyName = "MapLayersDescription";
    #endregion

    #region Private Fields
    /// <summary>
    /// The NewMapLayer view model that holds logic for adding a new layer
    /// </summary>
    private LiteNewMapLayerViewModel _newMapLayerViewModel;

    /// <summary>
    /// The description of the active layers
    /// </summary>
    private string _mapLayersDescription;
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="messenger"></param>
    public LiteMapThemesViewModel(Messenger messenger = null)
      : base(messenger)
    {
      AttachToMessenger();

      this.Resources = new ApplicationResources();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the message bus (get on the bus)
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, MapViewModelChanged);
      }
    }

    /// <summary>
    /// Callback from the the messenger when the mapview model changes
    /// </summary>
    private void MapViewModelChanged(PropertyChangedMessage<LiteMapViewModel> mapViewModelMessage)
    {
      if (mapViewModelMessage.PropertyName == LiteMapsViewModel.CurrentMapPropertyName)
      {
        var model = mapViewModelMessage.NewValue;

        if (model != null)
        {
          this.MapViewModel = model;

          // Set the newMapLayerViewModel to one that knows about the new MapViewModel
          NewMapLayerViewModel = new LiteNewMapLayerViewModel(model, Messenger);

          var removeVisibility = model.MapType == LiteMapType.User ? Visibility.Visible : Visibility.Collapsed;
          foreach (var themeLayer in this.ThemeLayers)
          {
            themeLayer.RemoveVisibility = removeVisibility;
          }
        }
      }
    }

    /// <summary>
    /// Change notification for the active MapView
    /// </summary>
    /// <param name="mapViewModel">The new active mapViewModel</param>
    protected override void OnMapViewChanged(MapViewModel mapViewModel)
    {
      var liteMapViewModel = mapViewModel as LiteMapViewModel;
      if (mapViewModel != null)
      {
        var formatString = ApplicationResources.MapLayersDescriptionFormat;
        this.MapLayersDescription = string.Format(formatString, mapViewModel.ExternalName);
      }
      else
      {
        this.MapLayersDescription = SpatialEye.Framework.Client.Resources.FrameworkResources.ThemesLayers;
      }

      base.OnMapViewChanged(mapViewModel);
    }
    #endregion

    #region Public Api
    /// <summary>
    /// Viewmodel for adding new map layers
    /// </summary>
    public LiteNewMapLayerViewModel NewMapLayerViewModel
    {
      get { return _newMapLayerViewModel; }
      private set
      {
        if (value != _newMapLayerViewModel)
        {
          _newMapLayerViewModel = value;
          RaisePropertyChanged(NewMapLayerViewModelPropertyName);
        }
      }
    }

    /// <summary>
    /// Callback for removal of a layerViewModel 
    /// </summary>
    /// <param name="layerViewModel"></param>
    public override void Remove(MapThemeLayerViewModel layerViewModel)
    {
      // Removes the layerViewModel
      base.Remove(layerViewModel);

      // Make sure we check all available elements again
      this.NewMapLayerViewModel.Refilter();
    }

    /// <summary>
    /// Holds the description of the active Map's Layers
    /// </summary>
    public string MapLayersDescription
    {
      get { return _mapLayersDescription; }
      set
      {
        if (_mapLayersDescription != value)
        {
          _mapLayersDescription = value;
          RaisePropertyChanged(MapLayersDescriptionPropertyName);
        }
      }
    }
    #endregion

    #region Layer presenters
    /// <summary>
    /// Returns a new MapThemeLayerViewModel (or subclass) that represents the layer
    /// in the Themes View
    /// </summary>
    protected override MapThemeLayerViewModel NewThemeLayerViewModelFor(MapViewModel mapViewModel, MapLayerViewModel layer)
    {
      return new LiteMapThemeLayerViewModel(layer);
    }
    #endregion
  }
}
