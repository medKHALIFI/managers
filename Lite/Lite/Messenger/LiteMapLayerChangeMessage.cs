using SpatialEye.Framework.Client;
using SpatialEye.Framework.Maps;

namespace Lite
{
  /// <summary>
  /// The message that is sent whenever the server pushes a change in map layer content
  /// </summary>
  public class LiteMapLayerChangeMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Constructs the message for the specified changes
    /// </summary>
    public LiteMapLayerChangeMessage(object sender, MapLayerChangeCollection mapLayerChanges)
      : base(sender)
    {
      Changes = mapLayerChanges;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The changes that were pushed to the client to indicate server side
    /// changes in the contents of a map layer. 
    /// </summary>
    public MapLayerChangeCollection Changes
    {
      get;
      private set;
    }
    #endregion

  }
}
