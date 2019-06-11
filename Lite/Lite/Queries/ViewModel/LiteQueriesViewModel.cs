using Microsoft.Practices.ServiceLocation;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

using SpatialEye.Framework.Authentication;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Features.Services;
using SpatialEye.Framework.Features.Expressions;
using SpatialEye.Framework.Parameters;
using SpatialEye.Framework.Queries;
using SpatialEye.Framework.ServiceProviders;

using Lite.Resources.Localization;
using SpatialEye.Framework.Queries.Services;

namespace Lite
{
  /// <summary>
  /// The viewModel that handles multiple queries; some coming from the server(s) and
  /// some that are set up client-side. All are specific implementations of LiteQueryViewModel.
  /// </summary>
  public class LiteQueriesViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The queries that are being handled
    /// </summary>
    public const string QueriesPropertyName = "Queries";

    /// <summary>
    /// A flag indicating whether the queriesViewModel is busy
    /// </summary>
    public const string IsBusyPropertyName = "IsBusy";

    /// <summary>
    /// The New User Query that is being set up
    /// </summary>
    public const string NewQueryPropertyName = "NewQuery";

    /// <summary>
    /// The new Query ViewModel for interaction with the new query
    /// </summary>
    public const string NewQueryViewModelPropertyName = "NewQueryViewModel";

    /// <summary>
    /// The view visibility of the new query
    /// </summary>
    public const string NewQueryViewVisibilityPropertyName = "NewQueryViewVisibility";

    /// <summary>
    /// The custom queries visibility
    /// </summary>
    public const string CustomQueriesVisibilityPropertyName = "CustomQueriesVisibility";
    #endregion

    #region Fields
    /// <summary>
    /// The queries that are presented to the user
    /// </summary>
    private SortedObservableCollection<LiteQueryViewModel> _queries;

    /// <summary>
    /// The query filter, indicating which queries are allowed
    /// </summary>
    private Predicate<FeatureCollectionQueryDefinition> _queryDefinitionFilter;

    /// <summary>
    /// A flag indicating whether the queries viewBodel is busy
    /// </summary>
    private bool _isBusy;

    /// <summary>
    /// The viewModel that handles the UI for setting up a new Client Query
    /// </summary>
    private LiteNewUserQueryViewModel _newQueryViewModel;

    /// <summary>
    /// The resulting new clientQuery ViewModel
    /// </summary>
    private LiteQueryViewModel _newQuery;

    /// <summary>
    /// The visibility of the new QueryView
    /// </summary>
    private Visibility _newQueryViewVisibility;

    /// <summary>
    /// Do we allow custom queries
    /// </summary>
    private bool _isCustomQueriesVisible = true;
    #endregion

    #region Constructor
    /// <summary>
    /// The constructor
    /// </summary>
    /// <param name="messenger"></param>
    public LiteQueriesViewModel(Messenger messenger = null)
      : base(messenger)
    {
      Resources = new Lite.Resources.Localization.ApplicationResources();

      NewQueryViewModel = new LiteNewUserQueryViewModel(messenger);

      _newQueryViewVisibility = Visibility.Collapsed;

      SetupCommands();

      AttachToMessengerAndParameters();
    }
    #endregion

    #region Messenger and Parameter handling
    /// <summary>
    /// Attach to messenger and the parameters
    /// </summary>
    private void AttachToMessengerAndParameters()
    {
      if (!IsInDesignMode)
      {
        this.Messenger.Register<LiteActionRunningStateMessage>(this, HandleRunningStateChange);
        SystemParameterManager.Instance.ParameterValueChanged += HandleSystemParameterValueChanged;
      }
    }

    /// <summary>
    /// Handles the change in running state
    /// </summary>
    private void HandleRunningStateChange(LiteActionRunningStateMessage runningStateMessage)
    {
      // Propagate the changes to the individual queries; in case one of the queries
      // is responsible for the running-state change, make it display the progress indicator.
      foreach (var query in this.Queries)
      {
        query.HandleRunningStateChange(runningStateMessage);
      }

      if (this.NewQuery != null)
      {
        this.NewQuery.HandleRunningStateChange(runningStateMessage);
      }
    }

