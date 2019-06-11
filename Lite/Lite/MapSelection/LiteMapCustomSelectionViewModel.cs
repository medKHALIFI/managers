using System.Threading.Tasks;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Features.Expressions;
using SpatialEye.Framework.Features.Services;
using SpatialEye.Framework.Geometry;

using glf = SpatialEye.Framework.Features.Expressions.GeoLinqExpressionFactory;

namespace Lite
{
  /// <summary>
  /// The custom selection view model that takes care of selection in case the user 
  /// has clicked on the Map but nothing was selected
  /// </summary>
  public class LiteMapCustomSelectionViewModel : ViewModelBase
  {
    #region Fields
    /// <summary>
    /// The table descriptor
    /// </summary>
    private FeatureTableDescriptor _tableDescriptor;

    /// <summary>
    /// Did we try retrieving the table yet
    /// </summary>
    private bool _triedRetrievingTables;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor
    /// </summary>
    public LiteMapCustomSelectionViewModel(Messenger messenger = null)
      : base(messenger)
    {
      if (!IsInDesignMode)
      {
        // Only attach to the messenger in design mode
        AttachToMessenger();
      }

      // Set a default max distance
      MaxDistance = 100.0;
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<LiteCustomSelectionRequestMessage>(this, HandleCustomSelectionRequest);
      }
    }
    #endregion

    #region TableDescriptor handling
    /// <summary>
    /// Returns the cached table descriptor
    /// </summary>
    private async Task<FeatureTableDescriptor> EnsureTableDescriptorAsync()
    {
      if (_tableDescriptor == null && !_triedRetrievingTables && TableName != null && FieldName != null)
      {
        // Get the table descriptor, since it was not received before
        var tables = await GetService<ICollectionService>().GetTableDescriptorsAsync(TableName);

        // We've tried receiving it (in case it's not there; don't keep on trying)
        _triedRetrievingTables = true;

        if (tables.Count > 0)
        {
          // Get the appropriate table, but only in case there is a corresponding field
          var table = tables[0];
          var field = table.FieldDescriptors[FieldName];
          if (field != null && field.IsGeometry)
          {
            // The table descriptor is fine
            _tableDescriptor = tables[0];
          }
        }
      }

      return _tableDescriptor;
    }

    /// <summary>
    /// Called whenever authentication changes; make sure we stop caching
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      // Reset the table
      _triedRetrievingTables = false;
      _tableDescriptor = null;

      // Get base behavior happening
      base.OnAuthenticationChanged(context, isAuthenticated);
    }
    #endregion

    #region Custom Selection Handling
    /// <summary>
    /// Handler for custom selection request
    /// </summary>
    /// <param name="message">The custom selection request</param>
    private async void HandleCustomSelectionRequest(LiteCustomSelectionRequestMessage message)
    {
      if (message.Args != null)
      {
        // Get the table
        var table = await EnsureTableDescriptorAsync();

        if (table != null)
        {
          // Get the field descriptor
          var field = table.FieldDescriptors[FieldName] as FeatureGeometryFieldDescriptor;

          // Build up the predicate
          var searchLocation = message.Args.Location;

          // CS( 3785 )
          var csExpression = glf.Function(GeoLinqExpressionType.SpatialCS, glf.Constant(searchLocation.CoordinateSystem.EPSGCode));

          // Point( CS(3785), X, Y) 
          var locationExpression = glf.Function( GeoLinqExpressionType.SpatialPoint, csExpression, glf.Constant(searchLocation.Coordinate.X), glf.Constant(searchLocation.Coordinate.Y));

          // <field>.WithinDistance(Point( CS(3785), X, Y), 100.0)
          var predicate = glf.Spatial.WithinDistance(glf.Data.Field(field), locationExpression, MaxDistance);

          // Get the resulting elements (only 100)
          var result = await table.Collection.Where(predicate).Take(100).EvaluateAsync();

          if (result != null && result.Count > 0)
          {
            // Filter by getting the closest only (could use them all if interesting)

            // Initiate vars for getting the closest
            double foundDistance = double.MaxValue;
            Feature foundFeature = null;
            IFeatureGeometry foundGeometry = null;

            foreach (var element in result)
            {
              // Get the geometry and its distance to our search location
              var geometry = element[FieldName] as IFeatureGeometry;
              var testDistance = geometry.DistanceTo(searchLocation);

              // If closer than the last closest, set as current
              if (geometry != null && testDistance < foundDistance)
              {
                foundFeature = element;
                foundDistance = testDistance;
                foundGeometry = geometry;
              }
            }

            // If we've found a feature, select it on the Map that the request took place on
            if (foundFeature != null)
            {
              var target = new FeatureTargetGeometry(foundFeature, field, foundGeometry);

              message.Map.SelectedFeatureGeometry.Set(target);
            }
          }

        }

      }
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// The table name to search
    /// </summary>
    public string TableName
    {
      get;
      set;
    }

    /// <summary>
    /// The field (name) to search
    /// </summary>
    public string FieldName
    {
      get;
      set;
    }

    /// <summary>
    /// The max distance to search in
    /// </summary>
    public double MaxDistance
    {
      get;
      set;
    }
    #endregion
  }
}
