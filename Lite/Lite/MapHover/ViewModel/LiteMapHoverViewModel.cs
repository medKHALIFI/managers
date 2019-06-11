using System.Collections.Generic;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// A view model that responds to HoveredFeatureGeometry property changes of
  /// a MapView
  /// </summary>
  public class LiteMapHoverViewModel : MapFeatureGeometryNotificationViewModel
  {
    #region Constructor
    /// <summary>
    /// Constructs the LiteMapHoverViewModel, by reacting to HoveredFeatureGeometry
    /// notifications.
    /// </summary>
    public LiteMapHoverViewModel(Messenger messenger = null)
      : base(MapViewModel.MouseHoverFeatureGeometryPropertyName)
    { }
    #endregion

    #region Implementation
    /// <summary>
    /// Returns the fields to be displayed for the specified feature and hovered geometry.
    /// Should return null in case no information should be displayed for this table
    /// </summary>
    /// <param name="feature">The feature to return fields for</param>
    /// <param name="geometry">The geometry being hovered</param>
    /// <returns>The fields to display information for</returns>
    protected override IEnumerable<FeatureFieldDescriptor> FieldsFor(Feature feature, IFeatureGeometry geometry)
    {
      // Get the table descriptor
      var tableDescriptor = feature.TableDescriptor;

      // Get all fields of the table descriptor
      var fields = tableDescriptor.FieldDescriptors;

      // filter the fields to only include visible alphanumeric fields
      return fields.FindAll(a => a.FieldDescriptorType == FeatureFieldDescriptorType.Alpha && a.IsVisible);
    }
    #endregion
  }
}
