using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A change in map selection
  /// </summary>
  public class LiteMapSelectionChangedMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Constructs the map selection change message
    /// </summary>
    public LiteMapSelectionChangedMessage(object sender, Collection<Feature> selectedFeatures, Collection<FeatureTargetGeometry> selectedFeatureGeometry)
      : base(sender)
    {
      SelectedFeatures = selectedFeatures;
      SelectedFeatureGeometry = selectedFeatureGeometry;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The selected features
    /// </summary>
    public Collection<Feature> SelectedFeatures
    {
      get;
      private set;
    }

    /// <summary>
    /// The selected feature geometry
    /// </summary>
    public Collection<FeatureTargetGeometry> SelectedFeatureGeometry
    {
      get;
      private set;
    }

    /// <summary>
    /// Is this a single selection
    /// </summary>
    public bool IsSingleSelection
    {
      get { return SelectedFeatureGeometry != null && SelectedFeatureGeometry.Count == 1; }
    }

    /// <summary>
    /// Returns the single selecton's feature
    /// </summary>
    public Feature SingleFeature
    {
      get { return IsSingleSelection ? SelectedFeatures[0] : null; }
    }

    /// <summary>
    /// Returns the single selection
    /// </summary>
    public FeatureTargetGeometry SingleFeatureGeometry
    {
      get { return IsSingleSelection ? SelectedFeatureGeometry[0] : null; }
    }

    /// <summary>
    /// Gets the selection of the specified type
    /// </summary>
    public IList<FeatureTargetGeometry> SelectionOfType(params FeatureGeometryType[] geometryTypes)
    {
      var result = new List<FeatureTargetGeometry>();

      if (SelectedFeatureGeometry != null)
      {
        result.AddRange(SelectedFeatureGeometry.Where(fg => fg.TargetGeometry != null && geometryTypes.Contains(fg.TargetGeometry.GeometryType)));
      }

      return result;
    }
    #endregion
  }
}
