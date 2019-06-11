using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lite
{
  /// <summary>
  /// The Themes control, displaying the current themes/layers and allowing to extent the
  /// set of layers with new ones
  /// </summary>
  public partial class LiteThemesControl : UserControl
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
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(IsExpandedPropertyName, typeof(Boolean), typeof(LiteThemesControl), new PropertyMetadata(false, OnPropertyChanged));
    #endregion

    #region Private Fields
    /// <summary>
    /// Menu height for caching
    /// </summary>
    private Double _menuHeight;

    /// <summary>
    /// The typed dataContext, which should be a LiteThemesViewModel
    /// </summary>
    private LiteMapThemesViewModel _viewModel;
    #endregion

    #region Constructors
    /// <summary>
    /// Default Constructor
    /// </summary>
    public LiteThemesControl()
    {
      InitializeComponent();

      _menuHeight = MenuControl.Height;

      DataContextChanged += LiteThemesControl_DataContextChanged;

      Loaded += LiteThemesControl_Loaded;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback when a dependency property changes
    /// </summary>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var sender = d as LiteThemesControl;

      if (sender != null && e.Property == IsExpandedProperty)
      {
        sender.UpdateVisualState();
      }
    }

    /// <summary>
    /// Callback when the datacontext of the control changes
    /// </summary>
    private void LiteThemesControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (_viewModel != null)
      {
        _viewModel.PropertyChanged -= viewModel_PropertyChanged;
      }

      _viewModel = DataContext as LiteMapThemesViewModel;

      if (_viewModel != null)
      {
        _viewModel.PropertyChanged += viewModel_PropertyChanged;

        if (_viewModel.NewMapLayerViewModel != null)
        {
          SetBinding(IsExpandedProperty, new Binding(LiteNewMapLayerViewModel.AddMapLayerViewVisiblePropertyName) { Mode = BindingMode.TwoWay, Source = _viewModel.NewMapLayerViewModel });
        }
      }
    }

    /// <summary>
    /// Callback when the attached viewmodel property changes
    /// </summary>
    void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == LiteMapThemesViewModel.NewMapLayerViewModelPropertyName)
      {
        if (_viewModel != null && _viewModel.NewMapLayerViewModel != null)
        {
          SetBinding(IsExpandedProperty, new Binding(LiteNewMapLayerViewModel.AddMapLayerViewVisiblePropertyName) { Mode = BindingMode.TwoWay, Source = _viewModel.NewMapLayerViewModel });
        }
      }
    }

    /// <summary>
    /// Calllback when the control is loaded
    /// </summary>
    private void LiteThemesControl_Loaded(object sender, RoutedEventArgs e)
    {
      UpdateVisualState(false);
    }

    /// <summary>
    /// Callback when the expand animation finished
    /// </summary>
    private void ExpandStoryboardCompleted(object sender, EventArgs e)
    {
      if (MenuControl.Height > 0 && _menuHeight.Equals(Double.NaN))
      {
        MenuControl.Height = Double.NaN;
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
        MenuAnimation.To = (_menuHeight.Equals(Double.NaN)) ? GetDesiredControlHeight(MenuControl) : _menuHeight;
        ExpandStoryboard.Begin();
      }
      else
      {
        MenuControl.Height = _menuHeight;
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
        MenuAnimation.From = (_menuHeight.Equals(Double.NaN)) ? GetDesiredControlHeight(MenuControl) : _menuHeight;
        MenuAnimation.To = 0;
        ExpandStoryboard.Begin();
      }
      else
      {
        MenuControl.Height = 0;
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
