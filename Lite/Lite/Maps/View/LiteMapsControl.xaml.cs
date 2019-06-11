using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lite
{
  /// <summary>
  /// The Maps control, displaying the current themes/layers and allowing to extent the
  /// set of layers with new ones
  /// </summary>
  public partial class LiteMapsControl : UserControl
  {
    #region Static Property Names
    /// <summary>
    /// The isExpanded property
    /// </summary>
    public const String IsExpandedPropertyName = "IsExpanded";
    #endregion

    #region Dependency Properties
    /// <summary>
    /// The IsExpanded property, allows for binding 
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(IsExpandedPropertyName, typeof(Boolean), typeof(LiteMapsControl), new PropertyMetadata(false, OnPropertyChanged));
    #endregion

    #region Private Fields
    /// <summary>
    /// Menu height for caching
    /// </summary>
    private Double _menuHeight;

    /// <summary>
    /// The typed dataContext, which should be a LiteMapsViewModel
    /// </summary>
    private LiteMapsViewModel _viewModel;
    #endregion

    #region Constructors
    /// <summary>
    /// Default Constructor
    /// </summary>
    public LiteMapsControl()
    {
      InitializeComponent();

      _menuHeight = MapMenuControl.Height;

      DataContextChanged += LiteMapsControl_DataContextChanged;

      Loaded += LiteMapsControl_Loaded;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback when a dependency property changes
    /// </summary>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var sender = d as LiteMapsControl;

      if (sender != null && e.Property == IsExpandedProperty)
      {
        sender.UpdateVisualState();
      }
    }

    /// <summary>
    /// Callback when the datacontext of the control changes
    /// </summary>
    private void LiteMapsControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      _viewModel = DataContext as LiteMapsViewModel;

      if (_viewModel != null)
      {
        SetBinding(IsExpandedProperty, new Binding(LiteNewUserMapViewModel.AddMapViewVisiblePropertyName) { Mode = BindingMode.TwoWay, Source = _viewModel.NewMapViewModel });
      }
    }

    /// <summary>
    /// Calllback when the control is loaded
    /// </summary>
    private void LiteMapsControl_Loaded(object sender, RoutedEventArgs e)
    {
      UpdateVisualState(false);
    }

    /// <summary>
    /// Callback when the expand animation finished
    /// </summary>
    private void MapExpandStoryboardCompleted(object sender, EventArgs e)
    {
      if (MapMenuControl.Height > 0 && _menuHeight.Equals(Double.NaN))
      {
        MapMenuControl.Height = Double.NaN;
      }
    }
    #endregion

    #region Visual States
    /// <summary>
    /// Update the visual state of this control
    /// </summary>
    private void UpdateVisualState(bool useTransitions = true)
    {
      if (IsExpanded)
      {
        GotoExpandedState(useTransitions);
      }
      else
      {
        GotoCollapsedState(useTransitions);
      }
    }

    /// <summary>
    /// Create the storyboard for the expanded state
    /// </summary>
    private void GotoExpandedState(bool useTransitions = true)
    {
      ExpandStoryboard.Stop();

      if (useTransitions)
      {
        MenuAnimation.From = 0;
        MenuAnimation.To = (_menuHeight.Equals(Double.NaN)) ? GetDesiredControlHeight(MapMenuControl) : _menuHeight;
        ExpandStoryboard.Begin();
      }
      else
      {
        MapMenuControl.Height = _menuHeight;
      }
    }

    /// <summary>
    /// Create the storyboard for the collapsed state
    /// </summary>
    private void GotoCollapsedState(bool useTransitions = true)
    {
      ExpandStoryboard.Stop();

      if (useTransitions)
      {
        MenuAnimation.From = (_menuHeight.Equals(Double.NaN)) ? GetDesiredControlHeight(MapMenuControl) : _menuHeight;
        MenuAnimation.To = 0;
        ExpandStoryboard.Begin();
      }
      else
      {
        MapMenuControl.Height = 0;
      }
    }

    /// <summary>
    /// Calculate the height of the given control
    /// </summary>
    private double GetDesiredControlHeight(FrameworkElement control)
    {
      bool setToZero = false;

      if (control.Height == 0)
      {
        control.Height = Double.NaN;
        setToZero = true;
      }

      control.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
      var result = control.DesiredSize.Height;

      if (setToZero)
      {
        control.Height = 0;
      }

      return result;
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Get or  set the expanded state of the control
    /// </summary>
    public Boolean IsExpanded
    {
      get { return (Boolean)GetValue(IsExpandedProperty); }
      set
      {
        SetValue(IsExpandedProperty, value);
      }
    }
    #endregion
  }
}
