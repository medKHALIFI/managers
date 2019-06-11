using Microsoft.Practices.ServiceLocation;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Geometry.Services;
using SpatialEye.Framework.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
  /// The main page class holding the Single Page UI for the Lite Application
  /// </summary>
  public partial class MainPage : UserControl
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public MainPage()
    {
      InitializeComponent();
    }

    /// <summary>
    /// Handler for RMC events, making sure that the 'Silverlight' Popup isn't shown
    /// </summary>
    private void LayoutRoot_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    /// <summary>
    /// Handle the keydown event
    /// </summary>T
    private void LayoutRoot_KeyDown(object sender, KeyEventArgs e)
    {
      var locator = System.Windows.Application.Current.Resources["Locator"] as ViewModelLocator;
      if (locator != null)
      {
        if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
        {
          locator.TraceLogger.Enabled = !locator.TraceLogger.Enabled;
          e.Handled = true;
        }
        if (e.Key == Key.Q && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
        {
          var map = locator.MapsViewModel.CurrentMap;
          var vs = map.ViewScale;
          var layers = map.Layers;
          var result = new MapLayerChangeCollection();

          foreach (var layer in layers)
          {
            result.Add(new MapLayerChange(layer.Name, vs / 10, vs * 10, new[] { map.Envelope.EnlargedBy(2) }));
          }

          map.ProcessMapLayerChanges(result);
        }

      }
    }
  }
}
