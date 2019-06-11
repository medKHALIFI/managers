using SpatialEye.Framework.ComponentModel;
using SpatialEye.Framework.Features;
using System;
using System.Threading.Tasks;
using System.Linq;
using SpatialEye.Framework.Features.Editability;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Shapes;

namespace Lite
{
  /// <summary>
  /// A container for an item that can be inserted
  /// </summary>
  public class FeatureInsertItemViewModel : BindableObject
  {
    #region PropertyNames
    /// <summary>
    /// The primary geometry
    /// </summary>
    public const string PrimaryGeometryDescriptorPropertyName = "PrimaryGeometryDescriptor";

    /// <summary>
    /// Is the item enabled
    /// </summary>
    public const string IsEnabledPropertyName = "IsEnabled";
    #endregion

    #region Private Fields
    /// <summary>
    /// A flag indicating whether the table descriptor is being retrieved
    /// </summary>
    private Boolean _retrieving;

    /// <summary>
    /// The geometry descriptor acting as primary field to start the insert action with
    /// </summary>
    private FeatureGeometryFieldDescriptor _geomDescriptor;

    /// <summary>
    /// A flag indicating whether the item is enabled
    /// </summary>
    private bool _isEnabled;
    #endregion

    #region Constructors

    /// <summary>
    /// Constructs the insert item
    /// </summary>
    /// <param name="tableDescriptor">The table descriptor for the item</param>
    /// <param name="isAttachPossible">Indicates whether attach is possible</param>
    /// <param name="isAttachRequired">Indicates whether attaching to an asset is required</param>
    public FeatureInsertItemViewModel(FeatureTableDescriptor tableDescriptor, bool isAttachRequired, bool isAttachPossible)
    {
      TableDescriptor = tableDescriptor;
      Category = tableDescriptor.EditabilityProperties.Category;
      IsAttachRequired = isAttachRequired;
      IsAttachPossible = isAttachPossible;

      var ignored = RetrieveTableDetails();
      CalculateState();
    }
    #endregion

    #region Get Details

    /// <summary>
    /// Get the table details
    /// </summary>
    private async Task RetrieveTableDetails()
    {
      bool evaluated = TableDescriptor.IsEvaluated;
      if (!evaluated && !_retrieving)
      {
        _retrieving = true;
        await TableDescriptor.EvaluateAsync();

        // Setup the geometry descriptor
        DetermineStartWithFieldDescriptor();

        _retrieving = false;
        evaluated = true;
      }
    }

    #endregion

    #region Private Members

    /// <summary>
    /// Determines the geometry to start with
    /// </summary>
    private void DetermineStartWithFieldDescriptor()
    {
      FeatureGeometryFieldDescriptor startWith = null;
      FeatureGeometryFieldDescriptor backstop = null;
      var startDim = -1;
      var backDim = -1;
      foreach (FeatureGeometryFieldDescriptor geometryField in TableDescriptor.FieldDescriptors.Descriptors(FeatureFieldDescriptorType.Geometry))
      {
        var editabilityProperties = geometryField.EditabilityProperties;
        if (geometryField.FieldType.IsStructured && editabilityProperties.AllowUpdate)
        {
          if (editabilityProperties.IsPrimary)
          {
            startWith = geometryField;
            break;
          }

          var newOrder = GeometryOrder(geometryField);
          if (editabilityProperties.IsMandatory)
          {
            // A mandatory field
            if (startWith == null || startDim < newOrder)
            {
              startWith = geometryField;
              startDim = newOrder;
            }
          }
          else if (backstop == null || backDim < newOrder)
          {
            backstop = geometryField;
            backDim = newOrder;
          }
        }
      }

      PrimaryGeometryDescriptor = startWith ?? backstop;
    }

    /// <summary>
    /// Get the dimension of the geometry
    /// </summary>
    /// <param name="geom">the geom to get the dimension for</param>
    /// <returns>dimension of the geometry</returns>
    private int GeometryOrder(FeatureGeometryFieldDescriptor geom)
    {
      var physicalType = geom.FieldType.PhysicalType;
      switch (physicalType)
      {
        case FeaturePhysicalFieldType.Annotation:
        case FeaturePhysicalFieldType.MultiAnnotation:
          return 0;

        case FeaturePhysicalFieldType.Point:
        case FeaturePhysicalFieldType.MultiPoint:
          return 1;

        case FeaturePhysicalFieldType.Curve:
        case FeaturePhysicalFieldType.MultiCurve:
          return 2;

        case FeaturePhysicalFieldType.Polygon:
        case FeaturePhysicalFieldType.MultiPolygon:
          return 3;

        default:
          return 10;
      }
    }
    #endregion

    #region Public Api

    /// <summary>
    /// The name of the item
    /// </summary>
    public String Name
    {
      get { return TableDescriptor.ExternalName; }
    }

    /// <summary>
    /// Gets the tabledescriptor of this item
    /// </summary>
    public FeatureTableDescriptor TableDescriptor
    {
      get;
      private set;
    }

