using System;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  public class LiteNewFeatureRequestMessage : MessageBase
  {
    /// <summary>
    /// Constructs the request
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="feature">The feature (recipe-holder) to show details for</param>
    public LiteNewFeatureRequestMessage(Object sender, FeatureTableDescriptor tableDescriptor, FeatureGeometryFieldDescriptor geomDescriptor, IFeatureGeometry startWithGeometry = null, Feature attachToFeature = null)
      : base(sender)
    {
      this.TableDescriptor = tableDescriptor;
      this.GeometryDescriptor = geomDescriptor;

      if (tableDescriptor.EditabilityProperties.AllowInsert)
      {
        var newFeature = tableDescriptor.NewTemplateFeature();

        if (geomDescriptor != null && startWithGeometry != null)
        {
          StartedWithTrail = true;
          newFeature[geomDescriptor.Name] = startWithGeometry;
        }

        Feature = new EditableFeature(newFeature, false, attachToFeature);
      }
    }

    /// <summary>
    /// The editable feature
    /// </summary>
    public EditableFeature Feature
    {
      get;
      private set;
    }

    /// <summary>
    /// The table descriptor to create the feature for
    /// </summary>
    private FeatureTableDescriptor TableDescriptor
    {
      get;
      set;
    }

    /// <summary>
    /// The primary geometry descriptor for the feature to insert
    /// </summary>
    public FeatureGeometryFieldDescriptor GeometryDescriptor
    {
      get;
      private set;
    }

    /// <summary>
    /// Have we started with the trail
    /// </summary>
    public bool StartedWithTrail
    {
      get;
      private set;
    }
  }
}
