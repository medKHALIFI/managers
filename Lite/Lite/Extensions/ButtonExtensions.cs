using System;
using System.Windows;

namespace Lite
{
  /// <summary>
  /// Extends the button class with attached properties
  /// </summary>
  public class ButtonExtensions
  {
    #region Text Property
    /// <summary>
    /// Text property that can hold addition text for the button
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(String), typeof(ButtonExtensions), null);

    /// <summary>
    /// Getter
    /// </summary>
    public static String GetText(DependencyObject obj)
    {
      return obj.GetValue(TextProperty) as String;
    }

    /// <summary>
    /// Setter
    /// </summary>
    public static void SetText(DependencyObject obj, String value)
    {
      obj.SetValue(TextProperty, value);
    }
    #endregion
  }
}
