using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// A request to stop editing geometry
  /// </summary>
  public class LiteStopEditFeatureRequestMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteStopEditFeatureRequestMessage(object sender)
      : base(sender)
    { }
    #endregion
  }
}
