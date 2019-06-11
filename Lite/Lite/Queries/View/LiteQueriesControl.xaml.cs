using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Lite
{
  /// <summary>
  /// Holds the Lite Queries Control, displaying all queries as set up in the LiteQueriesViewModel
  /// </summary>
  public partial class LiteQueriesControl : UserControl
  {
    #region Property Names
    /// <summary>
    /// The property name used for notification change
    /// </summary>
    public const String MenuVisibilityPropertyName = "MenuVisibility";
    #endregion

    #region Dependency Properties
    /// <summary>
    /// The menu visibility property
    /// </summary>
    public static readonly DependencyProperty MenuVisibilityProperty = DependencyProperty.Register(MenuVisibilityPropertyName, typeof(Visibility), typeof(LiteQueriesControl), new PropertyMetadata(Visibility.Collapsed, OnPropertyChanged));
    #endregion

    #region Private Fields
    /// <summary>
    /// Menu height for caching
    /// </summary>
    private Double _menuHeight;

    /// <summary>
    /// The dataContext stored typed
    /// </summary>
    private LiteQueriesViewModel _viewModel;
    #endregion

    #region Constructors
    /// <summary>
    /// Default Constructor
    /// </summary>
    public LiteQueriesControl()
    {
      InitializeComponent();

      _menuHeight = MenuControl.Height;

      Loaded += LiteQueriesControl_Loaded;
      DataContextChanged += LiteQueriesControl_DataContextChanged;
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback when a dependency property changes
    /// </summary>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var sender = d as LiteQueriesControl;

      if (sender != null)
      {
        if (e.Property == MenuVisibilityProperty)
        {
          sender.UpdateVisualState();
        }
      }
    }

    /// <summary>
    /// Calllback when the control is loaded
    /// </summary>
    private void LiteQueriesControl_Loaded(object sender, RoutedEventArgs e)
    {
      UpdateVisualState(false);
    }

    /// <summary>
    /// Callback when the datacontext of the control changes
    /// </summary>
    private void LiteQueriesControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (_viewModel != null)
      {
        _viewModel.PropertyChanged -= viewModel_PropertyChanged;
      }

      _viewModel = DataContext as LiteQueriesViewModel;

      if (_viewModel != null)
      {
        _viewModel.PropertyChanged += viewModel_PropertyChanged;

        SetupDataContext();
      }
    }

    /// <summary>
    /// Set up the model context
    /// </summary>
    void SetupDataContext()
    {
      if (_viewModel != null)
      {
        if (_viewModel.NewQueryViewModel != null)
        {
          SetBinding(MenuVisibilityProperty, new Binding(LiteQueriesViewModel.NewQueryViewVisibilityPropertyName));
        }
      }
    }
    /// <summary>
    /// Callback when the attached viewmodel property changes
    /// </summary>
    void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      var owner = sender as LiteQueriesViewModel;

      if (owner != null)
      {
        // Do react to changes
      }
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
      if (MenuVisibility == Visibility.Visible)
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
    public Visibility MenuVisibility
    {
      get { return (Visibility)GetValue(MenuVisibilityProperty); }
      set
      {
        SetValue(MenuVisibilityProperty, value);
      }
    }
    #endregion
  }
}
