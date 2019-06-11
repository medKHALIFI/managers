using System.Collections.Generic;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A request for calculating/yielding the candidates for attachment to 
  /// the table descriptor
  /// </summary>
  public class LiteGetAttachCandidatesRequestMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Constructor for the request
    /// </summary>
    public LiteGetAttachCandidatesRequestMessage(object sender, FeatureTableDescriptor table)
      : base(sender)
    {
      TableDescriptor = table;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The contents of the request for editing
    /// </summary>
    public FeatureTableDescriptor TableDescriptor
    {
      get;
      private set;
    }

    /// <summary>
    /// The (calculated) candidates for attaching to the specified table descriptor
    /// </summary>
    public IList<FeatureInsertItemViewModel> AttachCandidates
    {
      get;
      set;
    }
    #endregion
  }
}
