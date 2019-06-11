using System.Collections.Generic;

using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Units;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// A helper class for the Feature Details viewer, to wrap multiple features
  /// into one feature for display in the feature details view
  /// </summary>
  public class LiteFeatureDetailFeatureDetailsProvider
  {
    #region Constructor
    /// <summary>
    /// The constructor for the detailsProvider for multiple features
    /// </summary>
    public LiteFeatureDetailFeatureDetailsProvider()
    {
      SetupTableDescriptor();
    }
    #endregion

    #region TableDescriptor
    /// <summary>
    /// Set up the table descriptor
    /// </summary>
    private void SetupTableDescriptor()
    {
      var tableName = "_multiplefeatures_";
      var tableExternalName = ApplicationResources.FeatureDetailsMultipleFeatures;
      var elementsFieldName = "elements";
      var elementsFieldExternalName = ApplicationResources.FeatureDetailsMultipleFeaturesNumberOfElements;
      var areaFieldName = "area";
      var areaFieldExternalName = ApplicationResources.FeatureDetailsMultipleFeaturesArea;
      var lengthFieldName = "length";
      var lengthFieldExternalName = ApplicationResources.FeatureDetailsMultipleFeaturesLength;

      var tableDescriptor = new SimpleFeatureTableDescriptor(tableName, tableExternalName);
      tableDescriptor.FieldDescriptors.Add(elementsFieldName, elementsFieldExternalName, FeatureAlphaType.Int);
      tableDescriptor.FieldDescriptors.Add(lengthFieldName, areaFieldExternalName, FeatureAlphaType.StringWith(30));
      tableDescriptor.FieldDescriptors.Add(areaFieldName, lengthFieldExternalName, FeatureAlphaType.StringWith(30));

      this.MultiFeatureTableDescriptor = tableDescriptor;
    }

    /// <summary>
    /// The table descriptor for displaying details of multiple features
    /// </summary>
    private FeatureTableDescriptor MultiFeatureTableDescriptor
    {
      get;
      set;
    }
    #endregion

    #region Feature Details
    /// <summary>
    /// Determines the area and length details for the specified features
    /// </summary>
    private void DetermineAreaAndLengthDetailsFor(IList<Feature> features, out string areaString, out string lengthString)
    {
      var length = 0.0;
      var area = 0.0;
      foreach (var feature in features)
      {
        foreach (FeatureGeometryFieldDescriptor field in feature.TableDescriptor.FieldDescriptors.Descriptors(FeatureFieldDescriptorType.Geometry))
        {
          var geometry = feature[field.Name];
          if (geometry != null)
          {
            switch (field.FieldType.PhysicalType)
            {
              case FeaturePhysicalFieldType.Curve:
              case FeaturePhysicalFieldType.MultiCurve:
                ICurveGeometry curveGeometry = geometry as ICurveGeometry;
                length += curveGeometry.LineLength(LinearLineLengthType.Meter);
                break;

              case FeaturePhysicalFieldType.Polygon:
              case FeaturePhysicalFieldType.MultiPolygon:
                IPolygonGeometry polygonGeometry = geometry as IPolygonGeometry;
                area += polygonGeometry.Area(SurfaceAreaType.MeterSquared);
                break;
            }
          }
        }
      }

      // Interpret the area in m2, but let the unitSystem determine the actual locale's settings
      areaString = UnitSystem.Convert(area, "m2").ToString();

      // Interpret the length in m, but let the unitSystem determine the display units
      lengthString = UnitSystem.Convert(length, "m").ToString();
    }

    /// <summary>
    /// Returns a feature to display for the specified set of features
    /// </summary>
    public Feature DetailsFor(IList<Feature> features)
    {
      Feature result = null;
      if (features != null)
      {
        var length = features.Count;
        if (length == 1)
        {
          // A single feature needs to be displayed
          result = features[0];

          if (result != null)
          {
            // Make sure we display the homogeneous representation of this
            result = result.ToHomogeneous();
          }
        }
        else if (length > 1)
        {
          string areaString, lengthString;
          DetermineAreaAndLengthDetailsFor(features, out areaString, out lengthString);

          // Multiple features need to be displayed as one
          result = new Feature(this.MultiFeatureTableDescriptor, new object[3] { length, areaString, lengthString });
        }
      }

      return result;
    }
    #endregion
  }
}