    /// <summary>
    /// Callback for changes in system parameters; request all queries to have their state checked
    /// </summary>
    void HandleSystemParameterValueChanged(ParameterDefinition definition, ParameterValue sender)
    {
      CheckCommands();
    }
    #endregion

    #region Commands
    /// <summary>
    /// The command that should open the selected query
    /// This controls the QueryDialogViewVisibility 
    /// /// </summary>
    public RelayCommand OpenNewQueryDialogCommand { get; set; }

    /// <summary>
    /// The command that should close the active query.
    /// This controls the QueryDialogViewVisibility 
    /// </summary>
    public RelayCommand CloseNewQueryDialogCommand { get; set; }

    /// <summary>
    /// The command that should close the active query.
    /// This controls the QueryDialogViewVisibility 
    /// </summary>
    public RelayCommand SaveNewQueryCommand { get; set; }

    /// <summary>
    /// The command that should close the active query.
    /// This controls the QueryDialogViewVisibility 
    /// </summary>
    public RelayCommand RunNewQueryCommand { get; set; }

    /// <summary>
    /// Set up the available commands of the view model
    /// </summary>
    private void SetupCommands()
    {
      // Set up the open/close commands using lambdas
      OpenNewQueryDialogCommand = new RelayCommand(
        () =>
        {
          NewQueryViewModel.ResetNewQueryName();
          NewQueryViewVisibility = Visibility.Visible;
        },
        () => this.NewQueryViewVisibility == Visibility.Collapsed);

      CloseNewQueryDialogCommand = new RelayCommand(() => NewQueryViewVisibility = Visibility.Collapsed, () => NewQueryViewVisibility == Visibility.Visible);

      // Set up the save command using a separate method for carrying out the actual saving of the new query
      RunNewQueryCommand = new RelayCommand(RunNewQuery, () => NewQueryViewVisibility == Visibility.Visible);
      SaveNewQueryCommand = new RelayCommand(SaveNewQuery, () => NewQueryViewVisibility == Visibility.Visible);
    }

    /// <summary>
    /// Check the enabled states of the commands
    /// </summary>
    private void CheckCommands()
    {
      OpenNewQueryDialogCommand.RaiseCanExecuteChanged();
      RunNewQueryCommand.RaiseCanExecuteChanged();
      SaveNewQueryCommand.RaiseCanExecuteChanged();
      CloseNewQueryDialogCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Returns a new external name for the specified suggested name
    /// </summary>
    private string UniqueExternalNameFor(string suggestedExternalName)
    {
      var externalName = suggestedExternalName;
      var exists = new Func<string, bool>(n => Queries.Where(m => m.ExternalName.ToLower().Equals(n)).Count() > 0);
      int count = 2;
      while (exists(externalName.ToLower())) externalName = externalName = string.Format("{0} ({1})", suggestedExternalName, count++);
      return externalName;
    }

    /// <summary>
    /// The method that handles running the new query 
    /// </summary>
    private void RunNewQuery()
    {
      var mode = this.NewQueryViewModel.SelectedMode;
      if (mode != null)
      {
        // Get new Query details from the selected mode
        var table = mode.TableDescriptor;
        var expression = mode.NewQueryExpression();
        var parameters = mode.NewQueryParameterDefinitions();

        NewQuery = NewUserQuery(table.ExternalName, table, expression, parameters, false);

        if (NewQuery != null)
        {
          // Request the newly created query to run
          NewQuery.RunQuery();
        }
      }
    }

    /// <summary>
    /// The method that handles saving of the constructed query.
    /// </summary>
    private void SaveNewQuery()
    {
      var mode = this.NewQueryViewModel.SelectedMode;
      if (mode != null)
      {
        // Get the name
        var queryName = this.NewQueryViewModel.QueryName;

        // Make sure there is a name
        if (string.IsNullOrEmpty(queryName) || String.Compare(queryName, ApplicationResources.Query, StringComparison.OrdinalIgnoreCase) == 0)
        {
          queryName = string.Concat(ApplicationResources.Query, " (", mode.TableDescriptor.ExternalName, ")");
        }

        // Make sure it's unique
        queryName = UniqueExternalNameFor(queryName);

        // Get new Query details from the selected mode
        var table = mode.TableDescriptor;
        var description = UniqueExternalNameFor(queryName);
        var expression = mode.NewQueryExpression();
        var parameters = mode.NewQueryParameterDefinitions();

        var newQuery = NewUserQuery(description, table, expression, parameters, true);

        if (newQuery != null)
        {
          // Track in Analytics
          LiteAnalyticsTracker.TrackQueryCreate(newQuery);

          // Request the newly created query to run
          newQuery.RunQuery();
        }
      }

      NewQueryViewVisibility = Visibility.Collapsed;
    }
    #endregion

    #region Authentication/Queries Property Changes
    /// <summary>
    /// Whenever authentication changes, (re)get the queries. 
    /// </summary>
    /// <param name="context">The new authorisation context</param>
    /// <param name="isAuthenticated">A flag indicating whether the user is authenticated</param>
    protected override void OnAuthenticationChanged(AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Automatically get the queries upon authentication
        var ignored = GetQueriesAsync();

        // Get the custom queries settings
        this.IsCustomQueriesVisible = LiteClientSettingsViewModel.Instance.AllowCustomQueries;
      }
      else
      {
        // Not authenticated anymore; get rid of our queries
        EmptyQueries();
      }
    }
    #endregion

