using System;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A request for restricting a feature collection and adding extra predicates
  /// </summary>
  public class LiteRestrictFeatureCollectionRequestMessage : MessageBase
  {
    /// <summary>
    /// Constructor for request for displaying a feature collection
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="recipeHolder">The holder of the recipe for the feature collection</param>
    public LiteRestrictFeatureCollectionRequestMessage(Object sender, FeatureCollection sourceCollection)
      : base(sender)
    {
      this.FeatureCollection = sourceCollection;

      // Initialize to the source
      this.RestrictedCollection = sourceCollection;
    }

    /// <summary>
    /// The featureColleciton (actually, the holder of a featureCollectionRecipe)
    /// </summary>
    public FeatureCollection FeatureCollection
    {
      get;
      private set;
    }

    /// <summary>
    /// The resulting restricted collection
    /// </summary>
    public FeatureCollection RestrictedCollection
    {
      get;
      set;
    }
  }
}
