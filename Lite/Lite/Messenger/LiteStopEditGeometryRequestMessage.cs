using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A request to stop editing geometry
  /// </summary>
  public class LiteStopEditGeometryRequestMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteStopEditGeometryRequestMessage(object sender, FeatureGeometryFieldDescriptor fieldDescriptor = null)
      : base(sender)
    {
      FieldDescriptor = fieldDescriptor;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The field descriptor we are stopping editing for
    /// </summary>
    public FeatureGeometryFieldDescriptor FieldDescriptor
    {
      get;
      private set;
    }
    #endregion
  }
}