    #region Queries
    /// <summary>
    /// Empties all queries
    /// </summary>
    private void EmptyQueries()
    {
      this.Queries = new SortedObservableCollection<LiteQueryViewModel>(LiteQueryViewModel.LiteQueryViewModelComparer);
    }

    /// <summary>
    /// Gets all queries asynchronousluy
    /// </summary>
    public async Task GetQueriesAsync()
    {
      IsBusy = true;

      var queryModels = new List<LiteQueryViewModel>();

      try
      {
        // Get the Server Queries
        queryModels.AddRange(await this.GetProjectQueries());

        // Subsequently get the client queries (is done synchronously)
        queryModels.AddRange(await this.GetUserQueries());
      }
      finally
      {
        this.Queries = new SortedObservableCollection<LiteQueryViewModel>(LiteQueryViewModel.LiteQueryViewModelComparer, queryModels);

        IsBusy = false;
      }
    }

    /// <summary>
    /// Gets the query definitions from the server
    /// </summary>
    /// <returns></returns>
    private async Task<IList<LiteQueryViewModel>> GetProjectQueries()
    {
      var queryViewModels = new List<LiteQueryViewModel>();
      if (IsAuthenticated)
      {
        try
        {
          var queries = await GetService<IQueryService>().GetQueryDefinitionsAsync();
          foreach (var query in queries)
          {
            if (query.IsUserVisible && (QueryDefinitionFilter == null || QueryDefinitionFilter(query)))
            {
              queryViewModels.Add(new LiteQueryViewModel(this.Messenger, query));
            }
          }
        }
        catch (Exception)
        {
          // Something went wrong while retrieving the queries

        }
      }
      return queryViewModels;
    }

    /// <summary>
    /// Gets the query definitions as stored in the Isolated Storage
    /// </summary>
    private async Task<IList<LiteQueryViewModel>> GetUserQueries()
    {
      var queryViewModels = new List<LiteQueryViewModel>();

      try
      {
        var request = new GetDDRequest()
        {
          GroupTypes = new ServiceProviderGroupType[] { ServiceProviderGroupType.Business, ServiceProviderGroupType.Analysis }
        };

        var allSources = await ServiceLocator.Current.GetInstance<ICollectionService>().GetDDAsync(request);

        var isolatedStorageQueries = LiteIsolatedStorageManager.Instance.UserQueries;

        if (isolatedStorageQueries != null)
        {
          foreach (var isolatedStorageQuery in isolatedStorageQueries)
          {
            var userQuery = await isolatedStorageQuery.ToUserQueryViewModel(this.Messenger, allSources);
            if (userQuery != null)
            {
              queryViewModels.Add(userQuery);
            }
          }
        }
      }
      catch
      {
        // The stored queries could not be matched with our model
        // We could choose to clear the stored client queries
      }

      return queryViewModels;
    }

