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
  /// Workaround for fixing the Silverlights caret brush problem.
  /// If a custom caret brush is set on a textbox or passwordbox then the caret disapears.
  /// Use this behavior to workaround this problem
  /// </summary>
  public class FixCaretBrushBehavior
  {
    /// <summary>
    /// The caret brush property
    /// </summary>
    public static readonly DependencyProperty CaretBrushProperty = DependencyProperty.RegisterAttached("CaretBrush", typeof(Brush), typeof(FixCaretBrushBehavior), new PropertyMetadata(CaretBrushChanged));

    /// <summary>
    /// When the caret brush has changed, make sure we act
    /// </summary>
    private static void CaretBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var ctrl = d as Control;
      if (ctrl != null)
      {
        var oldValue = (Brush)e.OldValue;
        var newValue = (Brush)e.NewValue;

        if (oldValue != newValue)
        {
          if (oldValue == null)
          {
            ctrl.GotFocus += OnControlFocus;
          }

          if (newValue == null)
          {
            ctrl.GotFocus -= OnControlFocus;
          }
        }
      }
    }

    /// <summary>
    /// When control has received focus, update the caret brush
    /// </summary>
    private static void OnControlFocus(object sender, RoutedEventArgs e)
    {
      var txt = sender as TextBox;
      if (txt != null)
      {
        txt.CaretBrush = GetCaretBrush(txt);
        return;
      }

      var pwd = sender as PasswordBox;
      if (pwd != null)
      {
        pwd.CaretBrush = GetCaretBrush(pwd);
      }
    }

    /// <summary>
    /// Get the caret brush
    /// </summary>
    public static Brush GetCaretBrush(DependencyObject obj)
    {
      return (Brush)obj.GetValue(CaretBrushProperty);
    }

    /// <summary>
    /// Set the caret's brush
    /// </summary>
    public static void SetCaretBrush(DependencyObject obj, Brush value)
    {
      obj.SetValue(CaretBrushProperty, value);
    }

  }
}

