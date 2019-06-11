using SpatialEye.Framework.Client;
using SpatialEye.Framework.ComponentModel;
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
  /// Menu item class
  /// </summary>
  public class LiteMenuItemViewModel : BindableObject
  {
    #region Constants
    /// <summary>
    /// The default icon resources
    /// </summary>
    public const string DefaultIconResource = "MetroIcon.Content.Item";

    /// <summary>
    /// The default title
    /// </summary>
    public const string DefaultTitle = "Item";
    #endregion

    #region Private Fields
    /// <summary>
    /// The icon resource identifier
    /// </summary>
    private String _iconResource;

    /// <summary>
    /// The title
    /// </summary>
    private String _title;

    /// <summary>
    /// The command to execute upon activation of the menu item
    /// </summary>
    private RelayCommand _command;

    /// <summary>
    /// The visibility state of the item
    /// </summary>
    private Visibility _visibility = Visibility.Visible;

    /// <summary>
    /// An, optional, function that can be used to determine visibility
    /// </summary>
    private Func<bool> _visibilityFunction;
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteMenuItemViewModel()
    {
      _iconResource = DefaultIconResource;
      _title = DefaultTitle;
    }

    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="title">the title of the item</param>
    public LiteMenuItemViewModel(string title)
    {
      _iconResource = DefaultIconResource;
      _title = title;
    }

    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="title">the title if the item</param>
    /// <param name="iconResource">the icon resource of the item</param>
    public LiteMenuItemViewModel(string title, string iconResource)
    {
      _iconResource = iconResource;
      _title = title;
    }
    #endregion

    #region State
    /// <summary>
    /// Check the state
    /// </summary>
    internal void CheckState()
    {
      var command = Command;
      if(command != null)
      {
        command.RaiseCanExecuteChanged();
      }

      var visibilityFunction = _visibilityFunction;
      if (visibilityFunction != null)
      {
        var isVisible = visibilityFunction();
        Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the command to execute when the menu item is pressed
    /// </summary>
    public RelayCommand Command
    {
      get { return _command; }
      set
      {
        _command = value;
        RaisePropertyChanged();
      }
    }

    /// <summary>
    /// The visibility function to determine the visibility of this element
    /// </summary>
    public Func<bool> VisibilityPredicate
    {
      get { return _visibilityFunction; }
      set
      {
        if (value != null)
        {
          _visibilityFunction = value;
          Visibility = Visibility.Collapsed;
        }
      }
    }

    /// <summary>
    /// The visibility of the element
    /// </summary>
    public Visibility Visibility
    {
      get { return _visibility; }
      set
      {
        if (_visibility != value)
        {
          _visibility = value;
          RaisePropertyChanged();
        }
      }
    }

    /// <summary>
    /// Gets or sets the resource name containing the icon/image for the menu item
    /// </summary>
    public String IconResource
    {
      get { return _iconResource; }
      set
      {
        if (value != _iconResource)
        {
          _iconResource = value;
          RaisePropertyChanged();
        }
      }
    }

    /// <summary>
    /// Gets or sets the title of the menu item
    /// </summary>
    public String Title
    {
      get { return _title; }
      set
      {
        if (value != _title)
        {
          _title = value;
          RaisePropertyChanged();
        }
      }
    }
    #endregion
  }
}