    /// <summary>
    /// Saves all client queries to the isolated storage
    /// </summary>
    private void SaveUserQueries()
    {
      var queries = this.Queries;
      if (queries != null)
      {
        var isolatedStorageUserQueries = new List<LiteUserQueryStorageModel>();

        foreach (var queryViewModel in queries)
        {
          if (queryViewModel.Query.Context == ServiceProviderDatumContext.User)
          {
            isolatedStorageUserQueries.Add(new LiteUserQueryStorageModel(queryViewModel));
          }
        }

        // And save the lot
        LiteIsolatedStorageManager.Instance.UserQueries = isolatedStorageUserQueries;
      }
    }

    /// <summary>
    /// Adds the given client-query to the list of queries
    /// </summary>
    /// <param parameterName="parameterExternalName">The parameterName to show to the end user</param>
    /// <param parameterName="table">The table descriptor that the query belongs to</param>
    /// <param parameterName="predicate">The predicate that defines the filter for the query</param>
    /// <param parameterName="parameterDefinitions">The parameter definitions defining the functionParameters to be used</param>
    private LiteQueryViewModel NewUserQuery(string externalName, FeatureTableDescriptor table, System.Linq.Expressions.Expression predicate, ParameterDefinitionCollection parameterDefinitions, bool addToQueries)
    {
      LiteQueryViewModel clientQuery = null;
      if (IsAuthenticated && Queries != null)
      {
        var predicateText = predicate != null ? predicate.GeoLinqText(useInternalParameterNames: true) : null;

        var queryDefinition = new FeatureCollectionQueryDefinition()
        {
          ServiceProviderGroup = table.ServiceProviderGroup,
          Context = ServiceProviderDatumContext.User,
          Name = externalName.ToLower(),
          ExternalName = externalName,
          TableDescriptor = table,
          ParameterDefinitions = parameterDefinitions
        };

        clientQuery = new LiteQueryViewModel(this.Messenger, queryDefinition, predicateText);

        if (addToQueries)
        {
          Queries.Add(clientQuery);

          SaveUserQueries();
        }
      }

      return clientQuery;
    }

    /// <summary>
    /// Remove the specified query
    /// </summary>
    /// <param parameterName="query">The query to remove</param>
    private async void RemoveUserQuery(LiteQueryViewModel query)
    {
      if (Queries != null && query != null)
      {
        var caption = ApplicationResources.QueryRemoveTitle;
        var removeString = string.Format(ApplicationResources.QueryRemoveStringFormat, query.ExternalName);
        var result = await this.MessageBoxService.ShowAsync(removeString, caption, MessageBoxButton.OKCancel, MessageBoxResult.Cancel);
        if (result == MessageBoxResult.OK)
        {
          Queries.Remove(query);
        }

        if (query.Query != null && query.Query.Context == ServiceProviderDatumContext.User)
        {
          SaveUserQueries();
        }
      }
    }

    /// <summary>
    /// Whenever a query is renamed, restore it in the sorted collection
    /// </summary>
    /// <param parameterName="query">The query that was renamed</param>
    private void RenamedQuery(LiteQueryViewModel query)
    {
      if (Queries != null && query != null)
      {
        // Might need to be reordered; reinsert the item
        Queries.Remove(query);
        Queries.Add(query);

        SaveUserQueries();
      }
    }
    #endregion

    #region Changes in Queries
    /// <summary>
    /// Attach all queries, making sure we respond to collection changes as well
    /// as changes in individual queries
    /// </summary>
    private void AttachQueries()
    {
      if (_queries != null)
      {
        _queries.CollectionChanged += QueriesCollectionChanged;

        foreach (var query in _queries)
        {
          AttachQuery(query);
        }
      }
    }

    /// <summary>
    /// Detach from the queries, since we are setting up a new query collection.
    /// Detach from all individual queries as well
    /// </summary>
    private void DetachQueries()
    {
      if (_queries != null)
      {
        _queries.CollectionChanged -= QueriesCollectionChanged;

        foreach (var query in _queries)
        {
          DetachQuery(query);
        }
      }
    }

    /// <summary>
    /// Callback for changes in the queries collection; detach all old items (removed items) and attach
    /// to new queries.
    /// </summary>
    void QueriesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (e.OldItems != null)
      {
        foreach (LiteQueryViewModel item in e.OldItems)
        {
          DetachQuery(item);
        }
      }

