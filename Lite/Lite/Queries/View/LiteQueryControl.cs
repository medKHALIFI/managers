using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace Lite
{
  /// <summary>
  /// The control for one individual query
  /// </summary>
  [
    TemplatePart(Name = LiteQueryControl.TemplatePartOptionsControl, Type = typeof(FrameworkElement)),
    TemplatePart(Name = LiteQueryControl.TemplatePartElementControl, Type = typeof(FrameworkElement)),
  ]
  public class LiteQueryControl : Control
  {    
    #region Dependency Properties
    /// <summary>
    /// The isExpanded property (name)
    /// </summary>
    public const String IsExpandedPropertyName = "IsExpanded";

    /// <summary>
    /// The expanded property
    /// </summary>
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(IsExpandedPropertyName, typeof(Boolean), typeof(LiteQueryControl), new PropertyMetadata(false, OnPropertyChanged));
    #endregion

    #region Template part names
    /// <summary>
    /// The options framework element
    /// </summary>
    public const String TemplatePartOptionsControl = "MenuControl";

    /// <summary>
    /// The element control framework element
    /// </summary>
    public const String TemplatePartElementControl = "ElementControl";
    #endregion

    #region Private Fields
    /// <summary>
    /// The options control as set by the template
    /// </summary>
    private FrameworkElement _optionsControl;

    /// <summary>
    /// The element control as set by the template
    /// </summary>
    private FrameworkElement _elementControl;

    /// <summary>
    /// The typed datacontext
    /// </summary>
    private LiteQueryViewModel _viewModel;

    /// <summary>
    /// The storyboard for handling expand/collapse animation
    /// </summary>
    private Storyboard _expandCollapseStoryBoard;
    #endregion

    #region Constructors
    /// <summary>
    /// Default Constructor
    /// </summary>
    public LiteQueryControl()
    {
      this.DefaultStyleKey = typeof(LiteQueryControl);

      // Create empty storyboards
      _expandCollapseStoryBoard = new Storyboard();

      this.MouseLeftButtonDown += QueryControl_MouseLeftButtonDown;

      // Subscribe to the datacontext event
      this.DataContextChanged += QueryControl_DataContextChanged;
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Override when the control template is applied
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _elementControl = GetTemplateChild(TemplatePartElementControl) as FrameworkElement;
      _optionsControl = GetTemplateChild(TemplatePartOptionsControl) as FrameworkElement;

      SetupDataContext();
    }
    #endregion

    #region Setup
    /// <summary>
    /// Setup the datacontext for this control
    /// </summary>
    private void SetupDataContext()
    {
      _viewModel = DataContext as LiteQueryViewModel;

      if (_viewModel != null)
      {
        SetBinding(IsExpandedProperty, new Binding(LiteQueryViewModel.IsExpandedPropertyName) { Mode = BindingMode.TwoWay });
      }

      UpdateVisualState(false);
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
      // Clear the storyboard
      _expandCollapseStoryBoard.Stop();
      _expandCollapseStoryBoard.Children.Clear();

      if (useTransitions)
      {
        if (_optionsControl != null)
        {
          DoubleAnimation _optionsAnimation = new DoubleAnimation() { From = 0, To = GetDesiredControlHeight(_optionsControl), Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new QuadraticEase() };
          Storyboard.SetTarget(_optionsAnimation, _optionsControl);
          Storyboard.SetTargetProperty(_optionsAnimation, new PropertyPath(FrameworkElement.HeightProperty));
          _expandCollapseStoryBoard.Children.Add(_optionsAnimation);
        }

        if (_elementControl != null)
        {
          DoubleAnimation elementAnimation = new DoubleAnimation() { From = 0, To = GetDesiredControlHeight(_elementControl), Duration = TimeSpan.FromSeconds(0.3), EasingFunction = new QuadraticEase() };
          Storyboard.SetTarget(elementAnimation, _elementControl);
          Storyboard.SetTargetProperty(elementAnimation, new PropertyPath(FrameworkElement.HeightProperty));
          _expandCollapseStoryBoard.Children.Add(elementAnimation);
        }

        _expandCollapseStoryBoard.Begin();
      }
      else
      {
        if (_optionsControl != null)
        {
          _optionsControl.Height = GetDesiredControlHeight(_optionsControl);
        }

        if (_elementControl != null)
        {
          _elementControl.Height = GetDesiredControlHeight(_elementControl);
        }
      }
    }

    /// <summary>
    /// Create the storyboard for the collapsed state
    /// </summary>
    private void GotoCollapsedState(bool useTransitions = true)
    {
      // Clear the storyboard
      _expandCollapseStoryBoard.Stop();
      _expandCollapseStoryBoard.Children.Clear();

      if (useTransitions)
      {
        if (_optionsControl != null)
        {
          DoubleAnimation optionsAnimation = new DoubleAnimation() { From = GetDesiredControlHeight(_optionsControl), To = 0, Duration = TimeSpan.FromSeconds(0.2), EasingFunction = new QuadraticEase() };
          Storyboard.SetTarget(optionsAnimation, _optionsControl);
          Storyboard.SetTargetProperty(optionsAnimation, new PropertyPath(FrameworkElement.HeightProperty));
          _expandCollapseStoryBoard.Children.Add(optionsAnimation);
        }

        if (_elementControl != null)
        {
          DoubleAnimation elementAnimation = new DoubleAnimation() { From = GetDesiredControlHeight(_elementControl), To = 0, Duration = TimeSpan.FromSeconds(0.3), EasingFunction = new QuadraticEase() };
          Storyboard.SetTarget(elementAnimation, _elementControl);
          Storyboard.SetTargetProperty(elementAnimation, new PropertyPath(FrameworkElement.HeightProperty));
          _expandCollapseStoryBoard.Children.Add(elementAnimation);
        }

        _expandCollapseStoryBoard.Begin();
      }
      else
      {
        if (_optionsControl != null)
        {
          _optionsControl.Height = 0;
        }

        if (_elementControl != null)
        {
          _elementControl.Height = 0;
        }
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

    #region Callbacks
    /// <summary>
    /// Callback when the user clicks the control
    /// </summary>
    void QueryControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      if (_viewModel != null)
      {
        // Toggle the expanded state if required
        //_viewModel.IsExpanded = !_viewModel.IsExpanded;
      }
    }

    /// <summary>
    /// Callback when the datacontext changes
    /// </summary>
    void QueryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      SetupDataContext();
    }

    /// <summary>
    /// Callback when one of the dependency properties changes
    /// </summary>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (LiteQueryControl)d;
      if (e.Property == IsExpandedProperty)
      {
        control.UpdateVisualState();
      }
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
