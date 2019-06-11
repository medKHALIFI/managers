using SpatialEye.Framework.Client;
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
  /// A view model that holds the logic for selecting the active mode of 
  /// available backdrop/mode layers in the active map
  /// </summary>
  public class LiteMapBackdropLayerModeSelectionViewModel : MapLayerSelectionViewModel
  {
    #region Constructor
    /// <summary>
    /// Constructs the BackdropLayer Mode selection view model using the specified messenger
    /// </summary>
    public LiteMapBackdropLayerModeSelectionViewModel(Messenger messenger = null)
      : base(messenger)
    {
      if (!IsInDesignMode)
      {
        AttachToMessenger();
      }
    }
    #endregion

    #region MapViewModel Change
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      this.Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this,  HandleMapViewModelChange);
    }

    /// <summary>
    /// Handles the mapViewModel change
    /// </summary>
    private void HandleMapViewModelChange(PropertyChangedMessage<LiteMapViewModel> mapViewModel)
    {
      // Set the current map view to be used, automatically picking up
      // the backdrop layers of interest
      this.MapView = mapViewModel.NewValue;
    }
    #endregion
  }
}
