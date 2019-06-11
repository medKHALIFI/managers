using System;
using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// A message indicating the state of the editmode of the feature details view
  /// </summary>
  public class LiteFeatureDetailsEditModeChangedMessage : MessageBase
  {
    /// <summary>
    /// Constructor for a message indicating the editmode of the feature details
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="editModeActive">is the edit mode active or not</param>
    public LiteFeatureDetailsEditModeChangedMessage(Object sender, Boolean editModeActive)
      : base(sender)
    {
      EditModeActive = editModeActive;
    }

    /// <summary>
    /// The state of the edit mode
    /// </summary>
    public Boolean EditModeActive
    {
      get;
      private set;
    }
  }



}
