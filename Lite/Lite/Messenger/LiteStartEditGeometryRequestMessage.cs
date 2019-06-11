using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A request to start editing of a specified geometry
  /// </summary>
  public class LiteStartEditGeometryRequestMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Request editing the geometry
    /// </summary>
    public LiteStartEditGeometryRequestMessage(object sender, EditableFeature feature, FeatureGeometryFieldDescriptor field)
      : base(sender)
    {
      this.Feature = feature;
      this.FieldDescriptor = field;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The contents of the request for editing
    /// </summary>
    public EditableFeature Feature
    {
      get;
      private set;
    }

    /// <summary>
    /// The field descriptor
    /// </summary>
    public FeatureGeometryFieldDescriptor FieldDescriptor
    {
      get;
      private set;
    }
    #endregion
  }
}
