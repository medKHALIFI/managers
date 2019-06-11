using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

using SpatialEye.Framework.Client;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// A recently visited item view model, holding information on recent search results
  /// </summary>
  public class LiteMapRecentlyVisitedItemViewModel : ViewModelBase
  {
    #region Static
    /// <summary>
    /// The default maximum number of elements to hold on to
    /// </summary>
    private static int DefaultMaxRecentlyVisitedItems = 10;

    /// <summary>
    /// The property specifying the selected item
    /// </summary>
    private static string SelectedRecentlyVisitedItemPropertyName = "SelectedRecentlyVisitedItem";

    /// <summary>
    /// The property indicating whether there are recent items
    /// </summary>
    private static string HasRecentlyVisitedItemsPropertyName = "HasRecentlyVisitedItems";

    /// <summary>
    /// The boolean property specifying the recent items are visible
    /// </summary>
    public static string RecentlyVisitedItemsIsVisiblePropertyName = "RecentlyVisitedItemsIsVisible";

    /// <summary>
    /// The visibility typed version of the flag indicating whether the recent items are visible
    /// </summary>
    private static string RecentlyVisitedItemsVisibilityPropertyName = "RecentlyVisitedItemsVisibility";
    #endregion

    #region Fields
    /// <summary>
    /// Are the recent items visible
    /// </summary>
    private bool _recentlyVisitedItemsIsVisible;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the recent items view model
    /// </summary>
    public LiteMapRecentlyVisitedItemViewModel(Messenger messenger = null)
      : base(messenger)
    {
      MaxRecentlyVisitedItems = DefaultMaxRecentlyVisitedItems;

      if (!IsInDesignMode)
      {
        AttachToMessenger();

        RecentlyVisitedItems = new ObservableCollection<LiteGoToGeometryRequestMessage>();

        // Set up the resources
        this.Resources = new ApplicationResources();
      }
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attaches to the messenger, handling go-to requests for storage in our history
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<LiteGoToGeometryRequestMessage>(this, true, HandleGoToEnvelopeRequest);
      }
    }

    /// <summary>
    /// Handles the Go-To envelope request, actually not doing any Go-To action, but just
    /// recording this ins our history for resending later upon request
    /// </summary>
    private void HandleGoToEnvelopeRequest(LiteGoToGeometryRequestMessage request)
    {
      HandleGoToEnvelopeRequest(request, false);
    }

    /// <summary>
    /// Handles the Go-To envelope request, actually not doing any Go-To action, but just
    /// recording this ins our history for resending later upon request. 
    /// </summary>
    private void HandleGoToEnvelopeRequest(LiteGoToGeometryRequestMessage request, bool force)
    {
      if (request.StoreInHistory && request != null && !String.IsNullOrEmpty(request.Description) && request.Envelope != null)
      {
        // In case of default behavior (not forcing), remove any previous references to the same description
        // This is very loose, but the description is all the user sees so there is no way of distinguishing
        // between two similar descriptions anyway.
        if (!force)
        {
          var description = request.Description;
          for (int nr = RecentlyVisitedItems.Count - 1; nr >= 0; nr--)
          {
            if (String.Compare(description, RecentlyVisitedItems[nr].Description, StringComparison.Ordinal) == 0)
            {
              RecentlyVisitedItems.RemoveAt(nr);
            }
          }
        }

        // Insert the request
        RecentlyVisitedItems.Insert(0, request);

        if (RecentlyVisitedItems.Count > MaxRecentlyVisitedItems)
        {
          // We have more than the allowed number of elements; get rid of the least
          // recently visited item
          RecentlyVisitedItems.RemoveAt(RecentlyVisitedItems.Count - 1);
        }
      }

      // Notify the world that our history has changed
      RaisePropertyChanged(HasRecentlyVisitedItemsPropertyName);
    }
    #endregion

    #region Send Selected Request
    /// <summary>
    /// Sends the GoTo envelope request on the messsenger
    /// </summary>
    private async void SendGoToEnvelopeRequest(LiteGoToGeometryRequestMessage request)
    {
      RecentlyVisitedItemsIsVisible = false;

      // Allow the UI some breathing time
      await TaskEx.Yield();

      // Remove the request
      RecentlyVisitedItems.Remove(request);

      Messenger.Send(request);
    }
    #endregion

    #region API
    /// <summary>
    /// The maximum number of recently visited items
    /// </summary>
    public int MaxRecentlyVisitedItems
    {
      get;
      set;
    }

    /// <summary>
    /// The list of recently visited extents that are stored 
    /// </summary>
    public ObservableCollection<LiteGoToGeometryRequestMessage> RecentlyVisitedItems
    {
      get;
      private set;
    }

    /// <summary>
    /// In case an item is selected, we send the corresponding go-to request on the messenger
    /// </summary>
    public LiteGoToGeometryRequestMessage SelectedRecentlyVisitedItem
    {
      get { return null; }
      set
      {
        if (value != null)
        {
          RaisePropertyChanged(SelectedRecentlyVisitedItemPropertyName);
          SendGoToEnvelopeRequest(value);
        }
      }
    }

    /// <summary>
    /// A flag indicating whether there are recently visited items
    /// </summary>
    public bool HasRecentlyVisitedItems
    {
      get { return this.RecentlyVisitedItems.Count > 0; }
    }

    /// <summary>
    /// A flag indicating whether the recently visited items are/should be
    /// visible to the user
    /// </summary>
    public bool RecentlyVisitedItemsIsVisible
    {
      get { return _recentlyVisitedItemsIsVisible; }
      set
      {
        if (value != _recentlyVisitedItemsIsVisible)
        {
          _recentlyVisitedItemsIsVisible = value;
          RaisePropertyChanged(RecentlyVisitedItemsIsVisiblePropertyName);
          RaisePropertyChanged(RecentlyVisitedItemsVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Holds visible when the recently visited items are/should be presented
    /// to the user, collapsed otherwise.
    /// </summary>
    public Visibility RecentlyVisitedItemsVisibility
    {
      get { return RecentlyVisitedItemsIsVisible ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
