using Microsoft.Practices.ServiceLocation;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features.Editability;
using SpatialEye.Framework.Features.Services;
using SpatialEye.Framework.ServiceProviders;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lite
{
  /// <summary>
  /// The viewModel responsible for the features that can be inserted
  /// </summary>
  public class FeatureInsertViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The items name for property change notification
    /// </summary>
    public static string ItemsPropertyName = "Items";
    #endregion

    #region Private Fields
    /// <summary>
    /// The items that can be inserted
    /// </summary>
    private SortedObservableCollection<FeatureInsertItemViewModel> _insertItems;
    #endregion

    #region Constructors
 
    /// <summary>
    /// Default Constrstuctor
    /// </summary>
    public FeatureInsertViewModel()
    {
      _insertItems = CreateItemCollection();

      // Setup some defaults
      DoInsertAttached = true;
      DoInsertUnattached = true;

      // The categories to handle
      Categories = new[]
      {
        FeatureTableEditabilityProperties.Categories.GeoNotes
      };
    }
    #endregion

    #region Authentication Changed
    /// <summary>
    /// Callback for authentication changes; updates the content of the insert items
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      if (isAuthenticated)
      {
        // Setup the insertable items
        SetupItems();
      }
      else
      {
        // Not authenticated - clear the items
        ClearItems();
      }
    }
    #endregion

    #region Private Members
    /// <summary>
    /// Is the category allowed
    /// </summary>
    private bool AllowCategory(FeatureTableEditabilityProperties.Categories category)
    {
      var isAllowed = false;
      if (this.Categories != null)
      {
        foreach (var allowed in this.Categories)
        {
          if (allowed == category)
          {
            isAllowed = true;
            break;
          }
        }
      }

      return isAllowed;
    }

    /// <summary>
    /// Clears the items
    /// </summary>
    private void ClearItems()
    {
      _insertItems.Clear();
      OnItemsChanged();
    }

    /// <summary>
    /// Setup the insertable items
    /// </summary>
    private async void SetupItems()
    {
      var service = ServiceLocator.Current.GetInstance<ICollectionService>();

      // Get all the bare descriptors
      var request = new GetDDRequest { IncludeFields = false, GroupTypes = new ServiceProviderGroupType[] { ServiceProviderGroupType.Business } };
      var sourceDescriptors = await service.GetDDAsync(request);

      var items = CreateItemCollection();

      foreach (var descriptor in sourceDescriptors)
      {
        // Only process updatable and allowed sources
        if (descriptor.EditabilityProperties.AllowUpdate)
        {
          foreach (var tableDescriptor in descriptor.TableDescriptors)
          {
            var allowed = LiteClientSettingsViewModel.Instance.IsGeoNoteAllowed(tableDescriptor.ExternalName);
            var editProps = tableDescriptor.EditabilityProperties;

            if (allowed && editProps.AllowInsert && AllowCategory(editProps.Category))
            {
              if (this.DoInsertAttached && DoInsertUnattached)
              {
                bool isAttachRequired = editProps.IsAttachRequired;
                bool isAttachPossible = editProps.IsAttachPossible;

                // Do insert attached and attach is possible
                items.Add(new FeatureInsertItemViewModel(tableDescriptor, isAttachRequired, isAttachPossible));
              }
              else
              {
                if (this.DoInsertAttached)
                {
                  // Insert attached
                  if (editProps.IsAttachPossible)
                  {
                    // Do insert attached and attach is possible
                    items.Add(new FeatureInsertItemViewModel(tableDescriptor, true, true));
                  }
                }
                else if (this.DoInsertUnattached)
                {
                  // Insert unattached
                  if (!editProps.IsAttachRequired)
                  {
                    // Do insert attached and attach is possible
                    items.Add(new FeatureInsertItemViewModel(tableDescriptor, false, false));
                  }
                }
              }
            }
          }
        }
      }

      // Set the items
      this.Items = items;
    }

    /// <summary>
    /// Creates a new sorted item collection
    /// </summary>
    /// <returns>a new sorted item collection</returns>
    private SortedObservableCollection<FeatureInsertItemViewModel> CreateItemCollection()
    {
      return new SortedObservableCollection<FeatureInsertItemViewModel>((a, b) => a.Name.CompareTo(b.Name));
    }

    #endregion

    #region Public Members

    /// <summary>
    /// Called when the items changes
    /// </summary>
    protected virtual void OnItemsChanged()
    {
      RaisePropertyChanged(ItemsPropertyName);
    }
    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the lists of insertable items
    /// </summary>
    public SortedObservableCollection<FeatureInsertItemViewModel> Items
    {
      get { return _insertItems; }
      private set
      {
        if (value != _insertItems)
        {
          _insertItems = value;
          OnItemsChanged();
        }
      }
    }

    /// <summary>
    /// Do we want to do plain inserts
    /// </summary>
    public bool DoInsertUnattached
    {
      get;
      set;
    }

    /// <summary>
    /// Do we want to do attached inserts
    /// </summary>
    public bool DoInsertAttached
    {
      get;
      set;
    }

    /// <summary>
    /// Holds the categories for this insert manager
    /// </summary>
    public FeatureTableEditabilityProperties.Categories[] Categories
    {
      get;
      private set;
    }
    #endregion
  }
}
