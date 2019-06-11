using System;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// Messenger request for activating a feature-value; which means that a hyperlink has been
  /// activated and the user wants to follow to the result (geometry, smart-link, join)
  /// </summary>
  public class LiteActivateFeatureValueRequestMessage : MessageBase
  {
    /// <summary>
    /// Constructor for the activate value request
    /// </summary>
    /// <param name="sender">The originator of the activation</param>
    /// <param name="feature">The feature the value belongs to</param>
    /// <param name="fieldDescriptor">The fielddescriptor corresponding with the value</param>
    public LiteActivateFeatureValueRequestMessage(Object sender, Feature feature, FeatureFieldDescriptor fieldDescriptor)
      :this(sender, feature, fieldDescriptor, feature[fieldDescriptor.Name])
    { }

    /// <summary>
    /// Constructor for the activate value request
    /// </summary>
    /// <param name="sender">The originator of the activation</param>
    /// <param name="feature">The feature the value belongs to</param>
    /// <param name="fieldDescriptor">The fielddescriptor corresponding with the value</param>
    /// <param name="value">The actual value being activate</param>
    public LiteActivateFeatureValueRequestMessage(Object sender, Feature feature, FeatureFieldDescriptor fieldDescriptor, object value)
      : base(sender)
    {
      this.Feature = feature;
      this.FieldDescriptor = fieldDescriptor;
      this.Value = value;
    }

    /// <summary>
    /// The feature the value belongs to
    /// </summary>
    public Feature Feature
    {
      get;
      private set;
    }

    /// <summary>
    /// The fieldDescriptor of the value activated
    /// </summary>
    public FeatureFieldDescriptor FieldDescriptor
    {
      get;
      private set;
    }

    /// <summary>
    /// The actual activated value
    /// </summary>
    public Object Value
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns the description of the feature the value belongs to
    /// </summary>
    public string Description
    {
      get { return Feature != null ? Feature.Description : string.Empty; }
    }

  }
}
