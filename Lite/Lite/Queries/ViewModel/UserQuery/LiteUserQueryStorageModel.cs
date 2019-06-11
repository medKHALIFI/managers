using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Queries;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.XY.Dtos;

using SpatialEye.Framework.Transactions;
using System.Threading.Tasks;

namespace Lite
{
  /// <summary>
  /// The model of a user query that can be (easier) serialized to the Isolated Storage.
  /// </summary>
  public class LiteUserQueryStorageModel
  {
    #region Constructors
    /// <summary>
    /// The (required) default constructor for serialization
    /// </summary>
    public LiteUserQueryStorageModel()
    { }

    /// <summary>
    /// Constructs the storage model for the specified model
    /// </summary>
    /// <param name="model"></param>
    public LiteUserQueryStorageModel(LiteQueryViewModel model)
    {
      var query = model.Query;
      var table = query.TableDescriptor;

      ProjectName = TransactionContext.ActiveContext.ProjectName;

      ProviderName = table.ServiceProviderGroup.ServiceProvider.Name;
      ProviderGroup = new XYServiceProviderGroupDto(table.ServiceProviderGroup, false);
      if (query.ParameterDefinitions != null)
      {
        ParameterDefinitions = new XYParameterDefinitionCollectionDto(query.ParameterDefinitions);
      }

      Name = model.Name;
      ExternalName = model.ExternalName;

      TableName = table.Name;
      PredicateText = model.Predicate;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The project name the query was set up for
    /// </summary>
    public string ProjectName;

    /// <summary>
    /// The name of the provider
    /// </summary>
    public string ProviderName;

    /// <summary>
    /// The provider group (named analysis/business)
    /// </summary>
    public XYServiceProviderGroupDto ProviderGroup;

    /// <summary>
    /// The table the query is defined against
    /// </summary>
    public string TableName;

    /// <summary>
    /// The parameter definitions
    /// </summary>
    public XYParameterDefinitionCollectionDto ParameterDefinitions;

    /// <summary>
    /// The name of the query
    /// </summary>
    public string Name;

    /// <summary>
    /// The external name of the query
    /// </summary>
    public string ExternalName;

    /// <summary>
    /// The predicate text in case the query is predicated
    /// </summary>
    public string PredicateText;
    #endregion

    #region Conversion to Query
    /// <summary>
    /// Converts the storage model to a QueryViewModel that can be used within Lite.
    /// </summary>
    /// <param name="messenger">The messenger the ViewModel should be connected to</param>
    /// <param name="sources">The sources available for resolution of the Table</param>
    /// <returns>A LiteQueryViewModel</returns>
    public async Task<LiteQueryViewModel> ToUserQueryViewModel(Messenger messenger, ServiceProviderDatumCollection<FeatureSourceDescriptor> sources)
    {
      LiteQueryViewModel result = null;

      if (TransactionContext.ActiveContext.ProjectName == this.ProjectName)
      {
        var serviceProvider = ServiceProviderManager.Instance.ServiceProvider(ProviderName);
        if (serviceProvider != null)
        {
          var group = ProviderGroup.ToServiceProviderGroup(serviceProvider);
          var source = sources.Find(group);

          if (source != null)
          {
            var table = source.TableDescriptors[TableName];

            // Check the table, since it can have disappeared in the mean-time
            if (table != null)
            {
              // Ensure we have a table with fields and all
              await table.EvaluateAsync();

              var parameterDefinitions = this.ParameterDefinitions.ToParameterDefinitionCollection(serviceProvider);
              var queryDefinition = new FeatureCollectionQueryDefinition()
              {
                ServiceProviderGroup = table.ServiceProviderGroup,
                Context = ServiceProviderDatumContext.User,
                Name = Name,
                ExternalName = ExternalName,
                TableDescriptor = table,
                ParameterDefinitions = parameterDefinitions
              };

              result = new LiteQueryViewModel(messenger, queryDefinition, PredicateText);
            }
          }
        }
      }

      return result;
    }
    #endregion
  }
}
