using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.XY.Dtos;
using SpatialEye.Framework.Transactions;
using SpatialEye.Framework.Geometry.CoordinateSystems;
using SpatialEye.Framework.Maps;
using SpatialEye.Framework.Maps.Services;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// The model of a user map that can be (easier) serialized to the Isolated Storage.
  /// </summary>
  public class LiteUserMapStorageModel
  {
    #region Layer Storage
    /// <summary>
    /// Storage class for Map Layers
    /// </summary>
    public class LiteUserMapLayerStorageModel
    {
      /// <summary>
      /// The Default constructor
      /// </summary>
      public LiteUserMapLayerStorageModel()
      {
      }

      /// <summary>
      /// Constructor for MapLayerViewModel
      /// </summary>
      /// <param name="layer"></param>
      public LiteUserMapLayerStorageModel(MapLayerDefinition layer, bool isOn)
      {
        ProviderName = layer.ServiceProvider.Name;
        LayerReference = new XYServiceProviderDatumReferenceDto(layer.ServiceProviderGroup, layer.Name);
        IsOn = isOn;
      }

      public string ProviderName;

      public XYServiceProviderDatumReferenceDto LayerReference;
      public bool IsOn;

      /// <summary>
      /// Creates the mapLayerDefinition
      /// </summary>
      /// <returns></returns>
      public async Task<MapLayerDefinition> ToLayerDefinition()
      {
        MapLayerDefinition result = null;
        var providers = ServiceProviderManager.Instance.ServiceProviders;

        var provider = providers.FirstOrDefault(p => p.Name == ProviderName);

        if (provider != null)
        {
          var mapService = provider.GetService<IMapService>();

          var providerGroup = LayerReference.ServiceProviderGroup.ToServiceProviderGroup(provider);

          if (providerGroup != null)
          {
            var mapLayerRequest = new GetMapLayerDefinitionsRequest()
            {
              GroupNames = new string[] { providerGroup.GroupName },
              GroupTypes = new ServiceProviderGroupType[] { providerGroup.GroupType }
            };

            var mapLayers = await mapService.GetMapLayerDefinitionsAsync(mapLayerRequest);
            if (mapLayers != null)
            {
              result = mapLayers.MapLayerDefinitions.FirstOrDefault(l => l.Name == LayerReference.Name);
              if (result != null)
              {
                result.DefaultVisible = IsOn;
              }
            }
          }
        }

        return result;
      }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// The (required) default constructor for serialization
    /// </summary>
    public LiteUserMapStorageModel()
    { }

    /// <summary>
    /// Constructs the storage model for the specified model
    /// </summary>
    /// <param name="model"></param>
    public LiteUserMapStorageModel(LiteMapViewModel model)
    {
      ProjectName = TransactionContext.ActiveContext.ProjectName;
      ExternalName = model.ExternalName;

      if (model.DefaultEnvelope != null)
      {
        DefaultEnvelope = new XYEnvelopeDto(model.DefaultEnvelope);
      }

      var layers = new List<LiteUserMapLayerStorageModel>();
      foreach (var layer in model.Layers)
      {
        if (layer.LayerDefinition != null)
        {
          layers.Add(new LiteUserMapLayerStorageModel(layer.LayerDefinition, layer.IsOn));
        }
      }

      this.Layers = layers.ToArray();
    }

    #endregion

    #region Properties
    /// <summary>
    /// The project name the map was set up for
    /// </summary>
    public string ProjectName;

    /// <summary>
    /// The external name of the query
    /// </summary>
    public string ExternalName;

    /// <summary>
    /// The envelope
    /// </summary>
    public XYEnvelopeDto DefaultEnvelope;

    /// <summary>
    /// The layers
    /// </summary>
    public LiteUserMapLayerStorageModel[] Layers;
    #endregion

    #region Conversion to a Map
    /// <summary>
    /// Converts the storage model to a MapViewModel that can be used within Lite.
    /// </summary>
    /// <param name="messenger">The messenger the ViewModel should be connected to</param>
    /// <returns>A LiteMapViewModel</returns>
    public async Task<LiteMapViewModel> ToUserMapViewModel(LiteMapsViewModel maps, Messenger messenger, EpsgCoordinateSystemReferenceCollection epsgCoordinateSystems)
    {
      LiteMapViewModel result = null;

      var layers = new List<MapLayerDefinition>();

      try
      {
        foreach (var layer in this.Layers)
        {
          var layerDefinition = await layer.ToLayerDefinition();
          if (layerDefinition != null)
          {
            layers.Add(layerDefinition);
          }
        }

        if (layers.Count > 0)
        {
          Envelope defaultEnvelope = DefaultEnvelope != null ? DefaultEnvelope.ToEnvelope() : null;
          var universe = Universe.Default;
          var mapDefinition = new MapDefinition(ExternalName, ExternalName, universe, defaultEnvelope);

          foreach (var layer in layers)
          {
            mapDefinition.Layers.Add(layer);
          }

          result = new LiteMapViewModel(messenger, mapDefinition, true, epsgCoordinateSystems, World.Default);
        }
      }
      catch
      {
        // Defend against issues upon serializing
      }

      return result;
    }
    #endregion
  }
}
