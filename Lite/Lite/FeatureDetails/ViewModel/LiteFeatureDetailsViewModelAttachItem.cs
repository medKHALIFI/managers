using System;
using System.Windows.Input;

using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// A container for attach items in the feature details view model
  /// </summary>
  public class LiteFeatureDetailsViewModelAttachItem
  {
    /// <summary>
    /// The constructor for the attach item
    /// </summary>
    public LiteFeatureDetailsViewModelAttachItem(string caption,string description, string pathName, Action action)
    {
      Name = caption;
      Description = description;
      Path = pathName;
      StartAction = action;
    }

    /// <summary>
    /// The caption
    /// </summary>
    public string Name
    {
      get;
      private set;
    }

    /// <summary>
    /// The description
    /// </summary>
    public string Description
    {
      get;
      private set;
    }

    /// <summary>
    /// The path
    /// </summary>
    public string Path
    {
      get;
      private set;
    }

    /// <summary>
    /// The action to start creation of the attachment
    /// </summary>
    public Action StartAction
    {
      get;
      private set;
    }

  }
}
