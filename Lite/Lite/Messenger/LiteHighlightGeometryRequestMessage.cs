using System;
using System.Collections.Generic;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// Request for jumping to an extent (envelope) on the active map
  /// </summary>
  public class LiteHighlightGeometryRequestMessage : MessageBase
  {
    #region Static Table
    /// <summary>
    /// Creates a default table descriptor for one geometry of the specified geometry type
    /// </summary>
    private static FeatureTableDescriptor GeometryTableDescriptor(FeatureGeometryType type)
    {
      var table = new FeatureTableDescriptor("", "");
      var field = table.FieldDescriptors.Add("", "", type);
      return table;
    }
    #endregion

    /// <summary>
    /// Constructs the request to highlight a specified geometry on the active map 
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="targetGeometry">The geometry to highlight on the map</param>
    public LiteHighlightGeometryRequestMessage(Object sender, Feature feature)
      : base(sender)
    {
      this.Feature = feature;
    }

    /// <summary>
    /// Constructs the request to highlight a specified geometry on the active map 
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="targetGeometry">The geometry to highlight on the map</param>
    public LiteHighlightGeometryRequestMessage(Object sender, FeatureTargetGeometry targetGeometry)
      : base(sender)
    {
      this.FeatureTargetGeometry = targetGeometry;
    }

    /// <summary>
    /// Constructs the request to highlight a specified geometry on the active map 
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="targetGeometry">The geometry to highlight on the map</param>
    public LiteHighlightGeometryRequestMessage(Object sender, IFeatureGeometry geometry)
      : base(sender)
    {
      if (geometry == null)
      {
        throw new ArgumentException("Invalid geometry");
      }

      // Create a feature for the geometry
      var feature = new Feature(GeometryTableDescriptor(geometry.GeometryType), new object[] { geometry });

      // Create a feature target geometry for this feature, with the specified geometry selected
      FeatureTargetGeometry = new FeatureTargetGeometry(feature, 0);
    }

    public Feature Feature
    {
      get;
      private set;
    }

    /// <summary>
    /// The geometry to highlight on the map
    /// </summary>
    public FeatureTargetGeometry FeatureTargetGeometry
    {
      get;
      private set;
    }

    /// <summary>
    /// The geometry to highlight on the map
    /// </summary>
    public IList<FeatureTargetGeometry> FeatureTargetGeometryFor(World world)
    {
      if (FeatureTargetGeometry != null && FeatureTargetGeometry.TargetGeometry != null)
      {
        var geometryWorld = FeatureTargetGeometry.TargetGeometry.World;
        if (geometryWorld != null && geometryWorld.Equals(world))
        {
          return new List<FeatureTargetGeometry>() { this.FeatureTargetGeometry };
        }
      }

      if (Feature != null)
      {
        var result = new List<FeatureTargetGeometry>();

        foreach (FeatureGeometryFieldDescriptor geometryField in Feature.TableDescriptor.FieldDescriptors.Descriptors(FeatureFieldDescriptorType.Geometry))
        {
          var geometry = Feature[geometryField.Name] as IFeatureGeometry;

          if (geometry != null)
          {
            var geometryType = geometry.GeometryType;

            // Only allow implicit highlighting of structured geometry
            var okType = geometryType.IsPointOrMultiPoint || geometryType.IsCurveOrMultiCurve || geometryType.IsPolygonOrMultiPolygon;

            if (okType)
            {
              var geometryWorld = geometry.World;
              if (geometryWorld != null && geometryWorld.Equals(world))
              {
                result.Add(new FeatureTargetGeometry(this.Feature, geometryField, geometry));
              }
            }
          }
        }

        return result;
      }

      return null;
    }
  }
}
