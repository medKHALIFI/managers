using System.Collections.Generic;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A message that is sent via the messenger to tell other parts of the 
  /// application that a feature transaction is about to be carried out
  /// </summary>
  public class LiteFeatureTransactionMessage : MessageBase
  {
    #region Internal class
    /// <summary>
    /// The type of transaction
    /// </summary>
    public enum TransactionType
    {
      Inserted,
      Updated,
      Deleted
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor for the transaction change for the specified feature and type of change
    /// </summary>
    public LiteFeatureTransactionMessage(object sender, Feature feature, TransactionType type, IEnumerable<FeatureFieldDescriptor> affectedFields)
      : base(sender)
    {
      Feature = feature;
      Type = type;

      ChangedFields = new List<FeatureFieldDescriptor>(affectedFields);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// The Type of transaction (change)
    /// </summary>
    public TransactionType Type
    {
      get;
      private set;
    }

    /// <summary>
    /// The Feature the tranaction (change) applies to
    /// </summary>
    public Feature Feature
    {
      get;
      private set;
    }

    /// <summary>
    /// The table descriptor
    /// </summary>
    public FeatureTableDescriptor TableDescriptor
    {
      get { return Feature.TableDescriptor; }
    }

    /// <summary>
    /// Holds the changed fields
    /// </summary>
    public IList<FeatureFieldDescriptor> ChangedFields
    {
      get;
      private set;
    }

    /// <summary>
    /// Does this transaction hold a geometry change
    /// </summary>
    public bool ContainsGeometryChange
    {
      get
      {
        var containsGeometry = false;
        foreach (var field in ChangedFields)
        {
          if (field.IsGeometry)
          {
            containsGeometry = true;
            break;
          }
        }

        return containsGeometry;
      }
    }
    #endregion
  }
}

