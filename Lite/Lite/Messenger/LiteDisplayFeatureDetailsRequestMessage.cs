using System;
using System.Collections.Generic;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features.Recipe;
using SpatialEye.Framework.Features;

namespace Lite
{
  /// <summary>
  /// A reques for displaying the details of a feature
  /// </summary>
  public class LiteDisplayFeatureDetailsRequestMessage : MessageBase
  {
    /// <summary>
    /// Constructs the request
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="feature">The feature (recipe-holder) to show details for</param>
    public LiteDisplayFeatureDetailsRequestMessage(Object sender, IFeatureRecipeHolder feature, FeatureGeometryFieldDescriptor geometryField = null, bool makeViewActive = false, bool startEditing = false)
      : base(sender)
    {
      RecipeHolders = new List<IFeatureRecipeHolder> { feature };
      SelectedGeometryFieldDescriptor = geometryField;
      StartEditing = geometryField != null && startEditing;
      MakeViewActive = makeViewActive;
    }

    /// <summary>
    /// Constructs the request
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="feature">The features to show details for</param>
    public LiteDisplayFeatureDetailsRequestMessage(Object sender, IList<IFeatureRecipeHolder> features, FeatureGeometryFieldDescriptor geometryField = null)
      : base(sender)
    {
      RecipeHolders = new List<IFeatureRecipeHolder>(features);
      SelectedGeometryFieldDescriptor = geometryField;
    }

    /// <summary>
    /// The feature (recipe-holder) to show details for
    /// </summary>
    public IList<IFeatureRecipeHolder> RecipeHolders
    {
      get;
      private set;
    }

    /// <summary>
    /// The Geometry Field that was selected
    /// </summary>
    public FeatureGeometryFieldDescriptor SelectedGeometryFieldDescriptor
    {
      get;
      private set;
    }

    /// <summary>
    /// Make the view active
    /// </summary>
    public bool MakeViewActive
    {
      get;
      private set;
    }

    /// <summary>
    /// Start editing the geometry immediately
    /// </summary>
    public bool StartEditing
    {
      get;
      private set;
    }
  }
}
