using System.Windows;
using SpatialEye.Framework.Client;
using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The place finder view model, holding the geoLocator view model as well as the 
  /// recently visited (search result) items view model
  /// </summary>
  public class LiteMapPlaceFinderViewModel : ViewModelBase
  {
    #region Constructor
    /// <summary>
    /// Constructs the place finder view model for the specified messenger
    /// </summary>
    public LiteMapPlaceFinderViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Instead of sub-classing the toolkit's GeoLocatorViewModel and working with On<property>Changed overrides,
      // let's create one directly and use event handlers for dealing with changes in the result
      GeoLocatorViewModel = new GeoLocatorViewModel(messenger)
      {
        DoReplaceSearchStringWithActiveResult = false,
        DoShowPopupWithSingleCandidate = true
      };

      // Set up the event handler
      GeoLocatorViewModel.ResultActivated += GeoLocatorViewModel_ResultActivated;

      // Set up the recently visited items view model for being able to navigate back
      // to previously visited elements
      RecentItemsViewModel = new LiteMapRecentlyVisitedItemViewModel(messenger);

      // Property change handling
      GeoLocatorViewModel.PropertyChanged += GeoLocatorViewModel_PropertyChanged;
      RecentItemsViewModel.PropertyChanged += RecentItemsViewModel_PropertyChanged;

      // Typing delay before sending request to the geoLocator Viewmodel
      TypingDelayBeforeSendingRequest = 750;

      // Set up the resources
      this.Resources = new ApplicationResources();

      // Register to the Messenger
      RegisterToMessenger();
    }
    #endregion

    #region Messenger Registration
    /// <summary>
    /// Registers the Place Finder to the messenger, acting upon current map changes,
    /// notifying the GeoLocator to use the active map for retrieving hint bounds from
    /// </summary>
    private void RegisterToMessenger()
    {
      if (IsInDesignMode)
      {
        return;
      }

      // Upon a new map, tell the geoLocator of this map. 
      // The geoLocator is then capable of retrieving the map's bounds for use as hint
      Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, m => GeoLocatorViewModel.MapView = m.NewValue);
    }
    #endregion

    #region Recent Items
    /// <summary>
    /// The recent items view model's property has changed
    /// </summary>
    void RecentItemsViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == LiteMapRecentlyVisitedItemViewModel.RecentlyVisitedItemsIsVisiblePropertyName &&
        RecentItemsViewModel.RecentlyVisitedItemsIsVisible)
      {
        GeoLocatorViewModel.ViewResultsVisibility = Visibility.Collapsed;
      }
    }

    /// <summary>
    /// The recent items view model
    /// </summary>
    public LiteMapRecentlyVisitedItemViewModel RecentItemsViewModel
    {
      get;
      private set;
    }

    /// <summary>
    /// Holds the postfix to use for searching
    /// </summary>
    public string SearchPostfix
    {
      get { return GeoLocatorViewModel.SearchPostfix; }
      set { GeoLocatorViewModel.SearchPostfix = value; }
    }
    #endregion

    #region GeoLocator
    /// <summary>
    /// A geoLocator View Model's property has changed
    /// </summary>
    void GeoLocatorViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == GeoLocatorViewModel.ViewResultsVisibilityPropertyName &&
        GeoLocatorViewModel.ViewResultsVisibility == Visibility.Visible)
      {
        RecentItemsViewModel.RecentlyVisitedItemsIsVisible = false;
      }
    }
    /// <summary>
    /// A result has been selected in the GeoLocator 
    /// </summary>
    void GeoLocatorViewModel_ResultActivated(GeoLocatorViewModel.GeoLocatorResultEventArgs resultArgs)
    {
      // A result has been found from the GeoLocator; we want to jump to it on the active map
      var address = resultArgs.Address;

      if (address != null && address.Envelope != null && !double.IsNaN(address.Envelope.CentreLeft.X))
      {
        var request = new LiteGoToGeometryRequestMessage(resultArgs.Source, address.Envelope, address.Description)
        {
          DoHighlight = true,
          StoreInHistory = true
        };

        this.Messenger.Send(request);
      }
    }

    /// <summary>
    /// The geoLocator view model
    /// </summary>
    public GeoLocatorViewModel GeoLocatorViewModel
    {
      get;
      private set;
    }

    /// <summary>
    /// The delay (in ms) that will be waited before sending a request to the 
    /// geolocator (service). If before that time the user continues typing,
    /// the delay will start again. 
    /// </summary>
    public int TypingDelayBeforeSendingRequest
    {
      get;
      set;
    }

    /// <summary>
    /// The maximum number of result elements
    /// </summary>
    public int MaximumNumberOfResults
    {
      get { return GeoLocatorViewModel.MaximumNumberOfResults; }
      set { GeoLocatorViewModel.MaximumNumberOfResults = value; }
    }
    #endregion
  }
}
