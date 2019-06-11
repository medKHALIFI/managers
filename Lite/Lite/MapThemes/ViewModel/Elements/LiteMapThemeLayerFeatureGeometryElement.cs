using System.Linq.Expressions;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Parameters;

using glf = SpatialEye.Framework.Features.Expressions.GeoLinqExpressionFactory;

namespace Lite
{
  /// <summary>
  /// A sample representation of a Feature Geometry element, displaying the number of elements 
  /// </summary>
  public class LiteMapThemeLayerFeatureGeometryElement : MapThemeLayerElementViewModel
  {
    #region Static Property Names
    /// <summary>
    /// The property name for the description property
    /// </summary>
    public static string DescriptionPropertyName = "Description";
    #endregion

    #region Fields
    /// <summary>
    /// The description to be displayed as part of the Themes
    /// </summary>
    private string _description;
    private FeatureGeometryMapLayerViewModel _featureGeometryMapLayerViewModel;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor for the feature geometry description element
    /// </summary>
    internal LiteMapThemeLayerFeatureGeometryElement(MapViewModel map, FeatureGeometryMapLayerViewModel featureGeometryLayer)
    {
      this.Map = map;
      this.Map.PropertyChanged += Map_PropertyChanged;

      this.FeatureGeometryLayer = featureGeometryLayer;
    }
    #endregion

    #region Internal helpers
    /// <summary>
    /// Returns a predicate that determines whether things are on screen
    /// </summary>
    private Expression OnScreenPredicate()
    {
      var field = FeatureGeometryLayer.GeometryField;
      var constant = SystemParameterManager.Instance.CurrentMapExtent;

      return constant != null ? glf.Spatial.Interacts(glf.Data.Field(field), glf.Constant(constant)) : null;
    }

    /// <summary>
    /// A property of the map has changed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void Map_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == MapViewModel.EnvelopePropertyName)
      {
        // Only set up the description in case the map is not busy
        if (!Map.IsAnimating && !Map.IsPanning)
        {
          SetupDescription();
        }
      }
    }

    /// <summary>
    /// A property has changed of the feature geometry layer
    /// </summary>
    void FeatureGeometryLayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == FeatureGeometryMapLayerViewModel.FeaturesPropertyName)
      {
        SetupDescription();
      }
    }

    /// <summary>
    /// Sets up the description
    /// </summary>
    private async void SetupDescription()
    {
      var features = this.FeatureGeometryLayer.Features;
      var count = 0;
      var countOnScreen = 0;

      if (features != null)
      {
        var onScreen = OnScreenPredicate();

        count = features.Count;
        countOnScreen = onScreen != null ? await features.Where(onScreen).CountAsync() : 0;
      }

      Description = string.Format("{0} - {1} {2}", countOnScreen, count, this.FeatureGeometryLayer.GeometryField.TableDescriptor.ExternalName);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// The map that we are belonging to
    /// </summary>
    private MapViewModel Map
    {
      get;
      set;
    }

    /// <summary>
    /// The feature geometry layer
    /// </summary>
    private FeatureGeometryMapLayerViewModel FeatureGeometryLayer
    {
      get
      {
        return _featureGeometryMapLayerViewModel;
      }
      
      set
      {
        if (_featureGeometryMapLayerViewModel != null)
        {
          _featureGeometryMapLayerViewModel.PropertyChanged -= FeatureGeometryLayer_PropertyChanged;
        }

        _featureGeometryMapLayerViewModel = value;

        _featureGeometryMapLayerViewModel.PropertyChanged += FeatureGeometryLayer_PropertyChanged;
      }
    }

    /// <summary>
    /// The description of the Feature Geometry Layer, to be picked up for display
    /// in the themes part
    /// </summary>
    public string Description
    {
      get { return _description; }
      set
      {
        if (_description != value)
        {
          _description = value;
          RaisePropertyChanged(DescriptionPropertyName);
        }
      }
    }
    #endregion
  }
}
