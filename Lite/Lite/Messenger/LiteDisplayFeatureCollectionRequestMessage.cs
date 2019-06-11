using System;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Features.Recipe;

namespace Lite
{
  /// <summary>
  /// A request for displaying a feature collection
  /// </summary>
  public class LiteDisplayFeatureCollectionRequestMessage : MessageBase
  {
    /// <summary>
    /// Constructor for request for displaying a feature collection
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="recipeHolder">The holder of the recipe for the feature collection</param>
    public LiteDisplayFeatureCollectionRequestMessage(Object sender, IFeatureCollectionRecipeHolder recipeHolder)
      : base(sender)
    {
      if (recipeHolder != null)
      {
        // Check whether this is a direct collection
        var collection = recipeHolder as FeatureCollection;
        if (collection == null)
        {
          var recipe = recipeHolder.CollectionRecipe;
          if (recipe != null)
          {
            collection = new FeatureRecipeCollection(recipe);
          }
        }

        this.FeatureCollection = collection;
      }
    }

    /// <summary>
    /// The featureColleciton (actually, the holder of a featureCollectionRecipe)
    /// </summary>
    public FeatureCollection FeatureCollection
    {
      get;
      private set;
    }
  }
}
