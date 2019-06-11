using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Maps;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Geometry.CoordinateSystems;
using SpatialEye.Framework.Features.Recipe;
using SpatialEye.Framework.Maps.Services;
using SpatialEye.Framework.ServiceProviders.XY;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Client.Styles;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// A view model controlling multiple maps (mapViewModel instances). The maps
  /// can be accessed via the Maps property and there is the notion of a CurrentMap; 
  /// the latter can be used for binding to a MapControl to allow dynamic switching
  /// of the Map content by changing the CurrentMap of this MapsViewModel.
  /// </summary>
  public partial class LiteMapsViewModel : ViewModelBase
  {
    #region Static properties
    /// <summary>
    /// Is the MapsViewModel busy getting the definitions
    /// </summary>
    public const string IsBusyPropertyName = "IsBusy";

    /// <summary>
    /// The Maps
    /// </summary>
    public const string MapsPropertyName = "Maps";

    /// <summary>
    /// The Current Map
    /// </summary>
    public const string CurrentMapPropertyName = "CurrentMap";

    /// <summary>
    /// Is there a current map
    /// </summary>
    public const string HasCurrentMapPropertyName = "HasCurrentMap";

    /// <summary>
    /// Is there a current map
    /// </summary>
    public const string CurrentMapVisibilityPropertyName = "CurrentMapVisibility";

    /// <summary>
    /// The Previous World Map, to use to jump out of an internal world
    /// </summary>
    public const string PreviousWorldMapPropertyName = "PreviousWorldMap";

    /// <summary>
    /// Is there a previous map to jump to
    /// </summary>
    public const string HasPreviousWorldMapPropertyName = "HasPreviousWorldMap";

    /// <summary>
    /// The Previous World Map, to use to jump out of an internal world
    /// </summary>
    public const string PreviousWorldMapVisibilityPropertyName = "PreviousWorldMapVisibility";

    /// <summary>
    /// The named Spatial Reference IDs that are available; which are key-value
    /// pairs of a SRID (integer) and a descriptive Name (string)
    /// </summary>
    public const string EpsgCoordinateSystemsPropertyName = "EpsgCoordinateSystems";

    /// <summary>
    /// The custom maps visibility
    /// </summary>
    public const string CustomMapsVisibilityPropertyName = "CustomMapsVisibility";

    /// <summary>
    /// The name of the restriction layer
    /// </summary>
    public const string RestrictionMapLayerName = "restrictionareas";

    /// <summary>
    /// The external name of the restriction layer
    /// </summary>
    public const string RestrictionMapLayerExternalName = "Restriction Areas";

    /// <summary>
    /// The coverage field of the restriction layer
    /// </summary>
    public const string RestrictionMapLayerGeomFieldName = "Coverage";

    /// <summary>
    /// The name field of the restriction layer
    /// </summary>
    public const string RestrictionMapLayerNameFieldName = "Name";
    #endregion

    #region Fields
    /// <summary>
    /// An observable collection holding all mapDefinitions that have been provided
    /// by the service providers. Each map definition holds the layer definitions that
    /// the service provider has set up for that map.
    /// </summary>
    private ObservableCollection<MapDefinition> _serviceProviderMapDefinitions;

    /// <summary>
    /// Holds the map definitions as they are going to be used in this mapsViewModel.
    /// Each mapDefinition will/can be set up using layers coming from multiple service
    /// providers, thus allowing a client map(definition) that spans multiple providers.
    /// Each map definition will eventually result in a MapViewModel that is a candidate
    /// for being shown in a MapControl.
    /// </summary>
    private ObservableCollection<MapDefinition> _mapDefinitions;

    /// <summary>
    /// The maps, holding the MapViewModel instances that can be shown 
    /// </summary>
    private SortedObservableCollection<LiteMapViewModel> _maps;

    /// <summary>
    /// The current map (view Model) as should be shown in the MapControl that looks at this mapsViewModel
    /// </summary>
    private LiteMapViewModel _currentMap;

    /// <summary>
    /// The map to return to when inside an internals map; this is not a true bread crumb, but merely
    /// the last non internals map that was visited. When visiting a non-internals map, the previous
    /// map will automatically be reset to null.
    /// </summary>
    private LiteMapViewModel _previousWorldMap;

    /// <summary>
    /// Holds a flag indicating whether this viewmodel is busy
    /// </summary>
    private bool _isBusy;

    /// <summary>
    /// Holds the minimum zoom level that will be requested for any map. This means
    /// the user can not zoom any more out than the value specified here.
    /// Valid range = [1, 15]. The default is 1.
    /// </summary>
    private int _minZoomLevel = 1;

    /// <summary>
    /// Holds the maximum zoom level that will be requested for any map. This means
    /// the user can not zoom any deeper than the value specified here.
    /// Valid range = [1, 23], although sensibly the maximum zoom level should be
    /// in the range [21, 23]. The default value is 22.
    /// </summary>
    private int _maxZoomLevel = 22;

    /// <summary>
    /// The named Spatial Reference IDs that are available; which are key-value
    /// pairs of a SRID (integer) and a descriptive Name (string)
    /// </summary>
    private EpsgCoordinateSystemReferenceCollection _epsgCoordinateSystems;

    /// <summary>
    /// Do we allow custom maps
    /// </summary>
    private bool _isCustomMapsVisible = true;

    /// <summary>
    /// The last selection mode
    /// </summary>
    private LiteMapSelectInteractionMode _mainSelectionMode;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the MapViewModel for the Lite application
    /// </summary>
    public LiteMapsViewModel(Messenger messenger = null)
      : base(messenger)
    {
      AttachToMessenger();

      SetupCommands();

      SetupInteractionHandler();

      // Set up the Resources to use
      Resources = new ApplicationResources();

      // Set to empty maps; which will be set up later to the actual values
      this.Maps = new SortedObservableCollection<LiteMapViewModel>(LiteMapViewModel.LiteMapViewModelComparer);

      // The view model for new maps
      this.NewMapViewModel = new LiteNewUserMapViewModel(this);
    }
    #endregion

    #region Authentication Changed
    /// <summary>
    /// Callback for changes in authentication (context)
    /// </summary>
    /// <param name="context">The new authentication context</param>
    /// <param name="isAuthenticated">A flag indicating success of authentication</param>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      // Get the allow custom maps setting
      this.IsCustomMapsVisible = LiteClientSettingsViewModel.Instance.AllowCustomMaps;
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attaches the MapsViewModel to the messenger, responding to requests for jumping to bounds
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        this.Messenger.Register<LiteGoToGeometryRequestMessage>(this, HandleGoToGeometryRequest);
        this.Messenger.Register<LiteGoToWorldRequestMessage>(this, (r) => { var ignored = HandleGoToWorldRequest(r); });
        this.Messenger.Register<LiteHighlightGeometryRequestMessage>(this, HandleHighlightGeometryRequest);
        this.Messenger.Register<LiteFeatureTransactionMessage>(this, HandleTransactionChangeMessage);
        this.Messenger.Register<LiteMapLayerChangeMessage>(this, HandleMapLayerChangedMessage);
      }
    }

    /// <summary>
    /// Handles the messenger's GoTo Envelope request, being put on the Messenger by
    /// the activation view model.
    /// </summary>
    private async void HandleGoToGeometryRequest(LiteGoToGeometryRequestMessage request)
    {
      // The maximum zoom level that will be used for going to the geometry
      const int maxGotoGeometryZoomLevel = 18;

      // Get the Current Map and envelope
      var envelope = request.Envelope;
      if (CurrentMap != null && envelope != null)
      {
        var envelopeWorld = envelope.World;
        if (envelopeWorld != null)
        {
          if (!envelopeWorld.Equals(CurrentMap.World))
          {
            // The worlds are not similar - let's jump to the correct world
            if (envelopeWorld.Universe.HasGeographicCoordinateSystems && !envelopeWorld.IsPartOfMultiWorld)
            {
              if (PreviousWorldMap != null)
              {
                // Go-to a top-level geographic world - in this case, the previous world
                CurrentMap = PreviousWorldMap;
              }
              else
              {
                // Set the map to the startup map
                CurrentMap = StartupMap;
              }
            }
            else
            {
              var jumpToWorldRequest = new LiteGoToWorldRequestMessage(request.Sender, envelopeWorld);
              await HandleGoToWorldRequest(jumpToWorldRequest);
            }

            // Give some time to the UI to do some work
            await TaskFunctions.Yield();
          }
        }

        // Get the Coordinate System
        var cs = envelope.CoordinateSystem;
        if (cs != null && cs == CurrentMap.CoordinateSystem)
        {
          // Enlarge to take care of envelope rounding (because of discrete zoom levels)
          envelope = envelope.EnlargedBy(1.5);

          // Determine the zoom-level
          var zoomLevel = CurrentMap.ZoomLevelForEnvelope(envelope);
          if (zoomLevel > maxGotoGeometryZoomLevel) zoomLevel = maxGotoGeometryZoomLevel;

          // Determine the envelope to jump to
          var newEnvelope = CurrentMap.EnvelopeForZoomLevel(envelope.Centre, zoomLevel);

          // Set the new envelope
          CurrentMap.SetEnvelope(newEnvelope, request.DoRocketJump);

          if (request.DoHighlight)
          {
            HandleHighlightGeometryRequest(request.ToHighlightRequest());
          }
        }
      }
    }

    /// <summary>
    /// Handles the messenger's GoTo World request, being put on the Messenger by
    /// the activation view model.
    /// </summary>
    private async Task HandleGoToWorldRequest(LiteGoToWorldRequestMessage request)
    {
      if (CurrentMap != null && CurrentMap.World.Equals(request.World))
      {
        // Nothing to do; we are already in the right world
      }
      else
      {
        var world = request.World;
        if (world != null && world.Universe != null && world.Universe.IsMultiWorld)
        {
          LiteMapViewModel useMap = null;
          foreach (var map in this.Maps)
          {
            if (map.World.Equals(request.World))
            {
              useMap = map;
              break;
            }
          }

          if (useMap != null)
          {
            // There already is a map that has been set up for this World; let's use it
            CurrentMap = useMap;
          }
          else
          {
            // The universe to go to
            var universe = world.Universe;

            // Get the Envelope
            var mapService = GetService<IMapService>(ServiceProviderManager.Instance.MainServiceProvider);
            var envelope = await mapService.GetWorldDefaultEnvelopeAsync(world);

            if (envelope != null)
            {
              var candidateMapDefinitions = MapDefinitionCandidatesFor(universe);

              if (candidateMapDefinitions.Count > 0)
              {
                // Maybe, we should allow the end-user to choose where to jump to
                var mapDefinition = candidateMapDefinitions[0];

                foreach (var layerDefinition in mapDefinition.Layers)
                {
                  layerDefinition.DefaultVisible = layerDefinition.IsSelectable;
                }

                // Create a new map
                var newMap = new LiteMapViewModel(this.Messenger, mapDefinition, InteractionHandler, this.EpsgCoordinateSystems, world, envelope, request.Owner);

                // Add the new map and make it current
                this.Maps.Add(newMap);
                this.CurrentMap = newMap;

                // Check commands
                CheckCommands();
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Handles the messenger's Highlight Geometry request, making the requested geometry the focused 
    /// geometry in the active map.
    /// </summary>
    private void HandleHighlightGeometryRequest(LiteHighlightGeometryRequestMessage request)
    {
      if (CurrentMap != null)
      {
        var highlight = request.FeatureTargetGeometryFor(CurrentMap.World);
        CurrentMap.HighlightFeatureGeometry = highlight;
      }
    }

    /// <summary>
    /// Handle a transaction message
    /// </summary>
    private void HandleTransactionChangeMessage(LiteFeatureTransactionMessage message)
    {
      if (CurrentMap != null)
      {
        // We have a map; now respond to a delete message
        if (message.Type == LiteFeatureTransactionMessage.TransactionType.Deleted)
        {
          // Clear the selection
          CurrentMap.ClearSelection();
        }
      }
    }

    /// <summary>
    /// Handle the server push event of changed map layers; ask the active mapViewModel
    /// to process these.
    /// </summary>
    private void HandleMapLayerChangedMessage(LiteMapLayerChangeMessage message)
    {
      var currentMap = CurrentMap;
      if (currentMap != null)
      {
        currentMap.ProcessMapLayerChanges(message.Changes);
      }
    }
    #endregion

    #region Commands
    /// <summary>
    /// Set up the the commands that can be used from within buttons
    /// </summary>
    private void SetupCommands()
    {
      // Go-To the startup view of the current map
      GoToHomeViewCommand = new RelayCommand(GoToHomeView, () => CanGoToHomeView());

      // Go-To the extent of the current selection
      GoToSelectionCommand = new RelayCommand(GoToSelection, () => CanGoToSelection());

      // Clear the current selection
      ClearSelectionCommand = new RelayCommand(ClearSelection, () => CanClearSelection());

      // Go-To the previous world map
      GoToPreviousWorldMapCommand = new RelayCommand(GoToPreviousWorldMap, () => CanGoToPreviousWorldMap());
    }

    /// <summary>
    /// Checks the commands' enabled states
    /// </summary>
    private void CheckCommands()
    {
      GoToHomeViewCommand.RaiseCanExecuteChanged();
      GoToSelectionCommand.RaiseCanExecuteChanged();
      ClearSelectionCommand.RaiseCanExecuteChanged();
      GoToPreviousWorldMapCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// The Go-To Home View Command, jumping to the home-view of the active map definition
    /// </summary>
    public RelayCommand GoToHomeViewCommand { get; private set; }

    /// <summary>
    /// The Go-To Selection Command, jumping to the Envelope of the current selection
    /// </summary>
    public RelayCommand GoToSelectionCommand { get; private set; }

    /// <summary>
    /// The Clear Selection Command, clearing all selected geometry from the active map
    /// </summary>
    public RelayCommand ClearSelectionCommand { get; private set; }

    /// <summary>
    /// The Go-To Previous World Map Command, leaving the active internals map
    /// </summary>
    public RelayCommand GoToPreviousWorldMapCommand { get; private set; }

    /// <summary>
    /// A (calculated) flag, indicating whether it is possible to jump to the home view;
    /// this is only possible in case there is a current map that has a default envelope 
    /// set up 
    /// </summary>
    /// <returns></returns>
    private bool CanGoToHomeView()
    {
      return CurrentMap != null && CurrentMap.DefaultEnvelope != null;
    }

    /// <summary>
    /// Go-To the home view, setting the default envelope of the current map
    /// as its active envelope
    /// </summary>
    private void GoToHomeView()
    {
      var envelope = CurrentMap != null ? CurrentMap.DefaultEnvelope : null;
      if (envelope != null)
      {
        Messenger.Send(new LiteGoToGeometryRequestMessage(CurrentMap, envelope));
      }
    }

    /// <summary>
    /// Returns a (calculated) flag that indicates whether there is a current
    /// selection that can be jumped to
    /// </summary>
    /// <returns></returns>
    private bool CanGoToSelection()
    {
      var canGoToSelection = false;
      if (CurrentMap != null && CurrentMap.SelectedFeatures.Count > 0)
      {
        // Get the envelope directly from the first feature
        var envelope = CurrentMap.SelectedFeatures[0].GetEnvelope(CurrentMap.World);

        // Is there an envelope available to go to
        canGoToSelection = envelope != null;
      }

      return canGoToSelection;
    }

    /// <summary>
    /// Actually goes to the active selection of the map
    /// </summary>
    public void GoToSelection()
    {
      if (CurrentMap != null && CurrentMap.SelectedFeatures.Count > 0)
      {
        // There is a feature
        var feature = CurrentMap.SelectedFeatures[0];

        // Pick up the feature's envelope
        var envelope = feature.GetEnvelope();
        if (envelope != null)
        {
          // Request a go-to envelope and put it on the messenger
          Messenger.Send(new LiteGoToGeometryRequestMessage(CurrentMap, envelope, feature));
        }
      }
    }

    /// <summary>
    /// Returns a flag indicating whether there is a selection that can be cleared
    /// </summary>
    private bool CanClearSelection()
    {
      return (CurrentMap != null && (CurrentMap.SelectedFeatures.Count > 0 || CurrentMap.HighlightFeatureGeometry != null));
    }

    /// <summary>
    /// Clears the selection of the current map
    /// </summary>
    private void ClearSelection()
    {
      if (CurrentMap != null)
      {
        CurrentMap.ClearSelection();
      }
    }

    /// <summary>
    /// Go To the previously visited world map
    /// </summary>
    private void GoToPreviousWorldMap()
    {
      if (_previousWorldMap != null)
      {
        this.CurrentMap = _previousWorldMap;
      }
    }

    /// <summary>
    /// We can go to 
    /// </summary>
    /// <returns></returns>
    private bool CanGoToPreviousWorldMap()
    {
      return _previousWorldMap != null;
    }

    /// <summary>
    /// Flag indicating whether the Custom Map button is visible
    /// </summary>
    public bool IsCustomMapsVisible
    {
      get { return _isCustomMapsVisible; }
      set
      {
        if (_isCustomMapsVisible != value)
        {
          _isCustomMapsVisible = value;
          RaisePropertyChanged(CustomMapsVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// The custom map button's visibility
    /// </summary>
    public Visibility CustomMapsVisibility
    {
      get { return _isCustomMapsVisible ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion

    #region Interaction Handler set up
    /// <summary>
    /// Sets up the Interaction Handler to be used; normally the default interaction
    /// handler would suffice, but the created one has a couple of extra interaction
    /// modes for zooming that can be controlled via keyboard as well as mouse.
    /// </summary>
    private void SetupInteractionHandler()
    {
      // Setup some static mouse wheel defaults
      MapInteractionHandler.ScrollWheelZoomDirection = MapInteractionHandler.ScrollWheelZoomDirectionType.ForwardIsZoomIn;
      MapInteractionHandler.DefaultScrollWheelZoomLocation = MapInteractionHandler.ScrollWheelZoomLocationType.KeepLocationUnderMouse;

      _mainSelectionMode = new LiteMapSelectInteractionMode();
      InteractionHandler = new MapInteractionHandler(_mainSelectionMode);

      // Zoom In/Out interaction modes are only available when multi selection is switched off
      if (!LiteClientSettingsViewModel.Instance.AllowMultiSelection)
      {
        // Fixed interaction modes, only available when actively selecting the lot
        var stroke = new SolidColorBrush(Color.FromArgb(204, 102, 102, 204));
        var strokeThickness = 2;
        InteractionHandler.InteractionModes.Add(new MapZoomInOutInteractionMode(zoomIn: true) { Stroke = stroke, StrokeThickness = strokeThickness });
        InteractionHandler.InteractionModes.Add(new MapZoomInOutInteractionMode(zoomIn: false) { Stroke = stroke, StrokeThickness = strokeThickness });

        // A loose interaction mode that can react to modifier keys
        InteractionHandler.InteractionModes.Add(new MapZoomInOutInteractionMode() { Stroke = stroke, StrokeThickness = strokeThickness });
      }
    }
    #endregion

    #region Property Change handlers
    /// <summary>
    /// Attach all maps, making sure we respond to collection changes as well
    /// as changes in individual maps
    /// </summary>
    private void AttachMaps()
    {
      if (_maps != null)
      {
        _maps.CollectionChanged += MapsCollectionChanged;

        foreach (var map in _maps)
        {
          AttachMap(map);
        }
      }
    }

    /// <summary>
    /// Detach from the maps, since we are setting up a new maps collection.
    /// Detach from all individual maps as well
    /// </summary>
    private void DetachMaps()
    {
      if (_maps != null)
      {
        _maps.CollectionChanged -= MapsCollectionChanged;

        foreach (var query in _maps)
        {
          DetachMap(query);
        }
      }
    }

    /// <summary>
    /// Called whenever the collection of active Maps changes
    /// </summary>
    void MapsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (LiteMapViewModel item in e.OldItems)
        {
          DetachMap(item);
        }
      }

      if (e.NewItems != null)
      {
        foreach (LiteMapViewModel item in e.NewItems)
        {
          AttachMap(item);
        }
      }
      SetHeadingIndent();
      SetCurrentMap();
    }

    /// <summary>
    /// Attach to the map's remove command
    /// </summary>
    /// <param name="map">The map to attach to</param>
    private void AttachMap(LiteMapViewModel map)
    {
      if (map != null)
      {
        map.RequestRemoveMap += RemoveUserMap;
        map.PropertyChanged += MapPropertyChanged;
      }
    }

    /// <summary>
    /// Detach from the map's remove command
    /// </summary>
    /// <param name="map">The map to detach from</param>
    private void DetachMap(LiteMapViewModel map)
    {
      if (map != null)
      {
        map.RequestRemoveMap -= RemoveUserMap;
        map.PropertyChanged -= MapPropertyChanged;

      }
    }

    /// <summary>
    /// Attach the current map, tracking appropriate property changes
    /// </summary>
    private void AttachCurrentMap()
    {
      _currentMap.InteractionHandler = InteractionHandler;
      _currentMap.PropertyChanged += CurrentMapPropertyChanged;
      _currentMap.Layers.CollectionChanged += CurrentMapLayersChanged;
    }

    /// <summary>
    /// Detach the current map, remove tracking of property changes
    /// </summary>
    private void DetachCurrentMap()
    {
      _currentMap.InteractionHandler = null;
      _currentMap.PropertyChanged -= CurrentMapPropertyChanged;
      _currentMap.Layers.CollectionChanged -= CurrentMapLayersChanged;
    }

    /// <summary>
    /// A property of the current map has changed; respond to the property change
    /// </summary>
    void CurrentMapPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == MapViewModel.SelectedFeatureGeometryPropertyName)
      {
        SendMapSelectionChanged();
        SendCurrentMapSelectionDisplayFeatureRequest();
        CheckCommands();
      }
      else if (e.PropertyName == MapViewModel.HighlightFeatureGeometryPropertyName)
      {
        CheckCommands();
      }
    }

    /// <summary>
    /// The layers of the active map have changed
    /// </summary>
    void CurrentMapLayersChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      var map = _currentMap;
      if (map != null && map.MapType == LiteMapType.User)
      {
        if (map.Layers.Count == 0)
        {
          // All layers of a User Map have been deleted; let's get rid of the
          // user map altogether
          var allProjectMaps = this.Maps.Where(m => m.MapType != LiteMapType.User).ToArray();
          if (allProjectMaps.Length > 1)
          {
            // Set the active map to the first project based map
            this.CurrentMap = allProjectMaps[0];

            // And remove the empty user map
            this.RemoveUserMap(map);
          }
        }
        else
        {
          // Layers there
          SaveUserMaps();
        }
      }
    }

    /// <summary>
    /// Sends a message on the databus about changes in the map selection
    /// </summary>
    private void SendMapSelectionChanged()
    {
      // A feature selected by means of its geometry
      var map = CurrentMap;

      if (map != null)
      {
        var selectedFeatures = map.SelectedFeatures;
        var selectedFeatureGeometry = map.SelectedFeatureGeometry;

        // Send the map selection on the bus
        Messenger.Send(new LiteMapSelectionChangedMessage(this, selectedFeatures, selectedFeatureGeometry));
      }
    }

    /// <summary>
    /// If the Current Map has a feature selected; send a request on the messenger
    /// to display that feature
    /// </summary>
    private void SendCurrentMapSelectionDisplayFeatureRequest()
    {
      // A feature selected by means of its geometry
      var value = CurrentMap.SelectedFeatures;

      if (value != null && value.Count > 0)
      {
        FeatureGeometryFieldDescriptor selectedGeometryField = null;
        var selectedFeatureGeometry = CurrentMap.SelectedFeatureGeometry;
        if (selectedFeatureGeometry.Count == 1)
        {
          selectedGeometryField = selectedFeatureGeometry[0].TargetGeometryFieldDescriptor;
        }

        this.Messenger.Send(new LiteDisplayFeatureDetailsRequestMessage(this, new List<IFeatureRecipeHolder>(value), selectedGeometryField));
      }
      else
      {
        // We've not selected anything (upon a click), ask another viewModel to carry out selection for the specified location.
        if (_mainSelectionMode != null && _mainSelectionMode.LastPressedMouseArgs != null)
        {
          // Get the pressed world location
          Messenger.Send(new LiteCustomSelectionRequestMessage(CurrentMap, _mainSelectionMode.LastPressedMouseArgs));
        }
      }

      // Send the current map's display cs as well
      if (CurrentMap != null)
      {
        Messenger.Send(new LiteDisplayCoordinateSystemChangedMessage(this, CurrentMap.SelectedEpsgCoordinateSystem));
      }
    }
    #endregion

    #region Internal Helpers
    /// <summary>
    /// Gets all Map Definitions asynchronously
    /// </summary>
    public async Task GetServiceProviderMapDefinitionsAsync()
    {
      IsBusy = true;
      try
      {
        // Await for all map layers definitions to be returned
        var mapService = GetService<IMapService>();

        // Get named areas, which can be used to restrict areas to draw/select for users
        var areas = await mapService.GetMapLayerAreasAsync();

        // Get map definitions
        var mapDefinitions = await mapService.GetMapDefinitionsAsync(ServiceProviderGroupType.Business, ServiceProviderGroupType.Analysis, ServiceProviderGroupType.AddIn);

        if (mapDefinitions != null)
        {
          ServiceProviderMapDefinitions = new ObservableCollection<MapDefinition>(mapDefinitions);

          // And restrict the areas in case there are restriction areas set up in the user's settings
          RestrictMapLayerAreas();
        }

        // Get all Named SRIDs
        await SetupEpsgCoordinateSystems();

        // Set the map definitions to be used client side
        await SetMapDefinitions();
      }
      finally
      {
        // Even if the setting up fails, we should still reset the busy flag
        IsBusy = false;
      }
    }

    /// <summary>
    /// Make sure that our set up map (layer) definitions are correctly restricted
    /// </summary>
    private void RestrictMapLayerAreas()
    {
      var serviceProviderMapDefinitions = ServiceProviderMapDefinitions;
      var restrictionAreas = RestrictionMapLayerAreas;

      if (serviceProviderMapDefinitions != null && restrictionAreas != null && restrictionAreas.Any())
      {
        foreach (var mapDefinition in serviceProviderMapDefinitions.Where(x => x.Universe.HasGeographicCoordinateSystems))
        {
          foreach (var layer in mapDefinition.Layers.OfType<XYTileMapLayerDefinition>())
          {
            layer.RestrictionAreas = new ObservableCollection<MapLayerArea>(restrictionAreas);
          }
        }
      }
    }

    /// <summary>
    /// Set up the named Spatial Reference IDs, which is a list of tuples that
    /// hold a SRID (integer) with a descriptive name (string). Those represent
    /// the set of used EPSG-coordinate systems in the active service provider.
    /// </summary>
    /// <returns></returns>
    private async Task SetupEpsgCoordinateSystems()
    {
      var mapService = GetService<IMapService>();
      this.EpsgCoordinateSystems = await mapService.GetEpsgCoordinateSystemsAsync();
    }

    /// <summary>
    /// From all available ServiceProvider Map Definitions there are, here the Map Definitions to be used
    /// are set up. Each map definition can be built up using layers from different service providers.
    /// </summary>
    private Task SetMapDefinitions()
    {
      var defaultMapDefinitions = new List<MapDefinition>();
      var mainServiceProvider = ServiceProviderManager.Instance.ServiceProviders.Any() ? ServiceProviderManager.Instance.ServiceProviders[0] : null;

      if (mainServiceProvider != null && ServiceProviderMapDefinitions != null)
      {
        // Get backdrop definitions
        var backdropDefinitions = ServiceProviderMapDefinitions.Where(m => m.IsServiceProviderType(ServiceProviderType.Tile, ServiceProviderType.Google, ServiceProviderType.WMS) && m.IsGeographic && !m.IsMulti).ToList();
        var mainDefinitions = ServiceProviderMapDefinitions.Where(m => m.IsDefinedIn(mainServiceProvider) && m.IsGeographic && !m.IsMulti && m.ServiceProviderGroup.GroupType != ServiceProviderGroupType.AddIn).ToList();

        backdropDefinitions.Sort((a, b) =>
        {
          var typeA = a.ServiceProvider.ProviderType;
          var typeB = b.ServiceProvider.ProviderType;

          if (typeA != typeB)
          {
            // Sort by serviceProvider type
            return ((int)typeA).CompareTo((int)typeB);
          }

          return a.Name.CompareTo(b.Name);
        });

        foreach (var mainDefinition in mainDefinitions)
        {
          var newDefinition = new MapDefinition(mainDefinition.Name, mainDefinition.ExternalName, defaultEnvelope: mainDefinition.DefaultEnvelope, openOnStartup: mainDefinition.OpenOnStartup)
          {
            ServiceProviderGroup = mainDefinition.ServiceProviderGroup,
            MinZoomLevel = this.MinZoomLevel,
            MaxZoomLevel = this.MaxZoomLevel
          };

          // Add Main foreground/background Layer
          foreach (var layer in mainDefinition.Layers)
          {
            newDefinition.Layers.Add(layer);
          }

          // Add all backdrops
          for (int nr = 0; nr < backdropDefinitions.Count; nr++)
          {
            var backdropLayer = backdropDefinitions[nr].Layers[0];
            backdropLayer.DefaultVisible = nr == 0;
            newDefinition.Layers.Add(backdropLayer);

            if (UseSingleBackdropLayer)
            {
              break;
            }
          }

          defaultMapDefinitions.Add(newDefinition);
        }
      }

      // Set the map definitions
      this.MapDefinitions = new ObservableCollection<MapDefinition>(defaultMapDefinitions);

      // And subsequently, set up all the maps (mapViewModel instances)
      return SetupMaps();
    }

    /// <summary>
    /// Sets up the Maps
    /// </summary>
    private async Task SetupMaps()
    {
      // Create the new maps
      var maps = new SortedObservableCollection<LiteMapViewModel>(LiteMapViewModel.LiteMapViewModelComparer);

      // Set up the Maps from the MapDefinitions
      foreach (var mapDefinition in this.MapDefinitions)
      {
        var liteMapViewModel = new LiteMapViewModel(this.Messenger, mapDefinition, InteractionHandler, this.EpsgCoordinateSystems);

        bool setXYOn = true;
        foreach (var layer in liteMapViewModel.Layers)
        {
          // Set initial visibility for the 1st layer coming from our server
          if (layer.LayerDefinition != null && layer.LayerDefinition.IsServiceProviderType(ServiceProviderType.XY))
          {
            layer.IsOn = setXYOn;
            setXYOn = false;
          }
        }

        maps.Add(liteMapViewModel);
      }

      // Add custom layers to the Maps/ ie, some client-side layers that need to be present on some of the/all maps
      await AddCustomLayers(maps);

      // Add the maps as set up locally by the user
      await AddUserMaps(maps);

      // Add restriction area layer (in case there are restriction areas)
      if (DisplayRestrictionAreas)
      {
        AddRestrictionAreaLayers(maps);
      }

      // Set the maps, which will automatically set the current map
      this.Maps = maps;
    }

    /// <summary>
    /// Add the restriction area layers
    /// </summary>
    private void AddRestrictionAreaLayers(ObservableCollection<LiteMapViewModel> maps)
    {
      if (maps.Count > 0 && RestrictionMapLayerAreas != null && RestrictionMapLayerAreas.Count > 0)
      {
        var tableDescriptor = new FeatureTableDescriptor(RestrictionMapLayerName, RestrictionMapLayerExternalName);
        var nameField = tableDescriptor.FieldDescriptors.Add(RestrictionMapLayerNameFieldName.ToLower(), RestrictionMapLayerNameFieldName, FeatureAlphaType.StringWith(100));
        var geomField = tableDescriptor.FieldDescriptors.Add(RestrictionMapLayerGeomFieldName.ToLower(), RestrictionMapLayerGeomFieldName, FeatureGeometryType.MultiCurve);

        var memoryCollection = tableDescriptor.NewInMemoryCollection();
        foreach (var area in RestrictionMapLayerAreas)
        {
          if (area.Geometry != null)
          {
            MultiCurve multiCurve = null;

            foreach (var p in area.Geometry)
            {
              if (multiCurve == null)
              {
                multiCurve = new MultiCurve(p.World, p.CoordinateSystem);
              }

              multiCurve.Add(new Curve(p.World, p.CoordinateSystem, p.ExteriorRing));
            }

            var values = new object[] { area.Name, multiCurve };
            var feature = new Feature(tableDescriptor, values);

            memoryCollection.Add(feature);
          }
        }

        if (memoryCollection.Count > 0)
        {
          var lineColor = Color.FromArgb(204, 51, 51, 102);
          var selectionColor = Color.FromArgb(153, 51, 51, 255);

          // Set up a Layer to display
          var lineStyle = new SimpleLineStyle()
          {
            Stroke = new SolidColorBrush(lineColor),
            Width = 5,
            SelectedEffectColor = selectionColor
          };

          var style = new MapLayerDirectStyle(RestrictionMapLayerGeomFieldName, lineStyle);

          foreach (var map in maps)
          {
            if (map.Universe.HasGeographicCoordinateSystems && !map.IsMultiWorld)
            {
              var layer = new FeatureGeometryMapLayerViewModel(RestrictionMapLayerName, RestrictionMapLayerExternalName)
              {
                GeometryField = geomField,
                Features = memoryCollection,
                IsSelectable = false,
                IsOn = true,
                Style = style,
                VisibleScaleRange = new MapScaleRange(2000000)
              };

              map.Layers.Insert(0, layer);
            }
          }
        }
      }
    }

    /// <summary>
    /// Sets the selected map
    /// </summary>
    private void SetCurrentMap()
    {
      var maps = _maps;

      if (maps != null)
      {
        var currentMap = CurrentMap;
        if (currentMap == null || !maps.Contains(currentMap))
        {
          // Set to a new value
          currentMap = maps.Count > 0 ? maps[0] : null;

          if (maps.Count > 0)
          {
            // Do check for an OpenOnStartup map as the prefered map to set
            var preferedStartUpMap = maps.FirstOrDefault(m => m.Properties.OpenOnStartup);
            if (preferedStartUpMap != null)
            {
              currentMap = preferedStartUpMap;
            }
          }

          CurrentMap = currentMap;

          if (StartupMap == null)
          {
            StartupMap = CurrentMap;
          }
        }
      }
      else
      {
        // No maps; reset the current map
        CurrentMap = null;
      }
    }

    /// <summary>
    /// Returns the candidate map-definitions for the specified universe
    /// </summary>
    private IList<MapDefinition> MapDefinitionCandidatesFor(Universe universe)
    {
      var result = new List<MapDefinition>();
      foreach (var mapDefinition in this.ServiceProviderMapDefinitions)
      {
        if (mapDefinition.Universe == universe)
        {
          result.Add(mapDefinition);
        }
      }
      return result;
    }

    /// <summary>
    /// Returns a new external name for the specified suggested name
    /// </summary>
    private string UniqueExternalNameFor(string suggestedExternalName)
    {
      var externalName = suggestedExternalName;
      var exists = new Func<string, bool>(n => Maps.Where(m => m.ExternalName.ToLower().Equals(n)).Count() > 0);
      int count = 2;
      while (exists(externalName.ToLower())) externalName = externalName = string.Format("{0} ({1})", suggestedExternalName, count++);
      return externalName;
    }
    #endregion

    #region New Map View Model
    /// <summary>
    /// Viewmodel for adding new maps
    /// </summary>
    public LiteNewUserMapViewModel NewMapViewModel
    {
      get;
      set;
    }

    /// <summary>
    /// Adds a new Map to this viewModel, optionally making it current
    /// </summary>
    public void AddNewMap(MapDefinition definition, string mapName)
    {
      if (string.IsNullOrEmpty(mapName) || String.Compare(mapName, ApplicationResources.Map, StringComparison.OrdinalIgnoreCase) == 0)
      {
        mapName = definition.ExternalName;
      }

      // Make unique
      mapName = UniqueExternalNameFor(mapName);

      var newMapViewModel = new LiteMapViewModel(this.Messenger, definition, true, InteractionHandler, this.EpsgCoordinateSystems)
      {
        ExternalName = mapName
      };

      // Make sure the top level layer is switched on
      if (newMapViewModel.Layers.Any())
      {
        newMapViewModel.Layers[0].IsOn = true;
      }

      // Track in Analytics
      LiteAnalyticsTracker.TrackMapCreate(newMapViewModel);

      this.Maps.Add(newMapViewModel);

      this.CurrentMap = newMapViewModel;

      SaveUserMaps();
    }

    /// <summary>
    /// Gets the user maps from the Isolated Storage and adds them to the maps
    /// </summary>
    private async Task AddUserMaps(ObservableCollection<LiteMapViewModel> maps)
    {
      try
      {
        var isolatedStorageMaps = LiteIsolatedStorageManager.Instance.UserMaps;

        if (isolatedStorageMaps != null)
        {
          foreach (var isolatedStorageMap in isolatedStorageMaps)
          {
            var userMap = await isolatedStorageMap.ToUserMapViewModel(this, this.Messenger, this.EpsgCoordinateSystems);
            if (userMap != null)
            {
              maps.Add(userMap);
            }
          }
        }
      }
      catch
      {
        // The stored maps could not be matched with our model
        // We could choose to clear the stored client maps
      }
    }

    /// <summary>
    /// Save the user maps
    /// </summary>
    private void SaveUserMaps()
    {
      var maps = this.Maps;
      if (maps != null)
      {
        var isolatedStorageUserMaps = new List<LiteUserMapStorageModel>();

        foreach (var mapViewModel in maps)
        {
          if (mapViewModel.MapType == LiteMapType.User)
          {
            isolatedStorageUserMaps.Add(new LiteUserMapStorageModel(mapViewModel));
          }
        }

        // And save the lot
        LiteIsolatedStorageManager.Instance.UserMaps = isolatedStorageUserMaps;
      }
    }

    /// <summary>
    /// Callback for changes in a property of the current map
    /// </summary>
    private void MapPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      var currentMap = CurrentMap;
      if (e.PropertyName == LiteMapViewModel.SelectedEpsgCoordinateSystemPropertyName && currentMap != null)
      {
        Messenger.Send(new LiteDisplayCoordinateSystemChangedMessage(this, currentMap.SelectedEpsgCoordinateSystem));
      }
    }

    /// <summary>
    /// Remove the specified user map
    /// </summary>
    private async void RemoveUserMap(LiteMapViewModel map)
    {
      if (Maps != null && map != null)
      {
        var caption = ApplicationResources.MapRemoveTitle;
        var removeString = string.Format(ApplicationResources.MapRemoveStringFormat, map.ExternalName);
        var result = await this.MessageBoxService.ShowAsync(removeString, caption, MessageBoxButton.OKCancel, MessageBoxResult.Cancel);

        if (result == MessageBoxResult.OK)
        {
          Maps.Remove(map);
        }

        SaveUserMaps();
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// The interaction handler to be used by all maps that are set up in this MapsViewModel
    /// </summary>
    public MapInteractionHandler InteractionHandler { get; set; }

    /// <summary>
    /// A flag indicating whether a Single Backdrop Layer should be used (that is visible),
    /// or multiple Backdrop Layers should automatically be added (with only the first
    /// backdrop layer visible)
    /// </summary>
    public bool UseSingleBackdropLayer { get; set; }

    /// <summary>
    /// Holds the minimum zoom level that will be requested for any map. This means
    /// the user can not zoom any more out than the value specified here.
    /// Valid range = [1, 15]. The default is 1.
    /// </summary>
    public int MinZoomLevel
    {
      get { return _minZoomLevel; }
      set
      {
        if (value != _minZoomLevel)
        {
          _minZoomLevel = Math.Max(1, Math.Min(15, value));
        }
      }
    }

    /// <summary>
    /// Holds the maximum zoom level that will be requested for any map. This means
    /// the user can not zoom any deeper than the value specified here.
    /// Valid range = [19, 23], although sensibly the maximum zoom level should be
    /// in the range [21, 23]. The default value is 22.
    /// </summary>
    public int MaxZoomLevel
    {
      get { return _maxZoomLevel; }
      set
      {
        if (value != _maxZoomLevel)
        {
          _maxZoomLevel = Math.Max(19, Math.Min(23, value));
        }
      }
    }

    /// <summary>
    /// An observable collection holding all mapDefinitions that have been provided
    /// by the service providers. Each map definition holds the layer definitions that
    /// the service provider has set up for that map.
    /// </summary>
    public ObservableCollection<MapDefinition> ServiceProviderMapDefinitions
    {
      get { return _serviceProviderMapDefinitions; }
      set
      {
        if (_serviceProviderMapDefinitions != value)
        {
          _serviceProviderMapDefinitions = value;
        }
      }
    }

    /// <summary>
    /// Holds the map definitions as they are going to be used in this mapsViewModel.
    /// Each mapDefinition will/can be set up using layers coming from multiple service
    /// providers, thus allowing a client map(definition) that spans multiple providers.
    /// Each map definition will eventually result in a MapViewModel that is a candidate
    /// for being shown in a MapControl.
    /// </summary>    
    public ObservableCollection<MapDefinition> MapDefinitions
    {
      get { return _mapDefinitions; }
      set
      {
        if (_mapDefinitions != value)
        {
          _mapDefinitions = value;
        }
      }
    }

    /// <summary>
    /// The available map layer areas to be used for restricting layers with
    /// </summary>
    public ObservableCollection<MapLayerArea> RestrictionMapLayerAreas
    {
      get { return LiteClientSettingsViewModel.Instance.RestrictionMapLayerAreas; }
    }

    /// <summary>
    /// Holds a flag indicating whether the restriction areas must be displayed
    /// </summary>
    public bool DisplayRestrictionAreas
    {
      get { return LiteClientSettingsViewModel.Instance.DisplayRestrictionAreas; }
    }

    /// <summary>
    /// The maps, holding the MapViewModel instances that can be shown 
    /// </summary>
    public SortedObservableCollection<LiteMapViewModel> Maps
    {
      get { return _maps; }
      set
      {
        if (value != _maps)
        {
          if (_maps != null)
          {
            DetachMaps();
          }

          var oldMaps = _maps;

          _maps = value;

          if (_maps != null)
          {
            AttachMaps();
          }

          // Notify the outside world
          RaisePropertyChanged(MapsPropertyName, oldMaps, _maps, true);

          // Set the indent of all headers dependent on the availability
          // of user maps
          SetHeadingIndent();

          // Set the current map (defaulting to an OpenOnStartup one)
          SetCurrentMap();
        }
      }
    }

    /// <summary>
    /// The current map (view Model) as should be shown in the MapControl that looks at this mapsViewModel
    /// </summary>
    public LiteMapViewModel CurrentMap
    {
      get { return _currentMap; }
      set
      {
        if (value != null && _currentMap != value)
        {
          var oldMap = _currentMap;

          if (_currentMap != null)
          {
            // Tell the map it is no longer current
            DetachCurrentMap();
          }

          _currentMap = value;

          if (_currentMap != null)
          {
            // Tell the map it is the current one
            AttachCurrentMap();

            // In case there is an old map, try to retain the envelope it used
            // Do this in case both maps are geographic maps
            if (oldMap != null && _currentMap.Universe.HasGeographicCoordinateSystems &&
              oldMap.Universe.HasGeographicCoordinateSystems)
            {
              // Set the envelope of the current map
              _currentMap.SetZoomLevelCentre(oldMap.ZoomLevel, oldMap.Centre);
            }
          }

          // Raise the appropriate propertyChanged events, telling the outside world
          // that the current map has changed
          RaisePropertyChanged(CurrentMapPropertyName, oldMap, _currentMap, true);

          // Notify the outside world of changes in the availability of a Map
          if (oldMap == null ^ _currentMap == null)
          {
            RaisePropertyChanged(HasCurrentMapPropertyName, oldMap != null, _currentMap != null, true);
            RaisePropertyChanged(CurrentMapVisibilityPropertyName);
          }

          if (oldMap != null && _currentMap.World != null && _currentMap.World.IsPartOfMultiWorld)
          {
            // The new map is an internals world
            var oldMapWorld = oldMap.World;
            var oldMapUniverse = oldMapWorld != null ? oldMapWorld.Universe : null;
            var oldWasMainWorld = oldMapUniverse != null && oldMapUniverse.HasGeographicCoordinateSystems && !oldMapUniverse.IsMultiWorld;

            if (oldWasMainWorld)
            {
              PreviousWorldMap = oldMap;
            }
          }
          else
          {
            PreviousWorldMap = null;
          }

          // Send a request to display the Current Map's selection
          SendCurrentMapSelectionDisplayFeatureRequest();

          CheckCommands();

          // Finally - track the fact that a Map is activated in Analytics
          LiteAnalyticsTracker.TrackMapView(CurrentMap);
        }
      }
    }

    /// <summary>
    /// Returns a flag indicating whether there is a current map, which
    /// will always be the case when there actually are maps available
    /// </summary>
    public bool HasCurrentMap
    {
      get { return _currentMap != null; }
    }

    /// <summary>
    /// Returns a flag indicating whether there is a current map, which
    /// will always be the case when there actually are maps available
    /// </summary>
    public Visibility CurrentMapVisibility
    {
      get { return HasCurrentMap ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Holds the (default world) map that we started with
    /// </summary>
    private LiteMapViewModel StartupMap
    {
      get;
      set;
    }

    /// <summary>
    /// The Previous World Map, holding a Geographic world that is non multi.
    /// Is only set in case the CurrentMap actually is an internals world; in that
    /// case the previous world map can be used to jump out of the internal world.
    /// </summary>
    public LiteMapViewModel PreviousWorldMap
    {
      get { return _previousWorldMap; }
      set
      {
        if (_previousWorldMap != value)
        {
          _previousWorldMap = value;

          CheckCommands();

          // Raise the appropriate propertyChanged events, telling the outside world
          // that there now is a previous world map
          RaisePropertyChanged(PreviousWorldMapPropertyName);

          // Notify the outside world of changes in the availability of a Map
          RaisePropertyChanged(HasPreviousWorldMapPropertyName);

          // Notify the outside world of changes in the availability of a Map
          RaisePropertyChanged(PreviousWorldMapVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Do we have a previous world map
    /// </summary>
    public bool HasPreviousWorldMap
    {
      get { return _previousWorldMap != null; }
    }

    /// <summary>
    /// Visibility that can be used for previous worldMap access
    /// </summary>
    public Visibility PreviousWorldMapVisibility
    {
      get { return IsInDesignMode ? Visibility.Visible : HasPreviousWorldMap ? Visibility.Visible : Visibility.Collapsed; }

    }

    /// <summary>
    /// The named Spatial Reference IDs that are available, which are key-value
    /// pairs of an SRID (integer) and a descriptive Name (string)
    /// </summary>
    public EpsgCoordinateSystemReferenceCollection EpsgCoordinateSystems
    {
      get { return _epsgCoordinateSystems; }
      set
      {
        if (_epsgCoordinateSystems != value)
        {
          _epsgCoordinateSystems = value;
          RaisePropertyChanged(EpsgCoordinateSystemsPropertyName);
        }
      }
    }

    /// <summary>
    /// Holds a flag indicating whether this view model is busy
    /// </summary>
    public bool IsBusy
    {
      get { return _isBusy; }
      private set
      {
        if (_isBusy != value)
        {
          _isBusy = value;
          RaisePropertyChanged(IsBusyPropertyName);
        }
      }
    }

    /// <summary>
    /// Is there a user map
    /// </summary>
    public Boolean HasUserMap
    {
      get { return Maps != null && Maps.FirstOrDefault((m) => m.MapType == LiteMapType.User) != null; }
    }

    /// <summary>
    /// Sets the heading indent
    /// </summary>
    public void SetHeadingIndent()
    {
      var maps = this.Maps;
      if (maps != null)
      {
        var hasUserMaps = HasUserMap;
        foreach (var map in maps)
        {
          // Set the heading based on the existence of user maps
          map.HeadingIndent = hasUserMaps ? (map.IsUserMap ? 8 : 24) : 12;
        }
      }
    }
    #endregion
  }
}
