using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ComponentModel.Design;

namespace Lite
{
  /// <summary>
  /// The select-hover view, that pops up when the user presses and holds the Left Mouse Button 
  /// </summary>
  public partial class LiteMapSelectHoverView : UserControl
  {
    #region Dependency Properties
    /// <summary>
    /// The visibility of the view
    /// </summary>
    public const string ViewVisibilityPropertyName = "ViewVisibility";

    /// <summary>
    /// The view visibility
    /// </summary>
    public static DependencyProperty ViewVisibilityProperty = DependencyProperty.Register(ViewVisibilityPropertyName, typeof(Visibility), typeof(LiteMapSelectHoverView), new PropertyMetadata(Visibility.Collapsed, OnPropertyChanged));
    #endregion

    #region Private Properties
    /// <summary>
    /// Translate transform for positioning the control
    /// </summary>
    private TranslateTransform _positionTransform = new TranslateTransform();
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteMapSelectHoverView()
    {
      InitializeComponent();

      if (!DesignModeHelper.IsInDesignMode)
      {
        this.Loaded += Control_Loaded;
        this.DataContextChanged += AttachToContext;
      }
    }
    #endregion

    #region Property change handling
    /// <summary>
    /// Called whenever a dependency property of the view has changed
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var sender = d as LiteMapSelectHoverView;

      if (sender != null)
      {
        if (e.Property == ViewVisibilityProperty)
        {
          if (((Visibility)e.NewValue) == Visibility.Visible)
          {
            sender.UpdatePosition();
            sender.Visibility = Visibility.Visible;
          }
          else
          {
            sender.Visibility = Visibility.Collapsed;
          }
        }
      }
    }

    /// <summary>
    /// Callback when the control is loaded
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Control_Loaded(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// The datacontext has changed; attach to it
    /// </summary>
    void AttachToContext(object sender, DependencyPropertyChangedEventArgs e)
    {
      var notification = DataContext as LiteMapSelectHoverViewModel;

      if (notification != null)
      {
        SetBinding(ViewVisibilityProperty, new Binding("ViewVisibility") { Mode = BindingMode.OneWay });
      }
    }
    #endregion

    #region Position methods
    /// <summary>
    /// Do we need to close the dialog
    /// </summary>
    private void CheckForClosing(object sender, MouseEventArgs e)
    {
      if (Visibility == Visibility.Visible)
      {
        Point p = e.GetPosition((UIElement)this);
        var elems = VisualTreeHelper.FindElementsInHostCoordinates(p, (UIElement)this);
        if (!elems.Contains(this.ElementsRoot))
        {
          MouseManager.Instance.MouseMoveEvent -= CheckForClosing;
          ButtonAutomationPeer peer = new ButtonAutomationPeer(CloseButton);
          ((IInvokeProvider)peer).Invoke();
        }
      }
    }

    /// <summary>
    /// Updates the position of the control to the mouse position
    /// </summary>
    private void UpdatePosition()
    {
      Double offsetX = 16 + 60; // 75  is offset from border to gridview, 16 is move first list item under mouse
      Double offsetY = 16 + 57; // 75  is offset from border to gridview, 16 is move first list item under mouse
      Point p = MouseManager.Instance.CurrentMouseEventArgs.GetPosition(this);

      _positionTransform.X = p.X - offsetX;
      _positionTransform.Y = p.Y - offsetY;

      this.ElementsRoot.RenderTransform = _positionTransform;

      MouseManager.Instance.MouseMoveEvent += CheckForClosing;
    }
    #endregion
  }
}
