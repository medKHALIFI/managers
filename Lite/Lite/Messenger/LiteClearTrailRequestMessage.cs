using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// A request to clear the trail
  /// </summary>
  public class LiteClearTrailRequestMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Request to clear the trail, optionally only clearing the trail
    /// when it is selected
    /// </summary>
    public LiteClearTrailRequestMessage(object sender, bool clearSelectedOnly = false)
      : base(sender)
    {
      ClearSelectedOnly = clearSelectedOnly;
    }
    #endregion

    #region Properties
    /// <summary>
    /// A flag that indicates that only the selection needs to be removed from the trail model
    /// </summary>
    public bool ClearSelectedOnly
    {
      get;
      private set;
    }
    #endregion
  }
}
