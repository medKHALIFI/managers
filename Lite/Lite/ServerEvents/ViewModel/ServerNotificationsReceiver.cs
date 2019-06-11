using ServiceStack;

using SpatialEye.Framework.XY.Dtos;

namespace Lite
{
  public class ServerNotificationsReceiver : ServerEventReceiver
  {
    /// <summary>
    /// Pushed changes from the server
    /// </summary>
    public void Changes(MapLayerChangeCollectionDto notification)
    {
      // Do this using the server events view model
      ServerEventsViewModel.Instance.ProcessChanges(notification.ToMapLayerChangeCollection());
    }

    /// <summary>
    /// Dummy method
    /// </summary>
    public override void NoSuchMethod(string selector, object message)
    { }
  }
}
