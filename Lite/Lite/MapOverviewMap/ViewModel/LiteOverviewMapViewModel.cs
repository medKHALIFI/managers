using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The viewModel holding the logic of the overview map. This overview
  /// map is capable of tracking a source map (it picks off of the messenger).
  /// </summary>
  public class LiteOverviewMapViewModel : OverviewMapViewModel
  {
    #region Constructors
    /// <summary>
    /// Default constructor for the overviewMap view model
    /// </summary>
    public LiteOverviewMapViewModel(Messenger messenger = null)
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
      this.Messenger.Register<PropertyChangedMessage<LiteMapViewModel>>(this, HandleMapViewModelChange);
    }

    /// <summary>
    /// Handles the mapViewModel change
    /// </summary>
    private void HandleMapViewModelChange(PropertyChangedMessage<LiteMapViewModel> mapViewModel)
    {
      // Set the current map view to be used
      this.SourceMap = mapViewModel.NewValue;
    }
    #endregion
  }
}
