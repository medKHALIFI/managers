using System;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.Generic;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Features.Recipe;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The Result ViewModel, holding all logic for the Lite Collection Grid.
  /// Note that this implementation uses a FeatureCollectionGridViewModel, with some properties
  /// delegating behavior to this gridViewModel. Compare with the sample implementation of the
  /// LiteFeatureDetailsViewModel that actually inherits from the ClientToolkit's one (which is
  /// the FeatureDetailsViewModel).
  /// </summary>
  public class LiteFeatureCollectionResultViewModel : ViewModelBase
  {
    #region Static Property Names
    /// <summary>
    /// The active feature collection being display 
    /// </summary>
    public const string FeatureCollectionPropertyName = "FeatureCollection";

    /// <summary>
    /// The selected feature of the underlying gridView
    /// </summary>
    public const string SelectedFeaturePropertyName = "SelectedFeature";

    /// <summary>
    /// The batch provider holding all information on batches to retrieve
    /// for the current feature collection
    /// </summary>
    public const string BatchProviderPropertyName = "BatchProvider";

    /// <summary>
    /// Change in Result Description, holding the name of the collection (recipe)
    /// </summary>
    public const string ResultDescriptionPropertyName = "ResultDescription";

    /// <summary>
    /// The description holding the number of records
    /// </summary>
    public const string NumberOfRecordsDescriptionPropertyName = "NumberOfRecordsDescription";

    /// <summary>
    /// Are the reports enabled
    /// </summary>
    public const string ReportsEnabledPropertyName = "ReportsEnabled";

    /// <summary>
    /// Is there an export enabled
    /// </summary>
    public const string ExportsEnabledPropertyName = "ExportsEnabled";

    /// <summary>
    /// The requested expanded state of the result list
    /// </summary>
    public const string RequestActivationExpandedPropertyName = "RequestActivationExpanded";

    /// <summary>
    /// The visibility (requested visibility) of the result list
    /// </summary>
    public const string RequestActivationVisibilityPropertyName = "RequestActivationVisibility";

    /// <summary>
    /// The visibility for the sub collections; only appropriate in case 
    /// a heterogeneous collections is being displayed. This allows selection 
    /// of the (homogeneous) sub collections for 'drilling down'
    /// </summary>
    public const string SubCollectionsVisibilityPropertyName = "SubCollectionsVisibility";

    /// <summary>
    /// The sub collections in case of a heterogeneous collection being shown.
    /// This holds the homogenenous sub collections.
    /// </summary>
    public const string SubCollectionsPropertyName = "SubCollections";

    /// <summary>
    /// The selected sub collection of a heterogeneous collection.
    /// </summary>
    public const string SelectedSubCollectionPropertyName = "SelectedSubCollection";

    #endregion

    #region Helper Class for Sub Collections
    /// <summary>
    /// A helper class for representing sub collections of heterogeneous collections
    /// </summary>
    public class SubCollectionViewModel
    {
      /// <summary>
      /// The description of a sub collection
      /// </summary>
      public string Description { get; set; }

      /// <summary>
      /// The actual sub collection
      /// </summary>
      public InMemoryFeatureCollection Collection { get; set; }

      /// <summary>
      /// Returns the description of the sub collection as a representation
      /// of the sub collection
      /// </summary>
      public override string ToString()
      {
        return Description;
      }
    }
    #endregion

    #region Static values
    /// <summary>
    /// The maximum batch size
    /// </summary>
    private static int MaximumBatchSize = 100000;

    /// <summary>
    /// The batch size to resort to when the maximum batch size has been set 
    /// in the properties of this grid.
    /// </summary>
    private static int DefaultBatchSizeIfMaximumBatchSize = 100;
    #endregion

    #region Fields
    /// <summary>
    /// The feature collection to be displayed in the result list
    /// </summary>
    private FeatureCollection _featureCollection;

    /// <summary>
    /// The recipe that defines how the active collection is built up
    /// </summary>
    private FeatureCollectionRecipe _featureCollectionRecipe;

    /// <summary>
    /// The batch provider that has been set up to retrieve the (current batch of)
    /// records of the feature collection
    /// </summary>
    private FeatureCollectionBatchProvider _batchProvider;

    /// <summary>
    /// The requested visibility-state
    /// </summary>
    private Visibility _requestActivationVisibility = Visibility.Collapsed;

    /// <summary>
    /// The requested expanded state
    /// </summary>
    private bool _requestActivationExpanded;

    /// <summary>
    /// The sub collections
    /// </summary>
    private IList<SubCollectionViewModel> _subCollections;

    /// <summary>
    /// The selected sub collection
    /// </summary>
    private SubCollectionViewModel _selectedSubCollection;
    #endregion

    #region Constructors
    /// <summary>
    /// Standard constructor for the collection result view model
    /// </summary>
    public LiteFeatureCollectionResultViewModel(Messenger messenger = null)
      : base(messenger)
    {
      AttachToMessenger();

      SetupCommands();

      this.Properties = new LiteFeatureCollectionResultViewModelProperties();
      this.Properties.PropertyChanged += Properties_PropertyChanged;

      // set up the underlying GridViewModel
      this.GridViewModel = new FeatureCollectionGridViewModel(messenger);
      this.GridViewModel.PropertyChanged += GridViewModel_PropertyChanged;
      this.GridViewModel.FeatureValueActivated += HandleFeatureValueActivated;

      this.GroupingViewModel = new FeatureCollectionGridGroupingViewModel(this.GridViewModel);
      this.GroupingViewModel.AutoSort = true;

      // Set up the reports
      this.Reports = new ReportsViewModel(messenger) { AutoGetAvailableDefinitions = true };
      this.Reports.PropertyChanged += Reports_PropertyChanged;

      // Set up the exports
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

      // Set up a history of recipes that can be reactived to retrieve a previous/next collection
      this.History = new HistoryViewModel<FeatureCollection>(f => f.CollectionRecipe.Description);
      this.History.PropertyChanged += History_PropertyChanged;

      // Create an empty batch provider to allow binding to its commands
      this.BatchProvider = new FeatureCollectionBatchProvider();

      // Set up the resources for binding purposes
      this.Resources = new Lite.Resources.Localization.ApplicationResources();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach the result-ViewModel to the messenger to handle display request.
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<LiteDisplayFeatureCollectionRequestMessage>(this, HandleDisplayCollectionRequest);
      }
    }

    /// <summary>
    /// Called whenever a Display Collection Request is put on the messenger (databus); 
    /// this method is called and the display of the collection is initiated
    /// </summary>
    /// <param name="request"></param>
    private void HandleDisplayCollectionRequest(LiteDisplayFeatureCollectionRequestMessage request)
    {
      if (request != null)
      {
        var collection = request.FeatureCollection;

        if (collection != null)
        {
          // Make sure we set the result only when there is a result message
          SetupForCollection(collection, request.Sender);
        }
      }
    }

    /// <summary>
    /// Is called whenever the activation of an (activatable) value is carried out.
    /// Transforms the internal request (by GridViewModel) into an external request,
    /// by driving the appropriate Messenger bits of the Messenger value API.
    /// </summary>
    void HandleFeatureValueActivated(object sender, FeatureValueActivatedEventArgs e)
    {
      Messenger.Send(new LiteActivateFeatureValueRequestMessage(this, e.Feature, e.Field, e.Value));
    }
    #endregion

    #region Setup

    /// <summary>
    /// Resets the viewmodel
    /// </summary>
    private void ResetViewModel()
    {
      // Clear the model
      this.SelectedFeature = null;
      this.History.Clear();
      this.FeatureCollection = null;

      // Hide the view
      this.RequestActivationVisibility = Visibility.Collapsed;
      this.RequestActivationExpanded = false;
    }

    #endregion

    #region Commands
    /// <summary>
    /// Gets the command to run the report
    /// </summary>
    public RelayCommand<object> RunReportCommand { get; private set; }

    /// <summary>
    /// Gets the command to run the export
    /// </summary>
    public RelayCommand<object> RunExportCommand { get; private set; }

    /// <summary>
    /// Setup the viewmodel commands
    /// </summary>
    private void SetupCommands()
    {
      // Create the command to run a report on the active collection
      RunReportCommand = new RelayCommand<object>
        ((reportToRun) =>
        {
          var model = reportToRun as ReportViewModel;

          if (model != null && model.CanRun)
          {
            // Do Analytics Tracking
            LiteAnalyticsTracker.TrackReport(model, LiteAnalyticsTracker.Source.ResultList);

            model.Run(true, true);
          }
        },
        (context) =>
        {
          var model = context as ReportViewModel;
          return model != null;
        }
      );

      // Create the command to run an export for the active collection
      RunExportCommand = new RelayCommand<object>
       ((exportToRun) =>
       {
         var model = exportToRun as ExportViewModel;

         if (model != null)
         {
           // Do Analytics Tracking
           LiteAnalyticsTracker.TrackExport(model, LiteAnalyticsTracker.Source.ResultList);

           model.Save();
         }
       },
       (context) =>
       {
         var model = context as ExportViewModel;
         return model != null && model.CanRun;
       }
     );
    }

    /// <summary>
    /// Check the execution of the Report and Export commands
    /// </summary>
    private void CheckCommands()
    {
      RunReportCommand.RaiseCanExecuteChanged();
      RunExportCommand.RaiseCanExecuteChanged();
    }
    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Callback when the authentication is changed
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Reset the viewmodel when authenticated, can also be done upon de-authentication
        // doing on both will send unnecessary messages
        ResetViewModel();
      }
    }

    /// <summary>
    /// Callback for changes in Reports for the current collection
    /// </summary>
    private void Reports_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == ReportsViewModel.IsBusyPropertyName)
      {
        RaisePropertyChanged(ReportsEnabledPropertyName);
      }
      else if (e.PropertyName == ReportsViewModel.HasReportsPropertyName)
      {
        RaisePropertyChanged(ReportsEnabledPropertyName);
      }
    }

    /// <summary>
    /// Callback for changes in (target) export definitions for the current collection
    /// </summary>
    void Exports_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == ExportsViewModel.IsBusyPropertyName)
      {
        RaisePropertyChanged(ExportsEnabledPropertyName);
      }
      else if (e.PropertyName == ExportsViewModel.HasExportsPropertyName)
      {
        RaisePropertyChanged(ExportsEnabledPropertyName);
      }
    }

    /// <summary>
    /// Callback for changes in (properties of) the underlying GridViewModel
    /// </summary>
    void GridViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == FeatureCollectionGridViewModel.HierarchySelectedFeaturePropertyName)
      {
        // The underlying selected feature has changed; notify the outside world of the fact
        RaisePropertyChanged(SelectedFeaturePropertyName);

        if (SelectedFeature != null)
        {
          // There is a feature
          if (Properties.TrackSelectionInMap)
          {
            // The user wants to track the feature in the map; send a request on the messenger
            var envelope = this.SelectedFeature.GetEnvelope();
            if (envelope != null)
            {
              this.Messenger.Send(new LiteGoToGeometryRequestMessage(this, envelope, SelectedFeature) { DoRocketJump = false });
            }
          }

          if (Properties.HighlightSelectionInMap)
          {
            this.Messenger.Send(new LiteHighlightGeometryRequestMessage(this, SelectedFeature));
          }

          if (Properties.TrackSelectionInFeatureDetails)
          {
            // The user wants to track the feature in the Details view; send a request on the messenger
            this.Messenger.Send(new LiteDisplayFeatureDetailsRequestMessage(this, this.SelectedFeature));
          }
        }
      }
      else if (e.PropertyName == FeatureCollectionGridViewModel.HierarchyFeatureCollectionPropertyName)
      {
        // A new collection has been created
        SetFeatureCollectionUsageFromHierarchy();
      }
    }

    /// <summary>
    /// A property of our Properties View Model has changed; ie, driving which records are to be exported/reported
    /// </summary>
    void Properties_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == LiteFeatureCollectionResultViewModelProperties.ExportReportElementsModePropertyName ||
        e.PropertyName == LiteFeatureCollectionResultViewModelProperties.ExportReportTrackModePropertyName)
      {
        // The properties that indicate which records are to be exported/reported have changed
        SetFeatureCollectionUsageFromHierarchy();
      }
      else if (e.PropertyName == LiteFeatureCollectionResultViewModelProperties.TrackSelectionInFeatureDetailsPropertyName)
      {
        // The Track Selection in Details has changed; in case this now is true, immediately send a request
        if (Properties.TrackSelectionInFeatureDetails && SelectedFeature != null)
        {
          // Let's immediately notify the world
          this.Messenger.Send(new LiteDisplayFeatureDetailsRequestMessage(this, this.SelectedFeature));
        }
      }
      else if (e.PropertyName == LiteFeatureCollectionResultViewModelProperties.TrackSelectionInMapPropertyName)
      {
        // The Track Selection in Map has changed; in case this now is true, immediately send a request
        if (Properties.TrackSelectionInMap && SelectedFeature != null)
        {
          // Send a request for jumping to (the envelope of) the selected feature
          var envelope = this.SelectedFeature.GetEnvelope();
          if (envelope != null)
          {
            var request = new LiteGoToGeometryRequestMessage(this, envelope, SelectedFeature) { DoRocketJump = false };
            this.Messenger.Send(request);
          }
        }
      }
      else if (e.PropertyName == LiteFeatureCollectionResultViewModelProperties.HighlightSelectionInMapPropertyName)
      {
        // The Highlight Selection has changed; in case this now is true, immediately send a request
        if (Properties.HighlightSelectionInMap && SelectedFeature != null)
        {
          this.Messenger.Send(new LiteHighlightGeometryRequestMessage(this, SelectedFeature));
        }
      }
    }

    /// <summary>
    /// A property of the underlying batch provider has changed. Dependent on the type of property, react accordingly
    /// </summary>
    void BatchProvider_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == FeatureCollectionBatchProvider.IsRunningPropertyName)
      {
        Messenger.Send(new LiteActionRunningStateMessage(this, this.BatchProvider.Source, this.BatchProvider.IsRunning, ApplicationResources.FeatureResultListRetrievingCollection));
      }
      else if (e.PropertyName == FeatureCollectionBatchProvider.CurrentBatchFeaturesPropertyName)
      {
        // Set the active collection to our grid view model
        var mainFeatures = BatchProvider.CurrentBatchFeatures;
        GridViewModel.FeatureCollection = mainFeatures;

        // Tell the outside world about the new collection in the grid
        RaisePropertyChanged(ResultDescriptionPropertyName);
        RaisePropertyChanged(NumberOfRecordsDescriptionPropertyName);

        if (BatchProvider.IsMain)
        {
          // This is the top level batch; which means that we can actively look at sub collections
          // of a heterogeneous collection
          List<SubCollectionViewModel> subCollections = null;

          var activeFeatures = BatchProvider.CurrentBatchFeatures;
          if (activeFeatures != null && activeFeatures.Uniformity.IsHeterogeneous)
          {
            var allSubCollections = activeFeatures.HomogeneousCollections;
            if (allSubCollections != null && allSubCollections.Count > 0)
            {
              // Set up the sub collections
              subCollections = new List<SubCollectionViewModel>();

              // Add the main representation
              subCollections.Add(new SubCollectionViewModel { Description = ApplicationResources.HeterogeneousMainCollectionDescription, Collection = mainFeatures });
              foreach (InMemoryFeatureCollection sub in allSubCollections)
              {
                var description = string.Format(ApplicationResources.HeterogeneousSubCollectionDescription, sub.TableDescriptor.ExternalName);
                subCollections.Add(new SubCollectionViewModel { Description = description, Collection = sub });
              }
            }
          }

          // Set the sub collections; the sub collections will be set up in case
          // the main collection is heterogeneous. This allows navigation to the
          // individual homogeneous sub collections of this heterogeneous collection
          SubCollections = subCollections;
        }
      }
    }

    /// <summary>
    /// A property of the History (holder) has changed; if this means that the current element has changed,
    /// set the active collection to it (or its recipe)
    /// </summary>
    void History_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == HistoryViewModel.CurrentPropertyName)
      {
        try
        {
          SettingFeatureCollectionFromHistoryOrSub = true;

          this.SetupForCollection(this.History.Current);
        }
        finally
        {
          SettingFeatureCollectionFromHistoryOrSub = false;
        }
      }
    }
    #endregion

    #region History
    /// <summary>
    /// Indicates whether we are setting the current feature from the history
    /// </summary>
    private bool SettingFeatureCollectionFromHistoryOrSub
    {
      get;
      set;
    }

    /// <summary>
    /// The viewModel that takes care of the history of elements
    /// </summary>
    public HistoryViewModel<FeatureCollection> History
    {
      get;
      private set;
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
    /// Can the lot be exported; a property for easy binding
    /// </summary>
    public bool ReportsEnabled
    {
      get { return this.FeatureCollection != null && Reports.Reports.Count > 0 && !Reports.IsBusy; }
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
    /// Can the lot be exported; a property for easy binding
    /// </summary>
    public bool ExportsEnabled
    {
      get { return this.FeatureCollection != null && Exports.Exports.Count > 0 && !Exports.IsBusy; }
    }
    #endregion

    #region FeatureGrid
    /// <summary>
    /// The gridViewModel, holding the logic/data for the grid to be displayed. This whole class is
    /// basically a wrapper around the gridViewModel with some extra behavior for Exports/Reports as
    /// well as grouping
    /// </summary>
    public FeatureCollectionGridViewModel GridViewModel
    {
      get;
      private set;
    }

    /// <summary>
    /// The groupingViewModel that drives the grouping mechanism for our underlying grid ViewModel
    /// </summary>
    public FeatureCollectionGridGroupingViewModel GroupingViewModel
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns the active collection to be exported/reported on. What the exact active
    /// collection is, depends on the underlying settings. 
    /// </summary>
    /// <returns>A collection (better, a holder of a feature collection recipe)</returns>
    private IFeatureCollectionRecipeHolder ActiveReportExportFeatureCollection(out bool isEmpty)
    {
      isEmpty = false;

      if (Properties.ElementsUseSelectedFeature)
      {
        return GridViewModel.GetSelectedFeature(Properties.TrackUseFocused);
      }

      var activeGridCollection = GridViewModel.GetFeatureCollection(this.Properties.TrackUseFocused);

      if (activeGridCollection != null && activeGridCollection.Count > 0 && Properties.ElementsUseFull && activeGridCollection == GridViewModel.FeatureCollection)
      {
        // We want to use the full recipe in case of main and in this case, the active grid collection is the main collection.
        return _featureCollection;
      }

      // Check the isEmpty
      isEmpty = activeGridCollection.Count == 0;

      // Inall other cases, just return the active grid collection
      return activeGridCollection;
    }

    /// <summary>
    /// Set the active collection (as calculated above) on the Reports and Exports view models.
    /// </summary>
    private void SetFeatureCollectionUsageFromHierarchy()
    {
      bool isEmpty;
      var activeCollection = ActiveReportExportFeatureCollection(out isEmpty);

      this.Reports.SetupFor(activeCollection);

      // For export, make sure we don't export more than allowed
      if (isEmpty)
      {
        // Just set the empty collection
        this.Exports.SetupFor(activeCollection);
      }
      else
      {
        var exportRecipe = activeCollection != null ? activeCollection.CollectionRecipe : null;
        if (exportRecipe != null)
        {
          exportRecipe = exportRecipe.Take(LiteClientSettingsViewModel.Instance.ExportMaximumRecords);
        }
        this.Exports.SetupFor(exportRecipe);
      }


      // Request to be visible
      RequestActivationExpanded = true;
      RequestActivationVisibility = Visibility.Visible;
    }
    #endregion

    #region Batch Handling
    /// <summary>
    /// The (current) batch provider that carries out the retrieval of the active
    /// records and can potentially move through the result-collection in a batched way
    /// </summary>
    public FeatureCollectionBatchProvider BatchProvider
    {
      get { return _batchProvider; }
      private set
      {
        if (_batchProvider != value)
        {
          if (_batchProvider != null)
          {
            _batchProvider.PropertyChanged -= BatchProvider_PropertyChanged;
          }

          _batchProvider = value;

          if (_batchProvider != null)
          {
            _batchProvider.PropertyChanged += BatchProvider_PropertyChanged;
          }

          RaisePropertyChanged(BatchProviderPropertyName);
        }
      }
    }
    #endregion

    #region FeatureCollectionRecipe handling
    /// <summary>
    /// Sets up the feature collection for the specified recipe
    /// </summary>
    private Task SetupForCollection(FeatureCollection collection)
    {
      return SetupForCollection(collection, this);
    }

    /// <summary>
    /// Setup the contents of this list view using the specified collection.
    /// The specified source is the viewModel that originated the display of the collection.
    /// </summary>
    private Task SetupForCollection(FeatureCollection collection, object source, bool isMain = true)
    {
      _featureCollection = collection;
      _featureCollectionRecipe = _featureCollection != null ? _featureCollection.CollectionRecipe : null;

      if (!SettingFeatureCollectionFromHistoryOrSub && _featureCollection != null)
      {
        this.History.Add(_featureCollection);
      }

      // Only handle recipes that actually have a result table descriptor
      if (_featureCollectionRecipe == null)
      {
        // There is no spoon - do nothing, though we could choose to empty things
        // GridViewModel.FeatureCollection = null;
      }
      else
      {
        // Do batch wise
        return SetupBatchProviderFor(_featureCollectionRecipe, isMain, source);
      }

      // Return a simple task (use the Boolean generic task subclass - these are automatically cached and reused)
      return TaskEx.FromResult(true);
    }

    /// <summary>
    /// Sets up the batch provider for the specified recipe
    /// </summary>
    /// <param name="recipe">The recipe the provider must evaluate</param>
    /// <param name="isMain">Indicates whether this is the top level collection,
    /// otherwise a sub collection of a heterogeneous collection</param>
    /// <param name="source">The source that requested evaluated of the recipe</param>
    /// <param name="batchSize">The batch size</param>
    private Task SetupBatchProviderFor(FeatureCollectionRecipe recipe, bool isMain = true, object source = null, int batchSize = -1)
    {
      // Get the default batch size happening
      if (batchSize == -1)
      {
        batchSize = this.BatchSize;
      }

      // Set up the batch provider for the specified recipe
      BatchProvider = new FeatureCollectionBatchProvider(_featureCollectionRecipe, this.BatchSize)
      {
        // The source that activated this evaluation
        Source = source,

        // Indicates whether this is a top-level collection or a sub collection
        // of a heterogeneous collection
        IsMain = isMain,

        // Set the logger of the batch provider
        Logger = Logger
      };

      // Return the task that takes care of refreshing the current batch
      // This can be awaited by the consumer
      return BatchProvider.RefreshCurrentBatch();
    }

    /// <summary>
    /// Returns the batch-size for the active collection
    /// </summary>
    private int BatchSize
    {
      get
      {
        // Set up the default batch size
        int batchSize = DefaultBatchSizeIfMaximumBatchSize;

        // In case there is a recipe, try to determine a batch size for its table descriptor
        // This allows batch sizes to be set programmatically for each table
        if (_featureCollectionRecipe != null)
        {
          // Get the table properties set up for the active table descriptor
          var tableProperties = this.TablePropertiesCache.TablePropertiesFor(_featureCollectionRecipe.TableDescriptor);

          // Determine a batch size for this table properties (defaulting to our default batch size)
          batchSize = tableProperties.BatchSize >= 1 ? tableProperties.BatchSize : GridProperties.DefaultBatchSize;

          // Ensure there actually is a decent batch size; making sure it doesn't exceed the maximum batch size
          // as was set up in our view model
          if (batchSize < 0 || batchSize >= MaximumBatchSize)
          {
            batchSize = DefaultBatchSizeIfMaximumBatchSize;
          }
        }

        return batchSize;
      }
    }
    #endregion

    #region Sub Collection handling
    /// <summary>
    /// Exposes the visibility of the sub collections, for easy binding purposes
    /// </summary>
    public Visibility SubCollectionsVisibility
    {
      get { return _subCollections != null && _subCollections.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// The sub collections of a heterogeneous collection
    /// </summary>
    public IList<SubCollectionViewModel> SubCollections
    {
      get { return _subCollections; }
      set
      {
        if (_subCollections != value)
        {
          _subCollections = value;
          RaisePropertyChanged(SubCollectionsPropertyName);

          // Make sure we notify any visibility for sub collections from here
          RaisePropertyChanged(SubCollectionsVisibilityPropertyName);

          // Set the active sub collection explicitly
          _selectedSubCollection = _subCollections != null && _subCollections.Count > 0 ? _subCollections[0] : null;
          RaisePropertyChanged(SelectedSubCollectionPropertyName);
        }
      }
    }

    /// <summary>
    /// The selected sub collection of a heterogeneous collection
    /// </summary>
    public SubCollectionViewModel SelectedSubCollection
    {
      get { return _selectedSubCollection; }
      set
      {
        if (_selectedSubCollection != value)
        {
          _selectedSubCollection = value;

          if (_selectedSubCollection != null && _selectedSubCollection.Collection != null)
          {
            try
            {
              SettingFeatureCollectionFromHistoryOrSub = true;

              this.SetupForCollection(_selectedSubCollection.Collection, null, false);
            }
            finally
            {
              SettingFeatureCollectionFromHistoryOrSub = false;
            }
          }

          RaisePropertyChanged(SelectedSubCollectionPropertyName);
        }
      }
    }
    #endregion

    #region Main API
    /// <summary>
    /// Holds the active collection that is currently displayed in the Grid View (Model)
    /// </summary>
    public FeatureCollection FeatureCollection
    {
      get { return _featureCollection; }
      set
      {
        if (_featureCollection != value)
        {
          this.SetupForCollection(value);

          RaisePropertyChanged(FeatureCollectionPropertyName);
        }
      }
    }

    /// <summary>
    /// Returns the selected feature of the gridViewModel
    /// </summary>
    public Feature SelectedFeature
    {
      get { return this.GridViewModel.GetSelectedFeature(Properties.TrackUseFocused); }
      set { this.GridViewModel.SelectedFeature = value; }
    }

    /// <summary>
    /// Holds the Properties (of the underlying gridViewModel)
    /// </summary>
    public LiteFeatureCollectionResultViewModelProperties Properties
    {
      get;
      private set;
    }

    /// <summary>
    /// Table Properties Cache, holding properties for tables that displayed in the grid
    /// </summary>
    public FeatureBaseViewModelTablePropertiesCache<FeatureCollectionGridViewModelTableProperties> TablePropertiesCache
    {
      get { return GridViewModel.TablePropertiesCache; }
    }

    /// <summary>
    /// The properties of the grid
    /// </summary>
    public FeatureCollectionGridViewModelProperties GridProperties
    {
      get { return GridViewModel.Properties; }
    }

    /// <summary>
    /// The table properties for the active table
    /// </summary>
    public FeatureCollectionGridViewModelTableProperties TableProperties
    {
      get { return GridViewModel.HierarchyTableProperties; }
    }

    /// <summary>
    /// The result description
    /// </summary>
    public string ResultDescription
    {
      get { return _featureCollectionRecipe != null ? _featureCollectionRecipe.Description : string.Empty; }
    }

    /// <summary>
    /// A description 
    /// </summary>
    public string NumberOfRecordsDescription
    {
      get
      {
        var count = BatchProvider.TotalRecordsFound;
        var records = count == 1 ? " Record" : " Records";
        return String.Concat(count.ToString(), records);
      }
    }

    /// <summary>
    /// Holds the requested visibility for the result view model; is being used to 
    /// drive the visibility of this component on the UI
    /// </summary>
    public Visibility RequestActivationVisibility
    {
      get { return _requestActivationVisibility; }
      set
      {
        if (_requestActivationVisibility != value)
        {
          _requestActivationVisibility = value;
          RaisePropertyChanged(RequestActivationVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// This elements (requested) expanded state; is set to reflect the
    /// fact that contents have been set up
    /// </summary>
    public Boolean RequestActivationExpanded
    {
      get { return _requestActivationExpanded; }
      set
      {
        if (_requestActivationExpanded != value)
        {
          _requestActivationExpanded = value;
          RaisePropertyChanged(RequestActivationExpandedPropertyName);
        }
      }
    }
    #endregion
  }
}
