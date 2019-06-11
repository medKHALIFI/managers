using System;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// Request for jumping to a Geometry (or an Envelope)
  /// </summary>
  public class LiteGoToGeometryRequestMessage : MessageBase
  {
    #region Type Initialization
    /// <summary>
    /// Type initializer, setting up the featureTargetGeometry table descriptor that
    /// is used in case there is an GoTo Envelope request. This table is used to
    /// morph the Envelope request into a FeatureTargetGeometry request where the
    /// feature holds a description and the centre of the envelope.
    /// </summary>
    static LiteGoToGeometryRequestMessage()
    {
      FeatureTargetGeometryTable = new SimpleFeatureTableDescriptor("highlight", "Highlight");
      FeatureTargetGeometryTable.FieldDescriptors.Add("description", "Description", FeatureAlphaType.StringWith(255));
      FeatureTargetGeometryTable.FieldDescriptors.Add("location", "Location", FeatureGeometryType.Point);
    }

    /// <summary>
    /// The table descriptor to be used for describing highlight features to jump to
    /// </summary>
    private static FeatureTableDescriptor FeatureTargetGeometryTable { get; set; }

    /// <summary>
    /// Creates a Highlight feature for the specified description and envelope
    /// </summary>
    private static FeatureTargetGeometry FeatureTargetGeometryFor(string description, Envelope envelope)
    {
      var factory = new GeometryFactory(envelope.CoordinateSystem, envelope.World);
      var values = new object[] { description, factory.NewPoint(envelope.Centre) };
      var feature = new Feature(FeatureTargetGeometryTable, values);
      return new FeatureTargetGeometry(feature, 1);
    }

    /// <summary>
    /// Returns a description for the specified feature to be used for display purposes
    /// </summary>
    private static string DescriptionFor(Feature feature)
    {
      string description = string.Empty;
      if (feature != null)
      {
        var table = feature.TableDescriptor;
        if (table != null)
        {
          description = string.Format("{0} {1}", table.ExternalName, feature.Description);
        }
        else
        {
          description = feature.Description;
        }
      }
      return description;
    }
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the request to jump to specified envelope of the feature 
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="envelope">The extent to jump to</param>
    /// <param name="feature">The feature that the extent belongs to</param>
    public LiteGoToGeometryRequestMessage(Object sender, FeatureTargetGeometry targetGeometry)
      : base(sender)
    {
      this.Description = DescriptionFor(targetGeometry.Feature);
      this.Envelope = targetGeometry.Envelope;
      this.StoreInHistory = false;
      this.DoRocketJump = true;
      this.DoHighlight = false;
      this.HighlightFeatureTargetGeometry = targetGeometry;
    }

    /// <summary>
    /// Constructs the request to jump to specified envelope of the feature 
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="envelope">The extent to jump to</param>
    /// <param name="feature">The feature that the extent belongs to</param>
    public LiteGoToGeometryRequestMessage(Object sender, Envelope envelope, Feature feature)
      : base(sender)
    {
      this.Description = DescriptionFor(feature);
      this.Envelope = envelope;
      this.StoreInHistory = false;
      this.DoRocketJump = true;
      this.DoHighlight = false;
      this.HighlightFeatureTargetGeometry = FeatureTargetGeometryFor(Description, Envelope);
    }

    /// <summary>
    /// Constructs the request to jump to the specified envelope, without an owning feature
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="envelope">The extent to jump to</param>
    /// <param name="description">The description to store for this request</param>
    public LiteGoToGeometryRequestMessage(Object sender, Envelope envelope, string description = "")
      : base(sender)
    {
      this.Description = description;
      this.Envelope = envelope;
      this.StoreInHistory = false;
      this.DoRocketJump = true;
      this.DoHighlight = false;
      this.HighlightFeatureTargetGeometry = FeatureTargetGeometryFor(Description, Envelope);
    }
    #endregion

    #region Properties

    /// <summary>
    /// The description of (the source of) the jump extent
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// The extent to jump to
    /// </summary>
    public Envelope Envelope { get; private set; }

    /// <summary>
    /// Indicates whether the jump should be stored in history
    /// </summary>
    public bool StoreInHistory { get; set; }

    /// <summary>
    /// Indicates whether to do a rocket jump; default value is true but
    /// can be set separately from the constructor
    /// </summary>
    public bool DoRocketJump { get; set; }

    /// <summary>
    /// Do we automatically want to highlight the geometry
    /// </summary>
    public bool DoHighlight { get; set; }

    /// <summary>
    /// The feature target geometry to use for highlighting on the map
    /// </summary>
    public FeatureTargetGeometry HighlightFeatureTargetGeometry { get; private set; }

    /// <summary>
    /// Returns the request as a Highlight request
    /// </summary>
    public LiteHighlightGeometryRequestMessage ToHighlightRequest()
    {
      return new LiteHighlightGeometryRequestMessage(this.Sender, this.HighlightFeatureTargetGeometry);
    }

    /// <summary>
    /// Returns the description of the jump request
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return Description;
    }
    #endregion
  }
}
