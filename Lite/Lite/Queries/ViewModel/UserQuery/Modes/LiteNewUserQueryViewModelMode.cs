using System.Collections.Generic;
using System.Linq.Expressions;

using SpatialEye.Framework.ComponentModel;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Features.Expressions;
using SpatialEye.Framework.Parameters;

namespace Lite
{
  /// <summary>
  /// An abstract base class for viewModels that handle setting up a New Client Query 
  /// in a specific way. The subclasses implement a specific way in doing so.
  /// </summary>
  public abstract class LiteNewUserQueryViewModelMode : BindableObject
  {
    #region Property Names
    /// <summary>
    /// Is the mode enabled
    /// </summary>
    public static string IsEnabledPropertyName = "IsEnabled";
    #endregion

    #region Helpers
    /// <summary>
    /// Returns the queryable geometry fields for the specified table
    /// </summary>
    public static IList<FeatureGeometryFieldDescriptor> GeometryFieldsFor(FeatureTableDescriptor table)
    {
      var result = new List<FeatureGeometryFieldDescriptor>();

      if (table != null)
      {
        foreach (FeatureGeometryFieldDescriptor geometryField in table.FieldDescriptors.Descriptors(FeatureFieldDescriptorType.Geometry, false, false))
        {
          switch (geometryField.FieldType.PhysicalType)
          {
            case FeaturePhysicalFieldType.Point:
            case FeaturePhysicalFieldType.MultiPoint:
            case FeaturePhysicalFieldType.Curve:
            case FeaturePhysicalFieldType.MultiCurve:
            case FeaturePhysicalFieldType.Polygon:
            case FeaturePhysicalFieldType.MultiPolygon:
              result.Add(geometryField);
              break;
          }
        }
      }

      return result;
    }

    /// <summary>
    /// Returns a flag indicating whether there are geometry fields in the specified table
    /// </summary>
    public static bool HasGeometryFieldsFor(FeatureTableDescriptor table)
    {
      var fields = GeometryFieldsFor(table);
      return fields.Count > 0;
    }

    /// <summary>
    /// Returns a geometry target expression for the specified table
    /// </summary>
    public static Expression GeometryTargetExpressionFor(FeatureTableDescriptor table)
    {
      var fields = GeometryFieldsFor(table);
      return (fields.Count == 1) ? (Expression)GeoLinqExpressionFactory.Data.Field(fields[0]) : (Expression)GeoLinqExpressionFactory.Data.Table(table);
    }
    #endregion

    #region Fields
    /// <summary>
    /// The current table descriptor that the mode should allow setting up a query for
    /// </summary>
    private FeatureTableDescriptor _tableDescriptor;

    /// <summary>
    /// Is this mode enabled (dependent on the table descriptor)
    /// </summary>
    private bool _isEnabled;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the mode for the specified description
    /// </summary>
    /// <param name="description">The description of the mode</param>
    internal LiteNewUserQueryViewModelMode(string description)
    {
      Description = description;
    }
    #endregion

    #region Internal API
    /// <summary>
    /// Holds the table descriptor 
    /// </summary>
    internal FeatureTableDescriptor TableDescriptor
    {
      get { return _tableDescriptor; }
      set
      {
        if (_tableDescriptor != value)
        {
          _tableDescriptor = value;

          OnTableDescriptorChanged();
        }
      }
    }

    /// <summary>
    /// The table descriptor has changed
    /// </summary>
    internal abstract void OnTableDescriptorChanged();

    /// <summary>
    /// Returns the expression for the new query
    /// </summary>
    /// <returns></returns>
    internal abstract Expression NewQueryExpression();

    /// <summary>
    /// The parameter definitions that are required for running the query
    /// </summary>
    /// <returns></returns>
    internal abstract ParameterDefinitionCollection NewQueryParameterDefinitions();
    #endregion

    #region Public API
    /// <summary>
    /// Returns the description of the Mode
    /// </summary>
    public string Description
    {
      get;
      private set;
    }

    /// <summary>
    /// Is the element enabled 
    /// </summary>
    public bool IsEnabled
    {
      get { return _isEnabled; }
      protected set
      {
        if (_isEnabled != value)
        {
          _isEnabled = value;
          RaisePropertyChanged(IsEnabledPropertyName);
        }
      }
    }
    #endregion
  }
}
