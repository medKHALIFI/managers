using System;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;

using SpatialEye.Framework.Features.Recipe;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Queries;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// QueryViewModel holds the logic for interfacing with a QueryDefinition.
  /// </summary>
  public class LiteQueryViewModel : ViewModelBase
  {
    #region Request remove delegate
    /// <summary>
    /// Request to remove the query
    /// </summary>
    internal delegate void RequestRemoveRequeryDelegate(LiteQueryViewModel query);
    #endregion

    #region Static Comparer
    /// <summary>
    ///  A function that can be used as comparer between maps
    /// </summary>
    public static Func<LiteQueryViewModel, LiteQueryViewModel, int> LiteQueryViewModelComparer = new Func<LiteQueryViewModel, LiteQueryViewModel, int>(
      (a, b) =>
      {
        int valueA = (int)a.Query.Context;
        int valueB = (int)b.Query.Context;
        int compare = valueB.CompareTo(valueA);

        if (compare == 0)
        {
          compare = a.ExternalName.CompareTo(b.ExternalName);
        }

        return compare;
      });
    #endregion

    #region Property Names
    /// <summary>
    /// The external parameterName of the (underlying) query
    /// </summary>
    public const string ExternalNamePropertyName = "ExternalName";

    /// <summary>
    /// The functionParameters (as parameterViewModels) that hold the user values for
    /// the query, allowing to be changed by the user
    /// </summary>
    public const string ParametersPropertyName = "Parameters";

    /// <summary>
    /// Is the query enabled (can it be run)
    /// </summary>
    public const string CanRunPropertyName = "CanRun";

    /// <summary>
    /// Is this queryViewModel busy retrieving its result collection
    /// </summary>
    public const string IsRunningPropertyName = "IsRunning";

    /// <summary>
    /// Holds the running state description
    /// </summary>
    public const string RunningStateDescriptionPropertyName = "RunningStateDescription";

    /// <summary>
    /// Visibility dependent on the state whether the the query is running
    /// </summary>
    public const string IsRunningVisibilityPropertyName = "IsRunningVisibility";

    /// <summary>
    /// Is the query expanded
    /// </summary>
    public const string IsExpandedPropertyName = "IsExpanded";

    /// <summary>
    /// Is the query view visible (expanded)
    /// </summary>
    public const string IsExpandedVisibilityPropertyName = "IsExpandedVisibility";

    /// <summary>
    /// Are the query reports enabled
    /// </summary>
    public const string ReportsEnabledPropertyName = "ReportsEnabled";

    /// <summary>
    /// Are the query reports available/visible
    /// </summary>
    public const string ReportsVisibilityPropertyName = "ReportsVisibility";

    /// <summary>
    /// Are the exports enabled
    /// </summary>
    public const string ExportsEnabledPropertyName = "ExportsEnabled";

    /// <summary>
    /// Are the exports visible
    /// </summary>
    public const string ExportsVisibilityPropertyName = "ExportsVisibility";
    #endregion

    #region Fields
    /// <summary>
    /// The functionParameters to be used
    /// </summary>
    private ParameterValuesViewModel _parameters;

    /// <summary>
    /// The enabled state of the query
    /// </summary>
    private bool _canRun;

    /// <summary>
    /// A flag indicating whether the query is busy retrieving its result
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Holds the descriptive running state
    /// </summary>
    private string _runningStateDescription;

    /// <summary>
    /// A flag indicating whether the query is expanded
    /// </summary>
    private bool _isExpanded;
    #endregion

    #region Constructor
    /// <summary>
    /// The query view model constructor
    /// </summary>
    internal LiteQueryViewModel(Messenger messenger, FeatureCollectionQueryDefinition query, string predicateText = null)
    {
      Resources = new Lite.Resources.Localization.ApplicationResources();

      // Set the query
      this.Query = query;

      this.Predicate = predicateText;

      // The initial functionParameters are determined by the user definitions available in the parameter definitions
      this.Parameters = new ParameterValuesViewModel(query.ParameterDefinitions);

      // Reports
      this.Reports = new ReportsViewModel(messenger) { AutoGetAvailableDefinitions = true };
      this.Reports.PropertyChanged += Reports_PropertyChanged;

      // Exports
      this.Exports = new ExportsViewModel(messenger)
      {
        AutoGetAvailableDefinitions = true,
        AllowCoordinateSystemSettings = LiteClientSettingsViewModel.Instance.ExportAllowSetCS,
        AllowUnitSettings = LiteClientSettingsViewModel.Instance.ExportAllowSetUnits,
        DefinitionsFilter = definition => LiteClientSettingsViewModel.Instance.ExportAllowedTypes.Contains(definition.ExportType),
        ExportCoordinateSystemsFilter = cs => cs.SRId != 3785
      };

      // Get property changes
      this.Exports.PropertyChanged += Exports_PropertyChanged;

      // Set up the commands for running the query
      SetupCommands();

      this.PathGeometry = Application.Current.Resources[PathGeometryKey()] as string;

      CalculateRecipe();
    }

    public string PathGeometryKey()
    {
      switch (Query.Context)
      {
        case ServiceProviderDatumContext.User: return "MetroIcon.Content.User";
        default: return "MetroIcon.Content.Filter";
      }
    }
    #endregion

    #region Command Logic
    /// <summary>
    /// The command that runs the query
    /// </summary>
    public RelayCommand RunCommand { get; private set; }

    /// <summary>
    /// The command that runs the query export
    /// </summary>
    public RelayCommand<object> RunExportCommand { get; private set; }

    /// <summary>
    /// The command that runs the query report
    /// </summary>
    public RelayCommand<object> RunReportCommand { get; private set; }

    /// <summary>
    /// The command that removes the query
    /// </summary>
    public RelayCommand RemoveCommand { get; private set; }

    /// <summary>
    /// A visibility, indicating whether this item can be renamed
    /// </summary>
    public Visibility RenameVisibility
    {
      get { return CanRename ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// A visibility, indicating whether this item can be removed
    /// </summary>
    public Visibility RemoveVisibility
    {
      get { return CanRemove ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// The event to raise when requesting to remove this query
    /// </summary>
    internal event RequestRemoveRequeryDelegate RequestRemoveQuery;

    /// <summary>
    /// Setup the commands
    /// </summary>
    private void SetupCommands()
    {
      RunCommand = new RelayCommand(RunQuery, () => CanRun);
      RemoveCommand = new RelayCommand(RemoveQuery, () => CanRemove);

      RunReportCommand = new RelayCommand<object>
      ((context) =>
      {
        var model = context as ReportViewModel;

        if (model != null && model.CanRun)
        {
          model.Run(true, true);

          // Do Analytics Tracking
          LiteAnalyticsTracker.TrackReport(model, LiteAnalyticsTracker.Source.Queries);

          // Do Analytics Tracking
          LiteAnalyticsTracker.TrackReport(model, LiteAnalyticsTracker.Source.Details);
        }

      },
      (context) =>
      {
        var model = context as ReportViewModel;
        return model != null && model.CanRun;
      });

      RunExportCommand = new RelayCommand<object>
       ((context) =>
       {
         var model = context as ExportViewModel;

         if (model != null && model.CanRun)
         {
           model.Save();

           // Do Analytics Tracking
           LiteAnalyticsTracker.TrackExport(model, LiteAnalyticsTracker.Source.Queries);
         }
       },
       (context) =>
       {
         var model = context as ExportViewModel;
         return model != null && model.CanRun;
       });
    }

    /// <summary>
    /// Check the commands' enabled state
    /// </summary>
    private void CheckCommands()
    {
      RunCommand.RaiseCanExecuteChanged();
      RemoveCommand.RaiseCanExecuteChanged();
      RunReportCommand.RaiseCanExecuteChanged();
      RunExportCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Runs the query and publishes it on the Messenger
    /// </summary>
    internal void RunQuery()
    {
      if (CanRun)
      {
        // Determine the recipe
        var recipeCollection = ResultCollection();

        if (recipeCollection != null)
        {
          // Do Analytics Tracking
          LiteAnalyticsTracker.TrackQuery(this, LiteAnalyticsTracker.Source.Queries);

          // Publish the recipe for the rest of the application to pick up
          Messenger.Send(new LiteDisplayFeatureCollectionRequestMessage(this, recipeCollection));
        }
      }
    }

    /// <summary>
    /// Request to be removed
    /// </summary>
    private void RemoveQuery()
    {
      var handler = this.RequestRemoveQuery;
      if (handler != null)
      {
        handler(this);
      }
    }
    #endregion

    #region Property Changes
    /// <summary>
    /// Handles the running state change
    /// </summary>
    internal void HandleRunningStateChange(LiteActionRunningStateMessage runningStateChange)
    {
      if (runningStateChange.Source == this)
      {
        // We sent this request
        this.RunningStateDescription = runningStateChange.RunningStateDescription;
        this.IsRunning = runningStateChange.IsRunning;
      }
    }

    /// <summary>
    /// A property of the Reports has changed
    /// </summary>
    void Reports_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == ReportsViewModel.IsBusyPropertyName)
      {
        this.RunningStateDescription = (Reports.IsBusy) ? ApplicationResources.QueryCreatingReport : string.Empty;
        this.IsRunning = Reports.IsBusy;
      }
      else if (e.PropertyName == ReportsViewModel.HasReportsPropertyName)
      {
        RaisePropertyChanged(ReportsEnabledPropertyName);
        RaisePropertyChanged(ReportsVisibilityPropertyName);
      }
    }

    /// <summary>
    /// A property of the Exports has changed
    /// </summary>
    void Exports_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == ExportsViewModel.IsBusyPropertyName)
      {
        this.RunningStateDescription = (Exports.IsBusy) ? ApplicationResources.QueryCreatingExport : string.Empty;

        this.IsRunning = Exports.IsBusy;
      }
      else if (e.PropertyName == ExportsViewModel.HasExportsPropertyName)
      {
        RaisePropertyChanged(ExportsEnabledPropertyName);
        RaisePropertyChanged(ExportsVisibilityPropertyName);
      }
    }
    #endregion

    #region Check State
    /// <summary>
    /// Checks the enabled state of the query; this state will be checked whenever
    /// the system functionParameters have changed (map extent, selection, etc.), as 
    /// the query can dependent on these functionParameters' availability
    /// </summary>
    internal void CalculateRecipe()
    {
      // Indicate whether we can run
      CanRun = Parameters.IsComplete;

      // Set up the contents for the recipe for the Query (definition)
      var recipe = this.ResultCollection();

      // For reports and exports
      this.Reports.SetupFor(recipe);

      // For export, make sure we don't export more than allowed
      if (recipe != null)
      {
        recipe = recipe.Take(LiteClientSettingsViewModel.Instance.ExportMaximumRecords);
      }

      this.Exports.SetupFor(recipe);

      CheckCommands();
    }
    #endregion

    #region Reports
    /// <summary>
    /// Holds the Reports for this definition; automatically setting
    /// its contents dependent on this query 
    /// </summary>
    public ReportsViewModel Reports
    {
      get;
      private set;
    }

    /// <summary>
    /// Can the lot be exported
    /// </summary>
    public bool ReportsEnabled
    {
      get { return CanRun; }
    }

    /// <summary>
    /// The availability of the reports
    /// </summary>
    public Visibility ReportsVisibility
    {
      get { return Reports.Reports.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion

    #region Exports
    /// <summary>
    /// Holds the Exports for this definition; automatically setting
    /// its contents dependent on this query 
    /// </summary>
    public ExportsViewModel Exports
    {
      get;
      private set;
    }

    /// <summary>
    /// Can the lot be exported
    /// </summary>
    public bool ExportsEnabled
    {
      get { return CanRun; }
    }

    /// <summary>
    /// The availability of the reports
    /// </summary>
    public Visibility ExportsVisibility
    {
      get { return Exports.Exports.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion

    #region Properties
    /// <summary>
    /// The parameterName of the (underlying) query
    /// </summary>
    public string Name
    {
      get { return Query.Name; }
    }

    /// <summary>
    /// The external parameterName of the (underlying) query
    /// </summary>
    public string ExternalName
    {
      get
      {
        return Query.ExternalName;
      }
      set
      {
        if (CanRename)
        {
          Query.ExternalName = value;
          RaisePropertyChanged(ExternalNamePropertyName);
        }
      }
    }

    /// <summary>
    /// The functionParameters (as parameterViewModels) that hold the user values for
    /// the query, allowing to be changed by the user
    /// </summary>
    public ParameterValuesViewModel Parameters
    {
      get { return _parameters; }
      private set
      {
        if (_parameters != value)
        {
          if (_parameters != null)
          {
            _parameters.PropertyChanged -= _parameters_PropertyChanged;
          }

          _parameters = value;

          if (_parameters != null)
          {
            _parameters.PropertyChanged += _parameters_PropertyChanged;
          }

          RaisePropertyChanged(ParametersPropertyName);
        }
      }
    }

    /// <summary>
    /// The property changed
    /// </summary>
    void _parameters_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      // Calculate the recipe
      CalculateRecipe();
    }

    /// <summary>
    /// Gets a list of configrationitems
    /// </summary>
    public List<Object> ConfigurationItems
    {
      get
      {
        var result = new List<object>();

        if (this.Parameters.Count > 0)
        {
          result.Add(this);
        }

        return result;
      }
    }

    /// <summary>
    /// Is the query enabled (can it be run)
    /// </summary>
    public bool CanRun
    {
      get { return _canRun && !_isRunning; }
      set
      {
        if (value != _canRun)
        {
          _canRun = value;
          RaisePropertyChanged(CanRunPropertyName);
          RaisePropertyChanged(ReportsEnabledPropertyName);
          RaisePropertyChanged(ExportsEnabledPropertyName);
          CheckCommands();
        }
      }
    }

    /// <summary>
    /// Is this queryViewModel busy retrieving its result collection
    /// </summary>
    public bool IsRunning
    {
      get { return _isRunning; }
      set
      {
        if (_isRunning != value)
        {
          _isRunning = value;
          RaisePropertyChanged(IsRunningPropertyName);
          RaisePropertyChanged(IsRunningVisibilityPropertyName);
          RaisePropertyChanged(CanRunPropertyName);
          RaisePropertyChanged(ReportsEnabledPropertyName);
          RaisePropertyChanged(ExportsEnabledPropertyName);
          CheckCommands();
        }
      }
    }

    /// <summary>
    /// Gets or Sets the running state description
    /// </summary>
    public string RunningStateDescription
    {
      get { return _runningStateDescription; }
      set
      {
        if (_runningStateDescription != value)
        {
          _runningStateDescription = value;
          RaisePropertyChanged(RunningStateDescriptionPropertyName);
        }
      }
    }

    /// <summary>
    /// Visibility dependent on the state whether the the query is running
    /// </summary>
    public Visibility IsRunningVisibility
    {
      get { return (IsRunning) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Is the query expanded
    /// </summary>
    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        if (value != _isExpanded)
        {
          _isExpanded = value;
          RaisePropertyChanged(IsExpandedPropertyName);
          RaisePropertyChanged(IsExpandedVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Is the query view visible (expanded)
    /// </summary>
    public Visibility IsExpandedVisibility
    {
      get { return (IsExpanded) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Holds the path geometry
    /// </summary>
    public string PathGeometry
    {
      get;
      private set;
    }

    /// <summary>
    /// The underlying model of the query
    /// </summary>
    public FeatureCollectionQueryDefinition Query { get; private set; }

    /// <summary>
    /// The predicate text
    /// </summary>
    public string Predicate { get; private set; }

    /// <summary>
    /// Can the queryViewModel be renamed
    /// </summary>
    public bool CanRename
    {
      get { return Query.Context == ServiceProviderDatumContext.User; }
    }

    /// <summary>
    /// Can the queryViewModel be removed
    /// </summary>
    public bool CanRemove
    {
      get { return Query.Context == ServiceProviderDatumContext.User; }
    }

    /// <summary>
    /// Handles the collection restriction before evaluating the result
    /// </summary>
    /// <param name="collection">A collection to further restrict</param>
    /// <returns>A further restricted collection</returns>
    private FeatureCollection RestrictedCollection(FeatureCollection collection)
    {
      if (collection != null && LiteClientSettingsViewModel.Instance.QueriesUseRestrictionAreas)
      {
        // Check whether we can restrict the collection
        var message = new LiteRestrictFeatureCollectionRequestMessage(this, collection);

        // Ask other viewModels via the messenger
        Messenger.Send(message);

        // Get the restricted collection from the message
        collection = message.RestrictedCollection;
      }

      return collection;
    }

    /// <summary>
    /// The current recipe holder
    /// </summary>
    /// <returns>The collection that represents the query result</returns>
    public FeatureCollection ResultCollection()
    {
      // The result collection to build up
      FeatureCollection resultCollection = null;

      // Get client side queries happening from the client
      if (Query.Context == ServiceProviderDatumContext.User)
      {
        // For now do it via a client side request
        var predicateText = this.Predicate ?? string.Empty;
        var recipe = new FeatureCollectionRecipeByCollectionDefinition(this.Query.TableDescriptor, this.Parameters.ParameterValues, predicateText)
        {
          Description = this.ExternalName
        };

        resultCollection = new FeatureRecipeCollection(recipe);
      }
      else
      {
        // Server query
        resultCollection = this.Query.ToCollection(this.Parameters.ParameterValues);
      }

      // In case the user wants to further restrict these, ask the messenger for restricting
      resultCollection = RestrictedCollection(resultCollection);

      return resultCollection;
    }
    #endregion
  }
}
