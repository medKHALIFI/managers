using SpatialEye.Framework.ComponentModel;
using System;
using System.Collections.ObjectModel;
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
  /// Menu Category class, the category contains the individual items
  /// </summary>
  public class LiteMenuCategoryViewModel : BindableObject
  {
    #region Private Fields

    /// <summary>
    /// Menu items holder
    /// </summary>
    private ObservableCollection<LiteMenuItemViewModel> _items;

    /// <summary>
    /// The visibility of the category border
    /// </summary>
    private Visibility _borderVisibity;

    #endregion

    #region Constructors

    /// <summary>
    /// Default Constructor
    /// </summary>
    public LiteMenuCategoryViewModel()
    {
      Items = new ObservableCollection<LiteMenuItemViewModel>();
      BorderVisibility = Visibility.Collapsed;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the menu items collection
    /// </summary>
    public ObservableCollection<LiteMenuItemViewModel> Items
    {
      get { return _items; }
      set
      {
        _items = value;
        RaisePropertyChanged();
      }
    }

    /// <summary>
    /// Gets or sets the category title
    /// </summary>
    public String Title { get; set; }

    /// <summary>
    /// Gets or set the category border visibility
    /// </summary>
    public Visibility BorderVisibility
    {
      get { return _borderVisibity; }
      set
      {
        if (value != _borderVisibity)
        {
          _borderVisibity = value;
          RaisePropertyChanged();
        }
      }
    }

    #endregion

    #region Internals

    /// <summary>
    /// Check the menu item commands
    /// </summary>
    internal void CheckStates()
    {
      foreach (var item in Items)
      {
        item.CheckState();
      }
    }

    #endregion

  }
}
