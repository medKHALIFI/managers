using System.Linq;
using System.Collections.Generic;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

using glf = SpatialEye.Framework.Features.Expressions.GeoLinqExpressionFactory;

namespace Lite
{
  /// <summary>
  /// The collection restriction view model that is responsible for restricting collections' content
  /// before using them. This viewModel is a template that can be filled out further. The viewModel
  /// is loosely coupled (via messenger requests) and other viewModels use its restriction capabilities
  /// by just placing a LiteRestrictFeatureCollectionRequestMessage request on the messenger.
  /// </summary>
  public class LiteFeatureCollectionRestrictionViewModel : ViewModelBase
  {
    #region Helper class for easy of reading
    /// <summary>
    /// The table field restriction class
    /// </summary>
    public class TableFieldRestrictions : Dictionary<string, string> { }
    #endregion

    #region Fields
    /// <summary>
    /// Holds the content to restrict by area
    /// </summary>
    private Dictionary<string, string> _tableFieldNamesToRestrictByArea;
    #endregion

    #region Constructor
    /// <summary>
    /// Restricts the collection to be displayed
    /// </summary>
    public LiteFeatureCollectionRestrictionViewModel(Messenger messenger, bool includeDefaultRestrictions, Dictionary<string, string> tableFieldRestrictions = null)
      : base(messenger)
    {
      // Set the properties
      IncludeDefaultRestrictions = includeDefaultRestrictions;

      // Set up restriction data
      SetupRestrictionData(tableFieldRestrictions);

      // Attach to the messenger to pick off requests for restricting collections
      AttachToMessenger();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach the result-ViewModel to the messenger to handle display request.
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<LiteRestrictFeatureCollectionRequestMessage>(this, HandleRestrictFeatureCollection);
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Indicates whether default restrictions should apply for those table/fields that have not
    /// been set up explicitly
    /// </summary>
    private bool IncludeDefaultRestrictions
    {
      get;
      set;
    }
    #endregion

    #region Restriction API
    /// <summary>
    /// Sets up the restriction data
    /// </summary>
    private void SetupRestrictionData(Dictionary<string, string> tableFieldRestrictions)
    {
      // Set up the Table/Field combinations for area restriction
      _tableFieldNamesToRestrictByArea = new Dictionary<string, string>();

      // Add the ones set up upon construction of the view-model
      foreach (var kv in tableFieldRestrictions)
      {
        if (!_tableFieldNamesToRestrictByArea.ContainsKey(kv.Key))
        {
          _tableFieldNamesToRestrictByArea.Add(kv.Key, kv.Value);
        }
      }
    }

    /// <summary>
    /// Handle the restriction of a feature collection
    /// </summary>
    private void HandleRestrictFeatureCollection(LiteRestrictFeatureCollectionRequestMessage restrictionRequest)
    {
      // The sample behavior is to restrict the collection based on the MapLayerAreas that have been
      // set up for this user (its groups). In case there are restriction areas set up, the extra restriction
      // will be set on geometry fields that are set up in the restriction data collection

      // Restrict the Town collection
      var restrictionGeometry = LiteClientSettingsViewModel.Instance.RestrictionGeometry;
      var sourceCollection = restrictionRequest.RestrictedCollection;

      if (restrictionGeometry != null && sourceCollection != null)
      {
        var table = sourceCollection.TableDescriptor;
        if (table != null)
        {
          string fieldName;
          if (_tableFieldNamesToRestrictByArea.TryGetValue(table.Name, out fieldName))
          {
            // Do explicit restriction via the table/field combination
            var coverageField = table.FieldDescriptors[fieldName];
            if (coverageField != null && coverageField.IsGeometry)
            {
              var extraRestriction = glf.Spatial.Interacts(glf.Data.Field(coverageField), glf.Constant(restrictionGeometry));
              restrictionRequest.RestrictedCollection = sourceCollection.Where(extraRestriction);
            }
          }
          else if (IncludeDefaultRestrictions)
          {
            // Try to restrict by investigating the geometry fields of the collection
            // If there is a GeoGraphic field, let's try and restrict the lot according to our areas
            if (table.FieldDescriptors.FirstOrDefault(f =>
              {
                var geometryField = f as FeatureGeometryFieldDescriptor;
                return geometryField != null && geometryField.IsSelectableGeometry && geometryField.IsGeographic;
              }) != null)
            {
              var extraRestriction = glf.Function(SpatialEye.Framework.Features.Expressions.GeoLinqExpressionType.SpatialAnyInteracts, glf.Data.Table(table), glf.Constant(restrictionGeometry));
              restrictionRequest.RestrictedCollection = sourceCollection.Where(extraRestriction);
            }
          }
        }
      }
    }
    #endregion

  }
}