      if (e.NewItems != null)
      {
        foreach (LiteQueryViewModel item in e.NewItems)
        {
          AttachQuery(item);
        }
      }
    }

    /// <summary>
    /// Attach to the query's remove command and property change notification
    /// </summary>
    /// <param name="query">The query to attach to</param>
    private void AttachQuery(LiteQueryViewModel query)
    {
      if (query != null)
      {
        query.RequestRemoveQuery += RemoveUserQuery;
        query.PropertyChanged += QueryPropertyChanged;
      }
    }

    /// <summary>
    /// Detach from the query's remove command and property change notification
    /// </summary>
    /// <param name="query">The query to detach from</param>
    private void DetachQuery(LiteQueryViewModel query)
    {
      if (query != null)
      {
        query.RequestRemoveQuery -= RemoveUserQuery;
        query.PropertyChanged -= QueryPropertyChanged;
      }
    }

    /// <summary>
    /// Callback for property changes in a query
    /// </summary>
    void QueryPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == LiteQueryViewModel.ExternalNamePropertyName)
      {
        var query = sender as LiteQueryViewModel;
        RenamedQuery(query);
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// A flag indicating whether we are busy
    /// </summary>
    public bool IsBusy
    {
      get { return _isBusy; }
      set
      {
        if (_isBusy != value)
        {
          _isBusy = value;
          RaisePropertyChanged(IsBusyPropertyName);
        }
      }
    }

    /// <summary>
    /// Holds an observable collection of all queries (server as well as client)
    /// </summary>
    public SortedObservableCollection<LiteQueryViewModel> Queries
    {
      get { return _queries; }
      set
      {
        if (_queries != value)
        {
          if (_queries != null)
          {
            DetachQueries();
          }

          var oldQueries = _queries;
          _queries = value;

          if (_queries != null)
          {
            AttachQueries();
          }

          RaisePropertyChanged(QueriesPropertyName, oldQueries, _queries, true);
        }
      }
    }

    /// <summary>
    /// The query filter, indicating which queries are allowed
    /// </summary>
    public Predicate<FeatureCollectionQueryDefinition> QueryDefinitionFilter
    {
      get { return _queryDefinitionFilter; }
      set
      {
        if (_queryDefinitionFilter != value)
        {
          _queryDefinitionFilter = value;

          // If the filter changes, get all queries afresh. Since this is a task, 
          // suppress the warning by assigning the result
          var ignored = GetQueriesAsync();
        }
      }
    }

    /// <summary>
    /// Holds the new Query
    /// </summary>
    public LiteQueryViewModel NewQuery
    {
      get { return _newQuery; }
      set
      {
        if (value != _newQuery)
        {
          _newQuery = value;
          RaisePropertyChanged(NewQueryPropertyName);
        }
      }
    }

    /// <summary>
    /// The view model handling new queries
    /// </summary>
    public LiteNewUserQueryViewModel NewQueryViewModel
    {
      get { return _newQueryViewModel; }
      set
      {
        if (_newQueryViewModel != value)
        {
          _newQueryViewModel = value;
          RaisePropertyChanged(NewQueryViewModelPropertyName);
        }
      }
    }

    /// <summary>
    /// Indicates whether an active QuickFind Query should be run automatically
    /// when the OpenCommand is executed.
    /// </summary>
    public Visibility NewQueryViewVisibility
    {
      get { return _newQueryViewVisibility; }
      set
      {
        if (value != _newQueryViewVisibility)
        {
          _newQueryViewVisibility = value;
          CheckCommands();
          RaisePropertyChanged(NewQueryViewVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Flag indicating whether the Custom Query button is visible
    /// </summary>
    public bool IsCustomQueriesVisible
    {
      get { return _isCustomQueriesVisible; }
      set
      {
        if (_isCustomQueriesVisible != value)
        {
          _isCustomQueriesVisible = value;
          RaisePropertyChanged(CustomQueriesVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// The custom query button's visibility
    /// </summary>
    public Visibility CustomQueriesVisibility
    {
      get { return _isCustomQueriesVisible ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