    /// <summary>
    /// Holds the category
    /// </summary>
    public FeatureTableEditabilityProperties.Categories Category
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the primary geometry descriptor
    /// </summary>
    public FeatureGeometryFieldDescriptor PrimaryGeometryDescriptor
    {
      get
      {
        return _geomDescriptor;
      }
      private set
      {
        if (value != _geomDescriptor)
        {
          _geomDescriptor = value;
          RaisePropertyChanged(PrimaryGeometryDescriptorPropertyName);
        }
      }
    }

    /// <summary>
    /// The candidate start geometry
    /// </summary>
    public SpatialEye.Framework.Geometry.IFeatureGeometry CandidateStartWithGeometry
    {
      get;
      set;
    }

    /// <summary>
    /// The name of the path to use to describe this
    /// </summary>
    public string PathName
    {
      get
      {
        if (_geomDescriptor != null)
        {
          switch (_geomDescriptor.FieldType.PhysicalType)
          {
            case FeaturePhysicalFieldType.Point:
            case FeaturePhysicalFieldType.MultiPoint:
              return "MetroIcon.Content.TrailPoint";

            case FeaturePhysicalFieldType.Curve:
            case FeaturePhysicalFieldType.MultiCurve:
              return "MetroIcon.Content.TrailCurve";

            case FeaturePhysicalFieldType.Polygon:
            case FeaturePhysicalFieldType.MultiPolygon:
              return "MetroIcon.Content.TrailPolygon";
          }
        }

        // Default to this icon resource
        return LiteMenuItemViewModel.DefaultIconResource;
      }
    }

    /// <summary>
    /// Is this item enabled
    /// </summary>
    public bool IsEnabled
    {
      get { return _isEnabled; }
      private set
      {
        if (_isEnabled != value)
        {
          _isEnabled = value;
          RaisePropertyChanged(IsEnabledPropertyName);
        }
      }
    }

    /// <summary>
    /// The target geometry
    /// </summary>
    public FeatureTargetGeometry AttachTo
    {
      get;
      private set;
    }

    /// <summary>
    /// Is this an attached element
    /// </summary>
    public bool IsAttachRequired
    {
      get;
      private set;
    }

    /// <summary>
    /// Is attach possible
    /// </summary>
    public bool IsAttachPossible
    {
      get;
      private set;
    }

    #endregion

    #region Calculation of state
    /// <summary>
    /// Can we attach to the specified table descriptor
    /// </summary>
    internal bool CanAttachTo(FeatureTableDescriptor table)
    {
      bool canAttach = false;
      if (IsAttachPossible)
      {
        var candidateTables = TableDescriptor.CandidateTableDescriptorsForAttach();
        if (candidateTables != null)
        {
          foreach (var attach in candidateTables)
          {
            if (table.Name == attach.Name)
            {
              canAttach = true;
              break;
            }
          }
        }
      }

      return canAttach;
    }

    /// <summary>
    /// Sets a potential attachment
    /// </summary>
    internal void SetAttachmentCandidate(Collection<FeatureTargetGeometry> targetGeometryCollection)
    {
      if (IsAttachPossible)
      {
        FeatureTargetGeometry newTo = null;
        // Set the target geometry from the full collection of candidates
        var attachCandidate = targetGeometryCollection != null && targetGeometryCollection.Count == 1
          ? targetGeometryCollection[0]
          : null;

        if (attachCandidate != null)
        {
          var candidateTables = TableDescriptor.CandidateTableDescriptorsForAttach();
          if (candidateTables != null)
          {
            foreach (var attach in candidateTables)
            {
              if (attachCandidate.Feature.TableDescriptor.Name == attach.Name)
              {
                newTo = attachCandidate;
              }
            }
          }
        }

        AttachTo = newTo;

        CalculateState();
      }
    }

    /// <summary>
    /// Calculates the (enabled) state of this toolbox item
    /// </summary>
    private void CalculateState()
    {
      this.IsEnabled = !IsAttachRequired || AttachTo != null;
    }
    #endregion

    #region Activation
    /// <summary>
    /// Returns a LiteNwFeatureRequestMessage for this item
    /// </summary>
    internal LiteNewFeatureRequestMessage ToNewFeatureRequest(FeatureTargetGeometry forceAttachFeature = null)
    {
      LiteNewFeatureRequestMessage request = null;
      var doAttachTo = forceAttachFeature ?? AttachTo;
      var allowActivation = true;
      if (IsAttachRequired && doAttachTo == null)
      {
        allowActivation = false;
      }

      if (allowActivation)
      {
        var table = TableDescriptor;
        var primaryField = PrimaryGeometryDescriptor;

        SpatialEye.Framework.Geometry.IFeatureGeometry startWithGeometry = CandidateStartWithGeometry;

        if (startWithGeometry != null)
        {
          if (primaryField == null || primaryField.FieldType != startWithGeometry.GeometryType)
          {
            startWithGeometry = null;
          }
        }

        var attachToFeature = doAttachTo != null ? doAttachTo.Feature : null;
        request = new LiteNewFeatureRequestMessage(this, table, primaryField, startWithGeometry, attachToFeature);
      }
      return request;
    }
    #endregion

  }
}
