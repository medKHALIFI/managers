using Lite.Resources.Localization;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Lite
{
  /// <summary>
  /// The insert view model with specifics for Lite
  /// </summary>
  public class LiteFeatureInsertViewModel : FeatureInsertViewModel
  {
    #region Property Names
    /// <summary>
    /// The ViewVisibility
    /// </summary>
    public const string ViewVisibilityPropertyName = "ViewVisibility";

    /// <summary>
    /// The enabled state of the view
    /// </summary>
    public const string ViewIsEnabledPropertyName = "ViewIsEnabled";
    #endregion

    #region Private Fields
    /// <summary>
    /// Holder for the menu items
    /// </summary>
    private ObservableCollection<LiteMenuCategoryViewModel> _menuItems;

    /// <summary>
    /// Is the insert view active
    /// </summary>
    private bool _viewIsActive;

    /// <summary>
    /// Is the user allowed to insert new features
    /// </summary>
    private bool _allowInserts;

    /// <summary>
    /// Indicates the state of the edit mode
    /// </summary>
    private bool _editModeActive;
    #endregion

    #region Constructor
    public LiteFeatureInsertViewModel()
    {
      AttachToMessenger();

      _menuItems = new ObservableCollection<LiteMenuCategoryViewModel>();
      _allowInserts = true;
      _editModeActive = false;
    }
    #endregion

    #region MapViewModel Change
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      this.Messenger.Register<LiteFeatureDetailsEditModeChangedMessage>(this, HandleEditModeChange);
      this.Messenger.Register<LiteGetAttachCandidatesRequestMessage>(this, HandleCalculateAttachCandidates);
      this.Messenger.Register<LiteMapSelectionChangedMessage>(this, HandleMapSelectionChange);
    }

    /// <summary>
    /// Handles the edit mode change
    /// </summary>
    private void HandleEditModeChange(LiteFeatureDetailsEditModeChangedMessage message)
    {
      // Set the current state of the edit mode
      this.EditModeActive = message.EditModeActive;
    }

    /// <summary>
    /// Calculates the attach candidates for the table in the specified message
    /// </summary>
    private void HandleCalculateAttachCandidates(LiteGetAttachCandidatesRequestMessage message)
    {
      if (message != null && message.TableDescriptor != null)
      {
        var items = this.Items;
        if (items != null)
        {
          var attachCandidates = new List<FeatureInsertItemViewModel>();

          foreach (var item in items)
          {
            if (item.CanAttachTo(message.TableDescriptor))
            {
              attachCandidates.Add(item);
            }
          }

          // And update the request message to be picked up by the sender
          message.AttachCandidates = attachCandidates;
        }
      }
    }

    /// <summary>
    /// Callback for changes in a property of the MapView
    /// </summary>
    void HandleMapSelectionChange(LiteMapSelectionChangedMessage change)
    {
      var element = change.SelectedFeatureGeometry;
      SetSelectedTrail(element);

      if (DoInsertAttached)
      {
        var selection = change.SelectedFeatureGeometry;
        var items = Items;
        if (items != null)
        {
          foreach (var item in items)
          {
            item.SetAttachmentCandidate(selection);
          }
        }

        // Set visibility/state on items
        CheckMenuItemStates();
      }
    }

    /// <summary>
    /// Sets the last selected trail
    /// </summary>
    private void SetSelectedTrail(Collection<FeatureTargetGeometry> selectedElements)
    {
      IFeatureGeometry selectedTrail = null;
      if (selectedElements.Count == 1)
      {
        var selection = selectedElements[0];

        selectedTrail = LiteMapTrailViewModel.IsTrailFeature(selection.Feature) ? selection.TargetGeometry : null;
      }

      var items = Items;
      if (items != null)
      {
        foreach (var item in items)
        {
          item.CandidateStartWithGeometry = selectedTrail;
        }
      }
    }

    /// <summary>
    /// Let commands check their enabled state via checking state
    /// </summary>
    protected virtual void CheckMenuItemStates()
    {
      var menuItems = this.MenuItems;
      if (menuItems != null)
      {
        foreach (var menuItem in menuItems)
        {
          menuItem.CheckStates();
        }
      }
    }
    #endregion

    #region Menu Handling
    /// <summary>
    /// Setup the menu
    /// </summary>
    private void SetupMenu()
    {
      var menu = new ObservableCollection<LiteMenuCategoryViewModel>();

      var cat = new LiteMenuCategoryViewModel { Title = ApplicationResources.FeatureEditInsertCategoryHeader };

      foreach (var item in Items)
      {
        var menuItem = new LiteMenuItemViewModel(item.Name) { Command = new RelayCommand(() => InsertItem(item), () => item.IsEnabled) };
        cat.Items.Add(menuItem);
        item.PropertyChanged += (s, e) => SetupMenuItem(s, e, menuItem);
      }

      // Only add the category if there are any items
      if (cat.Items.Any())
      {
        menu.Add(cat);
      }

      // Set the menu
      MenuItems = menu;

      // Notify
      RaisePropertyChanged(ViewVisibilityPropertyName);
    }

    /// <summary>
    /// Setup a menuitem when the parent has changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="menuItem"></param>
    private void SetupMenuItem(object sender, System.ComponentModel.PropertyChangedEventArgs e, LiteMenuItemViewModel menuItem)
    {
      var item = sender as FeatureInsertItemViewModel;

      if (item != null)
      {
        if (e.PropertyName == FeatureInsertItemViewModel.PrimaryGeometryDescriptorPropertyName)
        {
          // Prime geom has changed, setup the menu item
          SetupMenuIcon(menuItem, item.PrimaryGeometryDescriptor.FieldType.PhysicalType);
        }
      }
    }

    /// <summary>
    /// Setup the menu icon
    /// </summary>
    /// <param name="menuItem">the menu item to setup </param>
    /// <param name="fieldType">geometry field type</param>
    private void SetupMenuIcon(LiteMenuItemViewModel menuItem, FeaturePhysicalFieldType fieldType)
    {
      var newIcon = LiteMenuItemViewModel.DefaultIconResource;

      switch (fieldType)
      {
        case FeaturePhysicalFieldType.Point:
        case FeaturePhysicalFieldType.MultiPoint:
          newIcon = "MetroIcon.Content.TrailPoint"; break;
        case FeaturePhysicalFieldType.Curve:
        case FeaturePhysicalFieldType.MultiCurve:
          newIcon = "MetroIcon.Content.TrailCurve"; break;
        case FeaturePhysicalFieldType.Polygon:
        case FeaturePhysicalFieldType.MultiPolygon:
          newIcon = "MetroIcon.Content.TrailPolygon"; break;
      }

      menuItem.IconResource = newIcon;
    }

    /// <summary>
    /// Are there any menu items
    /// </summary>
    private bool HasMenuItems()
    {
      return MenuItems.Any((a) => a.Items.Any());
    }
    #endregion

    #region CultureChanged
    /// <summary>
    /// Callback when the culture changes
    /// </summary>
    /// <param name="currentCultureInfo"></param>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      SetupMenu();
    }
    #endregion

    #region AuthenticationChanged
    /// <summary>
    /// Authentication changed callback
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Set the authorization options
        AllowInserts = LiteClientSettingsViewModel.Instance.AllowGeoNoteEdits;
      }
    }
    #endregion

    #region Menu Item Activation
    /// <summary>
    /// Start the interaction for inserting the new item
    /// </summary>
    private void InsertItem(FeatureInsertItemViewModel insertItem)
    {
      ViewIsActive = false;

      // Get the request
      var request = insertItem.ToNewFeatureRequest();

      if (request != null)
      {
        // The request is valid; send it to the messenger
        Messenger.Send(request);
      }
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Called whenever the items changes
    /// </summary>
    protected override void OnItemsChanged()
    {
      // Make sure default behavior applies
      base.OnItemsChanged();

      SetupMenu();
    }
    #endregion

    #region Public Api
    /// <summary>
    /// A flag indicating whether the editmode should be active
    /// </summary>
    public Boolean EditModeActive
    {
      get { return _editModeActive; }
      set
      {
        if (value != _editModeActive)
        {
          _editModeActive = value;

          RaisePropertyChanged();
          RaisePropertyChanged(ViewIsEnabledPropertyName);
        }
      }
    }

    /// <summary>
    /// Gets or sets the menu items
    /// </summary>
    public ObservableCollection<LiteMenuCategoryViewModel> MenuItems
    {
      get { return _menuItems; }
      set
      {
        _menuItems = value;
        RaisePropertyChanged();
      }
    }

    /// <summary>
    /// Is the user allowed to insert new items
    /// </summary>
    public bool AllowInserts
    {
      get { return _allowInserts; }
      private set
      {
        if (value != _allowInserts)
        {
          _allowInserts = value;
          RaisePropertyChanged();
          RaisePropertyChanged(ViewVisibilityPropertyName);
        }
      }
    }

    #endregion

    #region View
    /// <summary>
    /// Gets or sets the value if the insert view is active
    /// </summary>
    public bool ViewIsActive
    {
      get { return _viewIsActive; }
      set
      {
        if (value != _viewIsActive)
        {
          _viewIsActive = value;
          RaisePropertyChanged();
        }
      }
    }

    /// <summary>
    /// Gets the enabled state of the view
    /// </summary>
    public bool ViewIsEnabled
    {
      get { return !EditModeActive; }
    }

    /// <summary>
    /// Gets the view visibility
    /// </summary>
    public Visibility ViewVisibility
    {
      get { return (HasMenuItems() && AllowInserts) ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
