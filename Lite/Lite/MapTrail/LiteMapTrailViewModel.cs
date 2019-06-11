using System.Collections.ObjectModel;
using System.Collections.Generic;

using SpatialEye.Framework.Redlining;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The mapTrail view model holds the access to the commands to add trail elements and interact with them.
  /// </summary>
  public class LiteMapTrailViewModel : ViewModelBase
  {
    #region Static Trail Helpers
    
    /// <summary>
    /// Returns a flag indicating whether this is a trail feature; 
    /// we want to recognize those, since we are skipping them in the Details
    /// </summary>
    public static bool IsTrailFeature(IList<Feature> features)
    {
      var selectedFeature = features != null && features.Count == 1 ? features[0] : null;
      return IsTrailFeature(selectedFeature);
    }

    /// <summary>
    /// Returns a flag indicating whether this is a trail feature; 
    /// we want to recognize those, since we are skipping them in the Details
    /// </summary>
    public static bool IsTrailFeature(Feature feature)
    {
      var selectedTable = feature != null ? feature.TableDescriptor : null;
      return selectedTable != null && string.Equals(selectedTable.Name, MapViewModel.TrailTableDescriptorName);
    }

    #endregion

    #region Property Names

    /// <summary>
    /// Is the submenu of the trail active
    /// </summary>
    public static string SubMenuIsActivePropertyName = "SubMenuIsActive";

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

    /// <summary>
    /// Trail items
    /// </summary>
    private ObservableCollection<LiteMenuCategoryViewModel> _menuItems;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the MapTrail ViewModel and attaches it to the specified messenger
    /// </summary>
    /// <param name="messenger"></param>
    public LiteMapTrailViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Attach to the messenger; reacting to view model changes
      AttachToMessenger();

      // Set up the commands to craete/interact with the trail
      SetupMenuCommands();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      // Set up a handler for active mapView changes
      this.Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, change => MapView = change.NewValue);
      this.Messenger.Register<LiteClearTrailRequestMessage>(this, HandleClearTrailRequest);
    }

    /// <summary>
    /// The handler for requests to clear the trail
    /// </summary>
    private void HandleClearTrailRequest(LiteClearTrailRequestMessage request)
    {
      var mapView = MapView;
      if (mapView != null && mapView.TrailLayer != null)
      {
        if (request.ClearSelectedOnly)
        {
          // Get the selection
          var selection = mapView.TrailLayer.DrawingSelection;

          // And remove from the model (which will automatically remove it from the selection)
          mapView.TrailLayer.Drawing.Remove(selection);
        }
        else
        {
          // Clear the layer in its entirity
          mapView.TrailLayer.Drawing.Clear();
        }
      }
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

      var cat = new LiteMenuCategoryViewModel { Title = ApplicationResources.MapTrailCreateCategoryHeader };
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.TrailPoint, "MetroIcon.Content.TrailPoint")
      {
        Command = new RelayCommand(() => NewTrail(new RedliningPoint(CoordinateSystem)), CanAddNewElement),
      });
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.TrailCurve, "MetroIcon.Content.TrailCurve")
      {
        Command = new RelayCommand(() => NewTrail(new RedliningCurve(CoordinateSystem)), CanAddNewElement),
      });
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.TrailPolygon, "MetroIcon.Content.TrailPolygon")
      {
        Command = new RelayCommand(() => NewTrail(new RedliningPolygon(CoordinateSystem)), CanAddNewElement),
      });
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.TrailCircle, "MetroIcon.Content.TrailCircle")
      {
        Command = new RelayCommand(() => NewTrail(new RedliningCircle(CoordinateSystem)), CanAddNewElement),
      });
      cat.Items.Add(new LiteMenuItemViewModel(ApplicationResources.TrailRectangle, "MetroIcon.Content.TrailRectangle")
      {
        Command = new RelayCommand(() => NewTrail(new RedliningRectangle(CoordinateSystem)), CanAddNewElement),
      });
      items.Add(cat);

      var catActions = new LiteMenuCategoryViewModel { Title = ApplicationResources.MapTrailActionsCategoryHeader };
      catActions.Items.Add(new LiteMenuItemViewModel(ApplicationResources.MapTrailClearAll, "MetroIcon.Content.TrailClear")
      {
        Command = new RelayCommand(ClearElements, HasElements),
      });

      items.Add(catActions);

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

    /// <summary>
    /// Can we add a new element
    /// </summary>
    private bool CanAddNewElement()
    {
      return MapView != null && MapView.TrailLayer != null && MapView.CoordinateSystem != null;
    }

    /// <summary>
    /// Are there any trail elements
    /// </summary>
    private bool HasElements()
    {
      return MapView != null && MapView.TrailLayer != null && MapView.TrailLayer.Drawing.Count > 0;
    }

    /// <summary>
    /// Removes all the trail elements
    /// </summary>
    private void ClearElements()
    {
      // Switch off the submenu
      SubMenuIsActive = false;

      if (HasElements())
      {
        // Clear the selection
        MapView.TrailLayer.ClearSelection();

        // Remove all drawing elements
        MapView.TrailLayer.Drawing.Clear();
      }
    }

    #endregion

    #region Callbacks

    /// <summary>
    /// Callback when the culture changes
    /// </summary>
    /// <param name="currentCultureInfo"></param>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      SetupMenuCommands();
    }

    /// <summary>
    /// Callback when the trail drawing layer changes
    /// </summary>
    private void Drawing_ContentsChanged(GeometryModel<RedliningElement> model, GeometryModel<RedliningElement>.ContentsChangedEventArgs args)
    {
      CheckMenuItemStates();

      // Send the trail model change on the databus
      Messenger.Send(new LiteMapTrailModelChangedMessage(this, model, args));
    }
    #endregion

    #region Trail Creation Helpers
    /// <summary>
    /// Create a new element for the Trail Layer
    /// </summary>
    /// <param name="element">The element to add</param>
    private void NewTrail(RedliningElement element)
    {
      // Switch off the submenu
      SubMenuIsActive = false;

      MapView.TrailLayer.NewElement(element);
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
    /// Internal property to help trail construction be less verbose
    /// </summary>
    private CoordinateSystem CoordinateSystem
    {
      get { return MapView != null ? MapView.CoordinateSystem : null; }
    }

    /// <summary>
    /// Command to create a curve trail
    /// </summary>
    public RelayCommand CreateTrailPointCommand { get; private set; }

    /// <summary>
    /// Command to create a curve trail
    /// </summary>
    public RelayCommand CreateTrailCurveCommand { get; private set; }

    /// <summary>
    /// Command to create a polygon trail
    /// </summary>
    public RelayCommand CreateTrailPolygonCommand { get; private set; }

    /// <summary>
    /// Command to create a circle trail
    /// </summary>
    public RelayCommand CreateTrailCircleCommand { get; private set; }

    /// <summary>
    /// Command to create a rectangle trail
    /// </summary>
    public RelayCommand CreateTrailRectangleCommand { get; private set; }

    /// <summary>
    /// The map view we are currently creating trails for
    /// </summary>
    public MapViewModel MapView
    {
      get { return _mapView; }
      set
      {
        if (_mapView != value)
        {

          if (_mapView != null && _mapView.TrailLayer != null)
          {
            _mapView.TrailLayer.Drawing.ContentsChanged -= Drawing_ContentsChanged;
          }

          _mapView = value;

          if (_mapView != null && _mapView.TrailLayer != null)
          {
            _mapView.TrailLayer.Drawing.ContentsChanged += Drawing_ContentsChanged;
          }

          CheckMenuItemStates();
        }
      }
    }
    #endregion

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
    #endregion
  }
}
