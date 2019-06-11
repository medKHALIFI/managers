using SpatialEye.Framework.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lite
{
  /// <summary>
  /// The LiteSubMapBar is the control holding one popup element in the mapbar
  /// </summary>
  [
    TemplatePart(Name = LiteSubMapBar.TemplatePartRootElement, Type = typeof(FrameworkElement)),
    TemplatePart(Name = LiteSubMapBar.TemplatePartContentElement, Type = typeof(ContentControl)),
    TemplatePart(Name = LiteSubMapBar.StoryBoardActivationAnimation, Type = typeof(Storyboard)),
    TemplatePart(Name = LiteSubMapBar.StoryBoardDeactivationAnimation, Type = typeof(Storyboard))
  ]
  public class LiteSubMapBar : ContentControl
  {
    #region Template part names
    public const String TemplatePartRootElement = "RootElement";
    public const String TemplatePartContentElement = "ContentElement";
    public const String StoryBoardActivationAnimation = "ActivationAnimation";
    public const String StoryBoardDeactivationAnimation = "DeactivationAnimation";
    #endregion

    #region Dependency Properties
    /// <summary>
    /// The IsActive property name
    /// </summary>
    public const String IsActivePropertyName = "IsActive";

    /// <summary>
    /// The IsActive property
    /// </summary>
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(IsActivePropertyName, typeof(Boolean), typeof(LiteSubMapBar), new PropertyMetadata(false, OnPropertyChanged));

    /// <summary>
    /// Activator Control property name
    /// </summary>
    public const string ActivatorControlNamePropertyName = "ActivatorControlName";

    /// <summary>
    /// Activator control property
    /// </summary>
    public static readonly DependencyProperty ActivatorControlNameProperty = DependencyProperty.Register(ActivatorControlNamePropertyName, typeof(String), typeof(LiteSubMapBar), new PropertyMetadata(null, OnPropertyChanged));

    /// <summary>
    /// Activator Control center position property
    /// </summary>
    public const string ActivatorControlCenterPositionPropertyName = "ActivatorControlCenterPosition";

    /// <summary>
    /// Activator control center position property
    /// </summary>
    public static readonly DependencyProperty ActivatorControlCenterPositionProperty = DependencyProperty.Register(ActivatorControlCenterPositionPropertyName, typeof(Point), typeof(LiteSubMapBar), new PropertyMetadata(new Point(0, 0)));

    /// <summary>
    /// The hittest margin of the content control
    /// </summary>
    public const string HitTestOffsetMarginName = "HitTestMargin";

    /// <summary>
    /// Hittest margin property
    /// </summary>
    public static readonly DependencyProperty HitTestMarginProperty = DependencyProperty.Register(HitTestOffsetMarginName, typeof(Thickness), typeof(LiteSubMapBar), new PropertyMetadata(new Thickness(15)));

    public event DependencyPropertyChangedEventHandler IsActiveChanged;
    #endregion

    #region Private Fields
    /// <summary>
    /// The main content element
    /// </summary>
    private ContentControl _contentElement;

    /// <summary>
    /// The storyboard upon appearing
    /// </summary>
    private Storyboard _activationAnimation;

    /// <summary>
    /// The storyboard upon disappearing
    /// </summary>
    private Storyboard _deactivationAnimation;

    /// <summary>
    /// The root element
    /// </summary>
    private FrameworkElement _rootElement;

    /// <summary>
    /// A flag indicating whether the template has been loaded yet
    /// </summary>
    private bool _templateLoaded;

    /// <summary>
    /// The owning map bar
    /// </summary>
    private LiteMapBar _mapBar;

    /// <summary>
    /// The control activating this
    /// </summary>
    private FrameworkElement _activatorControl;

    /// <summary>
    /// The timer that handles the moving away from the sub control but still
    /// allowing it to remain focused. When the timer expires and control is
    /// outside the sub control, the focus will be lost.
    /// </summary>
    private Timer _lostFocusTimer;
    #endregion

    #region Constructor
    /// <summary>
    /// The default constructor
    /// </summary>
    public LiteSubMapBar()
    {
      this.DefaultStyleKey = typeof(LiteSubMapBar);
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Override when the control template is applied
    /// </summary>
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      // Get the root element for extraction of th resources
      _rootElement = GetTemplateChild(TemplatePartRootElement) as FrameworkElement;
      _contentElement = GetTemplateChild(TemplatePartContentElement) as ContentControl;

      if (_rootElement != null)
      {
        _activationAnimation = _rootElement.Resources[StoryBoardActivationAnimation] as Storyboard;
        _deactivationAnimation = _rootElement.Resources[StoryBoardDeactivationAnimation] as Storyboard;
      }

      // Setup the activation components
      SetupActivationControl();

      // Setup the initial state
      ChangeVisualState();
      _templateLoaded = true;

      // Subscribe to the mouse move
      Application.Current.RootVisual.MouseMove += Application_MouseMove;
    }

    #endregion

    #region Timer

    /// <summary>
    /// Stops the timer
    /// </summary>
    private void StopLostFocusTimer()
    {
      var currentTimer = _lostFocusTimer;

      // Reset the member
      _lostFocusTimer = null;

      // Dispose of the timer
      if (currentTimer != null)
      {
        currentTimer.Dispose();
      }
    }

    /// <summary>
    /// Starts the lost focus timer
    /// </summary>
    private void StartLostFocusTimer()
    {
      if (_lostFocusTimer == null)
      {
        _lostFocusTimer = new Timer((o) =>
        {
          // Stop the timer
          StopLostFocusTimer();

          // Deactivate the menu
          UIDispatcher.BeginInvoke(() => this.IsActive = false);

        }, null, 750, Timeout.Infinite);
      }
    }

    #endregion

    #region Callbacks

    /// <summary>
    /// Callback when the mouse moves in the application, close the menu if required
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Application_MouseMove(object sender, MouseEventArgs e)
    {
      if (IsActive && _contentElement != null)
      {
        var contentOffsetLeft = HitTestMargin.Left;
        var contentOffsetTop = HitTestMargin.Top;
        var contentOffsetRight = HitTestMargin.Right;
        var contentOffsetBottom = HitTestMargin.Bottom;

        var contentMousePoint = e.GetPosition(_contentElement);
        var contentRect = new Rect(-contentOffsetLeft, -contentOffsetTop, _contentElement.ActualWidth + contentOffsetLeft + contentOffsetRight, _contentElement.ActualHeight + contentOffsetTop + contentOffsetBottom);
        var inside = contentRect.Contains(contentMousePoint);

        if (!inside && _activatorControl != null)
        {
          var activatorMousePoint = e.GetPosition(_activatorControl);
          var activatorOffset = Math.Max(_activatorControl.ActualWidth, _activatorControl.ActualHeight) * 0.2;
          var activatorRect = new Rect(-activatorOffset, -activatorOffset, _activatorControl.ActualWidth + (2 * activatorOffset), _activatorControl.ActualHeight + (2 * activatorOffset));
          inside = activatorRect.Contains(activatorMousePoint);
        }

        if (!inside)
        {
          // Start the focustimer for a delayed close
          StartLostFocusTimer();
        }
        else
        {
          // Reset the timer
          StopLostFocusTimer();
        }
      }
    }

    /// <summary>
    /// Callback when one of the dependency properties changes
    /// </summary>
    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (LiteSubMapBar)d;
      if (e.Property == IsActiveProperty)
      {
        // Stop the focus timer if this property changes
        control.StopLostFocusTimer();

        // Propagate the event
        if (control.IsActiveChanged != null)
        {
          control.IsActiveChanged.Invoke(control, e);
        }

        // Reposition the bar to its correct location and set the correct state
        control.RepositionToActivationControl();
        control.ChangeVisualState();
      }
    }

    /// <summary>
    /// Callback when the main mapbar size changes
    /// </summary>
    void MapBar_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      // Make sure the bar is at the right location
      RepositionToActivationControl();
    }

    /// <summary>
    /// Callback when the layout of the activator control has changed
    /// </summary>
    void ActivatorControl_LayoutUpdated(object sender, EventArgs e)
    {
      SetupActivatorControlPosition();
    }

    /// <summary>
    /// Callback when the size of the activator control has changed
    /// </summary>
    void ActivatorControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      SetupActivatorControlPosition();
    }
    #endregion

    #region Activation Control

    /// <summary>
    /// Setup the activator control position relative to the host
    /// </summary>
    private void SetupActivatorControlPosition()
    {
      if (_activatorControl != null)
      {
        var trans = _activatorControl.TransformToVisual(Application.Current.RootVisual);
        ActivatorControlCenterPosition = trans.Transform(new Point(_activatorControl.ActualWidth / 2.0, _activatorControl.ActualHeight / 2.0));
      }
    }

    /// <summary>
    /// Setup the activation controls of this bar
    /// </summary>
    private void SetupActivationControl()
    {
      if (!String.IsNullOrWhiteSpace(ActivatorControlName))
      {
        // Get the main mapbar
        _mapBar = this.GetAntecedent<LiteMapBar>();

        if (_mapBar != null)
        {
          _mapBar.SizeChanged += MapBar_SizeChanged;

          // Get the activator control
          _activatorControl = _mapBar.GetDescendant<FrameworkElement>(ActivatorControlName);

          if (_activatorControl != null)
          {
            SetupActivatorControlPosition();
            _activatorControl.LayoutUpdated += ActivatorControl_LayoutUpdated;
            _activatorControl.SizeChanged += ActivatorControl_SizeChanged;
          }
        }
      }
    }

    /// <summary>
    /// Reposition the bar relative to the activators
    /// </summary>
    private void RepositionToActivationControl()
    {
      if (_activatorControl != null && _mapBar != null)
      {
        // Offset for aligning centers
        var activaterOffset = _activatorControl.ActualWidth / 2.0;
        var contentOffset = _contentElement.ActualWidth / 2.0;
        // Content metrics
        var contentLeft = Canvas.GetLeft(_contentElement);
        var contentWidth = _contentElement.ActualWidth;
        var parentWidth = _mapBar.ActualWidth;

        // Calculate new position
        var relX = _activatorControl.TransformToVisual(this).Transform(new Point()).X + activaterOffset;
        var cX = relX - contentOffset;

        // Check bounds and adjust position of required
        var parentOffset = parentWidth - (cX + contentWidth);
        if (parentOffset < 0)
        {
          cX += parentOffset;
        }

        if (cX < 0)
        {
          cX = 0;
        }

        Canvas.SetLeft(_contentElement, cX);

        // Invalidate the content element if it might be dependent of the location
        if (_contentElement != null)
        {
          _contentElement.InvalidateArrange();
        }
      }
    }
    #endregion

    #region State Transition

    /// <summary>
    /// Change the visual state of the control
    /// </summary>
    private void ChangeVisualState()
    {
      Storyboard toState = IsActive ? _activationAnimation : _deactivationAnimation;
      Storyboard fromState = !IsActive ? _activationAnimation : _deactivationAnimation;

      if (toState != null)
      {
        toState.Begin();

        // If the template is not loaded, then do not wait for the animation to finish
        if (!_templateLoaded)
        {
          toState.SkipToFill();
        }
      }

      // Stop the from animation
      if (fromState != null)
      {
        fromState.Stop();
      }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Get or set the active state of the control
    /// </summary>
    public Boolean IsActive
    {
      get { return (Boolean)GetValue(IsActiveProperty); }
      set
      {
        SetValue(IsActiveProperty, value);
      }
    }

    /// <summary>
    /// Get or set the hittest margin of the content control
    /// </summary>
    public Thickness HitTestMargin
    {
      get { return (Thickness)GetValue(HitTestMarginProperty); }
      set
      {
        SetValue(HitTestMarginProperty, value);
      }
    }

    /// <summary>
    /// Get or set the control name that activates this control
    /// </summary>
    public String ActivatorControlName
    {
      get { return (String)GetValue(ActivatorControlNameProperty); }
      set
      {
        SetValue(ActivatorControlNameProperty, value);
      }
    }

    /// <summary>
    /// Gets or set the activator control center position property in screen coordinates
    /// </summary>
    public Point ActivatorControlCenterPosition
    {
      get { return (Point)GetValue(ActivatorControlCenterPositionProperty); }
      private set
      {
        SetValue(ActivatorControlCenterPositionProperty, value);
      }
    }
    #endregion
  }
}
