using System;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// The select hover view model that manages a select-hover event tracker
  /// that takes care of selection by the user.
  /// </summary>
  public class LiteMapSelectHoverViewModel : ViewModelBase
  {
    #region Comparer
    /// <summary>
    /// Holds the lambda-based comparer for sorting featureTargetGeometry
    /// </summary>
    private static Func<FeatureTargetGeometry, FeatureTargetGeometry, int> FeatureTargetDescriptionComparer = new Func<FeatureTargetGeometry, FeatureTargetGeometry, int>((a, b) => a.Description.CompareTo(b.Description));
    #endregion

    #region Static Property Names
    /// <summary>
    /// The view visibility
    /// </summary>
    public static string ViewVisibilityPropertyName = "ViewVisibility";

    /// <summary>
    /// The elements property name
    /// </summary>
    public static string ElementsPropertyName = "Elements";

    /// <summary>
    /// The selected element
    /// </summary>
    public static string SelectedElementPropertyName = "SelectedElement";
    #endregion

    #region Fields
    /// <summary>
    /// The elements to be displayed in the select/hover view model
    /// </summary>
    private SortedObservableCollection<FeatureTargetGeometry> _elements;

    /// <summary>
    /// The element that is selected
    /// </summary>
    private FeatureTargetGeometry _selectedElement;

    /// <summary>
    /// The current map
    /// </summary>
    private LiteMapViewModel _currentMap;

    /// <summary>
    /// The view visibility
    /// </summary>
    private Visibility _viewVisibility = Visibility.Collapsed;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public LiteMapSelectHoverViewModel(Messenger messenger = null)
      : base(messenger)
    {
      if (!IsInDesignMode)
      {
        AttachToMessenger();
        SetupCommands();
        SetupTrackers();
      }
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, m => CurrentMap = m.NewValue);
      }
    }
    #endregion

    #region Commands
    /// <summary>
    /// The close command, used to close the view
    /// </summary>
    public RelayCommand CloseCommand
    {
      get;
      set;
    }

    /// <summary>
    /// Sets up the commands
    /// </summary>
    private void SetupCommands()
    {
      CloseCommand = new RelayCommand(() => ViewVisibility = Visibility.Collapsed);
    }
    #endregion

    #region Trackers
    /// <summary>
    /// Holds the select/hover event-tracker, that is used to track events of the current map
    /// </summary>
    private MapSelectHoverEventTracker SelectHoverEventTracker
    {
      get;
      set;
    }

    /// <summary>
    /// Sets up the select/hover event tracker that is used to track specified events
    /// from the current map (its interaction mode)
    /// </summary>
    private void SetupTrackers()
    {
      SelectHoverEventTracker = new MapSelectHoverEventTracker() { FeatureGeometryAction = HandleSelectHoverFeatureGeometry, MaximumFeatures = 25 };
    }

    /// <summary>
    /// Handles the select/hover event that resulted in the specified featureGeometry that was
    /// found upon select-hovering.
    /// </summary>
    /// <param name="featureGeometry">The featureGeometry at the current position</param>
    private void HandleSelectHoverFeatureGeometry(ObservableCollection<FeatureTargetGeometry> featureGeometry)
    {
      this.Elements = new SortedObservableCollection<FeatureTargetGeometry>(FeatureTargetDescriptionComparer, featureGeometry);
    }

    /// <summary>
    /// Select an element on the map
    /// </summary>
    private async void SelectElement(FeatureTargetGeometry element)
    {
      var map = CurrentMap;
      if (map != null)
      {
        // Allow the UI to finish its current action
        await TaskFunctions.Yield();

        // Set the map selection directly (we could have done this via the messenger as well)
        // An example of a tighter coupling, using the API of the MapViewModel directly
        map.SelectedFeatureGeometry.Set(element);
      }
    }
    #endregion

    #region API
    /// <summary>
    /// Holds the current map
    /// </summary>
    private LiteMapViewModel CurrentMap
    {
      get { return _currentMap; }
      set
      {
        if (_currentMap != null)
        {
          // Remove our event-tracker from the active map
          _currentMap.InteractionHandler.EventTrackers.Remove(SelectHoverEventTracker);
        }

        _currentMap = value;

        if (_currentMap != null)
        {
          // Add our event-tracker to the new active map
          _currentMap.InteractionHandler.EventTrackers.Add(SelectHoverEventTracker);
        }
      }
    }

    /// <summary>
    /// The visibility of the view
    /// </summary>
    public Visibility ViewVisibility
    {
      get { return _viewVisibility; }
      set
      {
        if (_viewVisibility != value)
        {
          _viewVisibility = value;
          RaisePropertyChanged(ViewVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Holds the feature target geometry collection that was found by the
    /// select-hover event-tracker
    /// </summary>
    public SortedObservableCollection<FeatureTargetGeometry> Elements
    {
      get { return _elements; }
      set
      {
        if (_elements != value)
        {
          _elements = value;
          RaisePropertyChanged(ElementsPropertyName);

          if (_elements != null && _elements.Count > 0)
          {
            this.ViewVisibility = Visibility.Visible;
          }
          else
          {
            this.ViewVisibility = Visibility.Collapsed;
          }

          SelectedElement = null;
        }
      }
    }

    /// <summary>
    /// The selected feature-target geometry
    /// </summary>
    public FeatureTargetGeometry SelectedElement
    {
      get { return _selectedElement; }
      set
      {
        if (_selectedElement != value)
        {
          _selectedElement = value;

          RaisePropertyChanged(SelectedElementPropertyName);

          if (_selectedElement != null)
          {
            ViewVisibility = Visibility.Collapsed;
            SelectElement(_selectedElement);
          }
        }
      }
    }
    #endregion
  }
}
