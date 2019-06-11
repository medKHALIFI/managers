using SpatialEye.Framework.Client;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Redlining;

namespace Lite
{
  /// <summary>
  /// A change in trail model
  /// </summary>
  public class LiteMapTrailModelChangedMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Constructs the trail model change
    /// </summary>
    public LiteMapTrailModelChangedMessage(object sender, GeometryModel<RedliningElement> model, GeometryModel<RedliningElement>.ContentsChangedEventArgs change)
      : base(sender)
    {
      Model = model;
      Change = change;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The elements of the Trail model
    /// </summary>
    public GeometryModel<RedliningElement> Model
    {
      get;
      private set;
    }

    /// <summary>
    /// The change that led up to the new model
    /// </summary>
    public GeometryModel<RedliningElement>.ContentsChangedEventArgs Change
    {
      get;
      private set;
    }
    #endregion
  }
}
