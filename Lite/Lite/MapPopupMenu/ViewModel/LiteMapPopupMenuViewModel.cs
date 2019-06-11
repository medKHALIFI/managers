using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.ServiceLocation;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Geometry.Services;
using SpatialEye.Framework.Redlining;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// THe map popup menu view model
  /// </summary>
  public class LiteMapPopupMenuViewModel : ViewModelBase
  {
    #region Event Tracker
    /// <summary>
    /// The event tracker
    /// </summary>
    internal class MapPopupMenuEventTracker : MapEventTracker
    {
      /// <summary>
      /// Constructor for the event tracker
      /// </summary>
      internal MapPopupMenuEventTracker(LiteMapPopupMenuViewModel owner)
      {
        Owner = owner;
      }

      /// <summary>
      /// The owner
      /// </summary>
      private LiteMapPopupMenuViewModel Owner { get; set; }

      /// <summary>
      /// Is this element active
      /// </summary>
      internal bool IsActive { get; set; }

      /// <summary>
      /// On mouse move handler
      /// </summary>
      protected override void OnMouseMove(MapViewModel sender, MapMouseEventArgs args)
      {
        if (IsActive)
        {
          Owner.ClosePopupMenu();
        }
      }

      /// <summary>
      /// On mouse rmc handler
      /// </summary>
      protected override void OnMouseRightButtonDown(MapViewModel sender, MapMouseEventArgs args)
      {
        Owner.OpenPopupMenu(args);
      }
    }
    #endregion

    #region Property Names

    /// <summary>
    /// The popup menu visibility
    /// </summary>
    public const string PopupMenuVisibilityPropertyName = "PopupMenuVisibility";

    /// <summary>
    /// The popup x indent
    /// </summary>
    public const string PopupMenuIndentXPropertyName = "PopupMenuIndentX";

    /// <summary>
    /// The popup y indent
    /// </summary>
    public const string PopupMenuIndentYPropertyName = "PopupMenuIndentY";

    #endregion

    #region Fields
    /// <summary>
    /// The active event tracker
    /// </summary>
    private MapPopupMenuEventTracker _activeEventTracker;

    /// <summary>
    /// The active map view model
    /// </summary>
    private LiteMapViewModel _currentMapViewModel;

    /// <summary>
    /// The popup menu visibility
    /// </summary>
    private Visibility _popupMenuVisibility = Visibility.Collapsed;

    /// <summary>
    /// The indentation of the popup
    /// </summary>
    private GridLength _popupMenuIndentX;

    /// <summary>
    /// The indentation of the popup
    /// </summary>
    private GridLength _popupMenuIndentY;

    /// <summary>
    /// Menu items holder
    /// </summary>
    private ObservableCollection<LiteMenuCategoryViewModel> _menuItems;

    /// <summary>
    /// Is editability enabled (authorized)
    /// </summary>
    private Boolean _editabilityEnabled;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the popup menu view model
    /// </summary>
    public LiteMapPopupMenuViewModel(Messenger messenger = null)
      : base(messenger)
    {
      AttachToMessenger();
      SetupMenuCommands();
      Resources = new Lite.Resources.Localization.ApplicationResources();
    }

    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, (m) => this.MapViewModel = m.NewValue);
    }
    #endregion

    #region Menu & Commands

    /// <summary>
    /// Set up the menu commands
    /// </summary>
    private void SetupMenuCommands()
    {
      // Setup the menu items
      var items = new ObservableCollection<LiteMenuCategoryViewModel>();

      // Setup the clipboard actions
      {
        var cat = new LiteMenuCategoryViewModel { Title = "Clipboard", BorderVisibility = Visibility.Collapsed };
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupCopyCoordinateToClipboard, "MetroIcon.Content.Clipboard")
        {
          Command = new RelayCommand(CopyMouseCoordinateToClipboard)
        });

        items.Add(cat);
      }

      // Setup the selection actions
      {
        var cat = new LiteMenuCategoryViewModel { Title = "Selection", BorderVisibility = Visibility.Visible };
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupGotoSelection, "MetroIcon.Content.GotoSelection")
        {
          Command = new RelayCommand(GoToSelection, HasSelection)
        });
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupClearSelection, "MetroIcon.Content.ClearSelection")
        {
          Command = new RelayCommand(ClearSelection, HasSelection)
        });
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupEditGeometrySelection, "MetroIcon.Content.EditSelection")
        {
          Command = new RelayCommand(EditSelectedGeometry),
          VisibilityPredicate = HasEditableGeometrySelection
        });

        items.Add(cat);
      }

      // Setup the trail actions
      {
        var cat = new LiteMenuCategoryViewModel { Title = "Trail", BorderVisibility = Visibility.Visible };
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupCreateTrailFromSelection, "MetroIcon.Content.Trail")
        {
          Command = new RelayCommand(CreateTrailFromSelection, HasSelectionForTrailCreation)
        });
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupRemoveTrail, "MetroIcon.Content.TrailClear")
        {
          Command = new RelayCommand(RemoveSelectedTrail, HasTrailSelection)
        });

        items.Add(cat);
      }

      // Setup the object properties actions
      {
        var cat = new LiteMenuCategoryViewModel { Title = "Properties", BorderVisibility = Visibility.Visible };
        cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapPopupObjectProperties, "MetroIcon.Content.Details")
        {
          Command = new RelayCommand(DisplayObjectProperties, HasSelection)
        });

        items.Add(cat);
      }

      // Set the new menu
      MenuItems = items;
    }

    /// <summary>
    /// Check the commands
    /// </summary>
    private void CheckMenuItemStates()
    {
      // Have the menu items check their states
      foreach (var item in MenuItems)
      {
        item.CheckStates();
      }
    }

    /// <summary>
    /// Copy the mouse coordinate to clipboard
    /// </summary>
    private void CopyMouseCoordinateToClipboard()
    {
      // Close the popup menu
      ClosePopupMenu();

      // Close the popup menu
      Clipboard.SetText(this.MouseClipboardText);
    }

    /// <summary>
    /// Go To selection
    /// </summary>
    private void GoToSelection()
    {
      var mapView = MapViewModel;

      ClosePopupMenu();

      if (mapView != null && mapView.SelectedFeatures.Count > 0)
      {
        // There is a feature
        var feature = mapView.SelectedFeatures[0];

        // Pick up the feature's envelope
        var envelope = feature.GetEnvelope();
        if (envelope != null)
        {
          // Request a go-to envelope and put it on the messenger
          Messenger.Send(new LiteGoToGeometryRequestMessage(mapView, envelope, feature));
        }
      }
    }

    /// <summary>
    /// Clear the selection
    /// </summary>
    private void ClearSelection()
    {
      var mapView = MapViewModel;

      ClosePopupMenu();

      if (mapView != null)
      {
        mapView.ClearSelection();
      }
    }

    /// <summary>
    /// Edit the selected geometry
    /// </summary>
    private void EditSelectedGeometry()
    {
      var mapView = MapViewModel;

      ClosePopupMenu();

      if (mapView != null && mapView.SelectedFeatureGeometry.Count > 0)
      {
        // There is a feature
        var featureGeometry = mapView.SelectedFeatureGeometry[0];

        // Request to display the feature
        Messenger.Send(new LiteDisplayFeatureDetailsRequestMessage(this, featureGeometry.Feature, featureGeometry.TargetGeometryFieldDescriptor, makeViewActive: true, startEditing: true));
      }
    }

    /// <summary>
    /// Display the object properties for the selected element
    /// </summary>
    private void DisplayObjectProperties()
    {
      var mapView = MapViewModel;

      ClosePopupMenu();

      if (mapView != null && mapView.SelectedFeatureGeometry.Count > 0)
      {
        // There is a feature
        var featureGeometry = mapView.SelectedFeatureGeometry[0];

        // Request to display the feature
        Messenger.Send(new LiteDisplayFeatureDetailsRequestMessage(this, featureGeometry.Feature, featureGeometry.TargetGeometryFieldDescriptor, makeViewActive: true));
      }
    }

    /// <summary>
    /// Create a trail from the selection
    /// </summary>
    private void CreateTrailFromSelection()
    {
      var mapView = MapViewModel;

      ClosePopupMenu();

      if (mapView != null)
      {
        var trailLayer = mapView.TrailLayer;
        var selectedElement = mapView.SelectedFeatureGeometry.Count > 0 ? mapView.SelectedFeatureGeometry[0] : null;

        if (selectedElement != null && selectedElement.Feature.TableDescriptor.Name != SpatialEye.Framework.Client.MapViewModel.TrailTableDescriptorName)
        {
          var redliningElement = RedliningElement.ElementFor(selectedElement.TargetGeometry);
          if (redliningElement != null)
          {
            trailLayer.Drawing.Add(redliningElement);
            trailLayer.DrawingSelection.Set(redliningElement);
          }
        }
      }
    }

    /// <summary>
    /// Clear the trail
    /// </summary>
    private void RemoveSelectedTrail()
    {
      var mapView = MapViewModel;

      ClosePopupMenu();

      if (mapView != null)
      {
        mapView.TrailLayer.Drawing.Remove(mapView.TrailLayer.DrawingSelection, true);
        mapView.TrailLayer.DrawingSelection.Clear();

        ClearSelection();
      }
    }

    /// <summary>
    /// Do we have a selection
    /// </summary>
    private bool HasSelection()
    {
      return MapViewModel != null && MapViewModel.SelectedFeatureGeometry != null && MapViewModel.SelectedFeatureGeometry.Count > 0;
    }

    /// <summary>
    /// Returns a flag indicating whether there is editable geometry selected
    /// </summary>
    private bool HasEditableGeometrySelection()
    {
      var hasEditableGeometrySelection = false;

      if (MapViewModel != null && EditabilityEnabled)
      {
        var selection = MapViewModel.SelectedFeatureGeometry;

        // Check whether the selected geometry field is editable
        hasEditableGeometrySelection = selection != null && selection.Count == 1 && selection[0].TargetGeometryFieldDescriptor.EditabilityProperties.AllowUpdate;
      }

      return hasEditableGeometrySelection;
    }

    /// <summary>
    /// Do we have a selection
    /// </summary>
    private bool HasSelectionForTrailCreation()
    {
      var hasSelection = false;
      if (MapViewModel != null && MapViewModel.SelectedFeatureGeometry != null && MapViewModel.SelectedFeatureGeometry.Count > 0)
      {
        var feature = MapViewModel.SelectedFeatureGeometry[0].Feature;
        if (feature != null && feature.TableDescriptor.Name != SpatialEye.Framework.Client.MapViewModel.TrailTableDescriptorName)
        {
          hasSelection = true;
        }
      }

      return hasSelection;
    }

    /// <summary>
    /// Do we have a trail
    /// </summary>
    /// <returns></returns>
    private bool HasTrailSelection()
    {
      var hasSelection = false;
      if (MapViewModel != null && MapViewModel.SelectedFeatureGeometry != null && MapViewModel.SelectedFeatureGeometry.Count > 0)
      {
        var feature = MapViewModel.SelectedFeatureGeometry[0].Feature;
        if (feature != null && feature.TableDescriptor.Name == SpatialEye.Framework.Client.MapViewModel.TrailTableDescriptorName)
        {
          hasSelection = true;
        }
      }
      return hasSelection;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Callback when the culture changes
    /// </summary>
    /// <param name="currentCultureInfo">the current culture info</param>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      SetupMenuCommands();
    }

    /// <summary>
    /// Authentication changed callback
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Set the authorized options
        EditabilityEnabled = LiteClientSettingsViewModel.Instance.AllowGeoNoteEdits;
      }
    }
    #endregion

    #region Event Tracker
    /// <summary>
    /// Attach an event tracker to the current map view model
    /// </summary>
    private void AttachMap()
    {
      var mapViewModel = MapViewModel;
      if (mapViewModel != null)
      {
        _activeEventTracker = new MapPopupMenuEventTracker(this);
        mapViewModel.InteractionHandler.EventTrackers.Add(_activeEventTracker);

        mapViewModel.PropertyChanged += MapViewModelPropertyChanged;
        mapViewModel.TrailLayer.Drawing.ContentsChanged += TrailLayerContentChanged;
      }
    }

    /// <summary>
    /// Detach the active map event tracker
    /// </summary>
    private void DetachMap()
    {
      var tracker = _activeEventTracker;
      var mapViewModel = MapViewModel;
      _activeEventTracker = null;
      if (tracker != null && mapViewModel != null)
      {
        mapViewModel.InteractionHandler.EventTrackers.Remove(tracker);

        mapViewModel.PropertyChanged -= MapViewModelPropertyChanged;
        mapViewModel.TrailLayer.Drawing.ContentsChanged -= TrailLayerContentChanged;
      }
    }

    /// <summary>
    /// Check the commands whenever appropriate properties change
    /// </summary>
    void MapViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == SpatialEye.Framework.Client.MapViewModel.SelectedFeatureGeometryPropertyName)
      {
        CheckMenuItemStates();
      }
    }

    /// <summary>
    /// The trail content has changed
    /// </summary>
    /// <param name="model"></param>
    /// <param name="args"></param>
    void TrailLayerContentChanged(GeometryModel<RedliningElement> model, GeometryModel<RedliningElement>.ContentsChangedEventArgs args)
    {
      CheckMenuItemStates();
    }
    #endregion

    #region Properties
    /// <summary>
    /// The current map view model
    /// </summary>
    public LiteMapViewModel MapViewModel
    {
      get { return _currentMapViewModel; }
      private set
      {
        if (_currentMapViewModel != value)
        {
          DetachMap();
          _currentMapViewModel = value;
          AttachMap();
        }
      }
    }

    /// <summary>
    /// Is editability enabled (authorized)
    /// </summary>
    public Boolean EditabilityEnabled
    {
      get { return _editabilityEnabled; }
      set
      {
        if (value != _editabilityEnabled)
        {
          _editabilityEnabled = value;
          RaisePropertyChanged();
          CheckMenuItemStates();
        }
      }
    }

    /// <summary>
    /// The clipboard text
    /// </summary>
    private string MouseClipboardText
    {
      get;
      set;
    }

    /// <summary>
    /// The visibility of the popup menu
    /// </summary>
    public Visibility PopupMenuVisibility
    {
      get { return _popupMenuVisibility; }
      set
      {
        if (_popupMenuVisibility != value)
        {
          _popupMenuVisibility = value;
          RaisePropertyChanged(PopupMenuVisibilityPropertyName);

          if (_activeEventTracker != null)
          {
            _activeEventTracker.IsActive = _popupMenuVisibility == Visibility.Visible;
          }
        }
      }
    }

    /// <summary>
    /// The X ident of the popup
    /// </summary>
    public GridLength PopupMenuIndentX
    {
      get { return _popupMenuIndentX; }
      set
      {
        if (_popupMenuIndentX != value)
        {
          _popupMenuIndentX = value;
          RaisePropertyChanged(PopupMenuIndentXPropertyName);
        }
      }
    }

    /// <summary>
    /// The Y ident of the popup
    /// </summary>
    public GridLength PopupMenuIndentY
    {
      get { return _popupMenuIndentY; }
      set
      {
        if (_popupMenuIndentY != value)
        {
          _popupMenuIndentY = value;
          RaisePropertyChanged(PopupMenuIndentYPropertyName);
        }
      }
    }

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
    #endregion

    #region Popup Menu handling
    /// <summary>
    /// Indicates a Right Mouse click at the specified position
    /// </summary>
    public async Task SetupMouseDescription(MapMouseEventArgs args)
    {
      // Set up coordinates in pixel and world space
      // Transform to WGS84
      var geomService = ServiceLocator.Current.GetInstance<IGeometryService>();

      // Get the epsg-code to transform the selected coordinate to
      var clipboardCS = MapViewModel.SelectedEpsgCoordinateSystem ?? LiteMapViewModel.DefaultEpsgCoordinateSystem;

      // Transform the coordinate to the selected display cs
      var clipboardCoordinate = await geomService.TransformAsync(args.Location.Coordinate, 3785, clipboardCS.SRId);

      var mouseClipboardText = string.Empty;

      var clipboardX = clipboardCoordinate.X.ToString(NumberFormatInfo.InvariantInfo);
      var clipboardY = clipboardCoordinate.Y.ToString(NumberFormatInfo.InvariantInfo);

      if (clipboardCS.SRId == 4326)
      {
        // Do something special for WGS84
        mouseClipboardText = string.Format("{0}   ({1}, {2})", CoordinateSystem.CoordinateToDmsString(clipboardCoordinate), clipboardY, clipboardX);
      }
      else
      {
        // Default clipboard text
        mouseClipboardText = string.Format("{0}, {1}", clipboardX, clipboardY);
      }

      // Set the clipboard text
      MouseClipboardText = mouseClipboardText;
    }

    /// <summary>
    /// Open the popup menu
    /// </summary>
    internal async void OpenPopupMenu(MapMouseEventArgs args)
    {
      // Setup the mouse description
      await SetupMouseDescription(args);

      PopupMenuIndentX = new GridLength(args.X - 10);
      PopupMenuIndentY = new GridLength(args.Y - 10);
      PopupMenuVisibility = Visibility.Visible;
    }

    /// <summary>
    /// Close the popup menu
    /// </summary>
    internal void ClosePopupMenu()
    {
      PopupMenuVisibility = Visibility.Collapsed;
    }
    #endregion

  }
}
