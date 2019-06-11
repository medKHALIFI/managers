using System;
using System.Windows;
using System.Collections.Generic;
using System.Threading.Tasks;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry.CoordinateSystems;
using SpatialEye.Framework.Geometry;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// The Details ViewModel, holding all logic for the Lite Feature Details Properties Grid.
  /// Note that this implementation inherits from the FeatureDetailsViewModel, automatically
  /// exposing all its properties to the outside world from this viewModel as well.
  /// Compare this with the implementation of the LiteFeatureCollectionResultViewModel, where
  /// the clientToolkit's FeatureCollectionGridViewModel is not inherited from but used internally
  /// to delegate behavior to.
  /// </summary>
  public class LiteFeatureDetailsViewModel : FeatureDetailsViewModel
  {
    #region Static Property Names
    /// <summary>
    /// Are the reports enabled
    /// </summary>
    public const string ReportsEnabledPropertyName = "ReportsEnabled";

    /// <summary>
    /// Are the reports visible 
    /// </summary>
    public const string ReportsVisibilityPropertyName = "ReportsVisibility";

    /// <summary>
    /// Are the feature details running
    /// </summary>
    public const string IsRunningPropertyName = "IsRunning";

    /// <summary>
    /// The running state description of the feature details
    /// </summary>
    public const string RunningStateDescriptionPropertyName = "RunningStateDescription";

    /// <summary>
    /// The (helper) is running visibility
    /// </summary>
    public const string IsRunningVisibilityPropertyName = "IsRunningVisibility";

    /// <summary>
    /// Is the details view model active (or at least should it be), meaning visible to the user
    /// </summary>
    public const string ViewIsActivePropertyName = "ViewIsActive";

    /// <summary>
    /// The edit mode visibility, triggered when there is a feature which is editable
    /// </summary>
    public const string EditModeVisibilityPropertyName = "EditModeVisibility";

    /// <summary>
    /// The edit view visibility property name triggered when the edit mode is activated and there is any crud action possible
    /// </summary>
    public const string EditViewVisibilityPropertyName = "EditViewVisibility";

    /// <summary>
    /// The insert feature control visibility
    /// </summary>
    public const string InsertVisibilityPropertyName = "InsertVisibility";

    /// <summary>
    /// The update feature control visibility
    /// </summary>
    public const string UpdateVisibilityPropertyName = "UpdateVisibility";

    /// <summary>
    /// The delete feature control visibility
    /// </summary>
    public const string DeleteVisibilityPropertyName = "DeleteVisibility";

    /// <summary>
    /// The attach candidates property
    /// </summary>
    public const string AttachCandidatesPropertyName = "AttachCandidates";

    /// <summary>
    /// The attach candidates visibility property
    /// </summary>
    public const string AttachCandidatesVisibilityPropertyName = "AttachCandidatesVisibility";
    #endregion

    #region Fields
    /// <summary>
    /// A flag indicating whether the ui is busy retrieving some information
    /// </summary>
    private bool _isRunning;

    /// <summary>
    /// Holds the descriptive running state
    /// </summary>
    private string _runningStateDescription;

    /// <summary>
    /// Holds the active state of the view
    /// </summary>
    private bool _viewIsActive;

    /// <summary>
    /// The display coordinate system
    /// </summary>
    private EpsgCoordinateSystemReference _displayCS;

    /// <summary>
    /// Is the user allowed to create or modify features
    /// </summary>
    private bool _allowEdits;

    /// <summary>
    /// The attach candidates
    /// </summary>
    private IList<LiteFeatureDetailsViewModelAttachItem> _attachCandidates;

    /// <summary>
    /// Indicates whether values should be refreshed on a change in edit mode
    /// </summary>
    private bool _refreshValuesOnEditModeDeactivate = true;
    #endregion

    #region Constructors
    /// <summary>
    /// Default Constructors
    /// </summary>
    public LiteFeatureDetailsViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Attach to the messenger to react to requests for displaying feature details
      AttachToMessenger();

      // Set up the commands
      SetupCommands();

      // Set up the resources for easier binding
      this.Resources = new Lite.Resources.Localization.ApplicationResources();

      // The provider that determines the actual details that need to be displayed
      // for a features/set of features.
      this.FeatureDetailsProvider = new LiteFeatureDetailFeatureDetailsProvider();

      // Set up the history, to allow going to previous/next features in the details ui
      this.History = new HistoryViewModel<EditableFeature>(f => string.Format("{0}: {1}", f.TableDescriptor.ExternalName, f.ToString()));
      this.History.PropertyChanged += History_PropertyChanged;

      // Set up the reports view model, to handle report activation for an active feature
      this.Reports = new ReportsViewModel(messenger)
      {
        AllowJoinedReports = true,
        AutoGetAvailableDefinitions = true
      };

      // Handle property changes, to cascade/translate some of the properties of the underlying
      // reports viewmodel
      this.Reports.PropertyChanged += Reports_PropertyChanged;

      // The attachment command
      NewAttachmentCommand = new RelayCommand<LiteFeatureDetailsViewModelAttachItem>(NewAttachment);
    }
    #endregion

    #region Setup

    /// <summary>
    /// Reset the viewmodel to its initial state
    /// </summary>
    private void ResetViewModel()
    {
      this.History.Clear();

      this.EditModeActive = false;

      this.AllowEdits = LiteClientSettingsViewModel.Instance.AllowGeoNoteEdits;
      this.Feature = null;
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attaches the view model to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      if (!IsInDesignMode)
      {
        Messenger.Register<LiteDisplayFeatureDetailsRequestMessage>(this, HandleDisplayFeatureRequest);
        Messenger.Register<LiteActionRunningStateMessage>(this, HandleRunningStateChange);
        Messenger.Register<LiteNewFeatureRequestMessage>(this, HandleNewFeatureRequest);
        Messenger.Register<LiteStopEditFeatureRequestMessage>(this, HandleStopEditFeatureRequest);
        Messenger.Register<LiteDisplayCoordinateSystemChangedMessage>(this, HandleDisplayCoordinateSystemChanged);
      }
    }

    /// <summary>
    /// Handles the messenger request for displaying the details of a provided feature. 
    /// </summary>
    private async void HandleDisplayFeatureRequest(LiteDisplayFeatureDetailsRequestMessage request)
    {
      // If there actually is a feature available and we are not running, display the feature
      if (request.RecipeHolders != null && !IsRunning)
      {
        try
        {
          // Display the running state ('Getting Feature')
          HandleRunningStateChange(new LiteActionRunningStateMessage(sender: this, source: this, isBusy: true, message: ApplicationResources.FeatureDetailsGettingFeature));

          // Get the feature asynchronously, by using the FeatureRecipe of the IFeatureRecipeHolder.
          // Note that a Feature itself implements IFeatureRecipeHolder and is capable of yielding itself via a Recipe
          var getFeatures = new List<Task<Feature>>();
          if (request.RecipeHolders != null && request.RecipeHolders.Count > 0)
          {
            foreach (var recipeHolder in request.RecipeHolders)
            {
              getFeatures.Add(recipeHolder.FeatureRecipe.GetFeatureAsync());
            }

            // Get all features, by awaiting all recipe holders.
            var resultFeatures = await TaskFunctions.WhenAll(getFeatures);
            var features = new List<Feature>();
            if (resultFeatures != null)
            {
              foreach (var feature in resultFeatures)
              {
                if (feature != null)
                {
                  features.Add(feature);
                }
              }
            }

            if (EditModeActive)
            {
              // We are currently editing, so set any candidate for attaching
              // Check whether we can handle the candidate
              if (features.Count > 0)
              {
                // Set the candidate feature for attaching
                CandidateForAttach = features[0];

                // Set the candidate geometry for picking up
                var selectionField = request.SelectedGeometryFieldDescriptor;
                if (selectionField != null)
                {
                  CandidateGeometryForAttach = features[0][selectionField.Name] as IFeatureGeometry;
                }

                CheckCrudCommands();
                RaiseEditabilityEvents();
              }
            }
            else
            {
              // First clear out edit mode, we are getting an existing feature
              StopEditing(false);

              // Determine the details to be presented for the current set of features
              if (features.Count > 0)
              {
                var featureDetails = FeatureDetailsProvider.DetailsFor(features);

                if (LiteMapTrailViewModel.IsTrailFeature(featureDetails))
                {
                  featureDetails = await UpdatedTrailFeature(featureDetails);
                }

                // Set the feature
                this.Feature = new EditableFeature(featureDetails, true);

                // Set the geometry field that was responsible for activing the request
                ActivatedViaGeometryField = request.SelectedGeometryFieldDescriptor;

                if (request.MakeViewActive)
                {
                  // Make the view active
                  ViewIsActive = true;
                }

                if (request.StartEditing && AllowEdits && ActivatedViaGeometryField != null)
                {
                  if (ActivatedViaGeometryField.EditabilityProperties.AllowUpdate)
                  {
                    // Activate the edit mode
                    this.EditModeActive = true;

                    // Notify the view that it should become visible for the user
                    this.ViewIsActive = true;

                    // Send a request to start editing the specified geometry
                    this.Messenger.Send(new LiteStartEditGeometryRequestMessage(this, this.Feature, request.SelectedGeometryFieldDescriptor));
                  }
                }
              }
            }
          }
        }
        finally
        {
          // Always make sure we reset the progress indicator (even in case of exceptions)
          HandleRunningStateChange(new LiteActionRunningStateMessage(sender: this, source: this, isBusy: false));
        }
      }
    }

    /// <summary>
    /// Handle the new feaure request message
    /// </summary>
    /// <param name="request">the incomming request</param>
    private void HandleNewFeatureRequest(LiteNewFeatureRequestMessage request)
    {
      if (request.StartedWithTrail)
      {
        // We started the new feature using the trail; clear the trail
        Messenger.Send(new LiteClearTrailRequestMessage(this, true));
      }

      // First cancel the current edit mode
      StopEditing(false);

      // Create and set a new feature, view will automatically switch to edit mode because its a new feature
      this.Feature = request.Feature;

      // Activate the edit mode
      this.EditModeActive = AllowEdits;

      // Notify the view that it should become visible for the user
      this.ViewIsActive = true;

      // Send a request to start editing the specified geometry
      this.Messenger.Send(new LiteStartEditGeometryRequestMessage(this, this.Feature, request.GeometryDescriptor));
    }

    /// <summary>
    /// Handle the new feaure request message
    /// </summary>
    /// <param name="request">the incomming request</param>
    private void HandleStopEditFeatureRequest(LiteStopEditFeatureRequestMessage request)
    {
      /// Make sure we stop editing
      this.EditModeActive = false;
    }

    /// <summary>
    /// The display cs has changed
    /// </summary>
    private void HandleDisplayCoordinateSystemChanged(LiteDisplayCoordinateSystemChangedMessage changedMessage)
    {
      // Set the new display coordinate system
      DisplayCoordinateSystem = changedMessage.DisplayCoordinateSystem;
    }
    #endregion

    #region Commands
    /// <summary>
    /// Gets the command to run the report
    /// </summary>
    public RelayCommand<object> RunReportCommand { get; private set; }

    /// <summary>
    /// The feature insert command
    /// </summary>
    public RelayCommand InsertCommand { get; private set; }

    /// <summary>
    /// The feature update command
    /// </summary>
    public RelayCommand UpdateCommand { get; private set; }

    /// <summary>
    /// The feature delete command
    /// </summary>
    public RelayCommand DeleteCommand { get; private set; }

    /// <summary>
    /// Setup the viewmodel commands
    /// </summary>
    private void SetupCommands()
    {
      SetupReportCommands();
      SetupCrudCommands();
    }

    /// <summary>
    /// Setup the report commands
    /// </summary>
    private void SetupReportCommands()
    {
      RunReportCommand = new RelayCommand<object>
       ((context) =>
       {
         var model = context as ReportViewModel;

         if (model != null)
         {
           // Do Analytics Tracking
           LiteAnalyticsTracker.TrackReport(model, LiteAnalyticsTracker.Source.Details);

           model.Run(true, true);
         }
       },
       (context) =>
       {
         var model = context as ReportViewModel;
         return model != null && !IsRunning;
       }
     );
    }

    /// <summary>
    /// Setup the editability commands
    /// </summary>
    private void SetupCrudCommands()
    {
      InsertCommand = new RelayCommand(() => { var ignored = InsertFeatureAsync(); }, () => CanInsertFeature());
      UpdateCommand = new RelayCommand(() => { var ignored = UpdateFeatureAsync(); }, () => CanUpdateFeature());
      DeleteCommand = new RelayCommand(() => { var ignored = DeleteFeatureAsync(); }, () => CanDeleteFeature());
    }

    /// <summary>
    /// Check the execution of the viewmodel commands
    /// </summary>
    private void CheckCommands()
    {
      CheckReportCommands();
      CheckCrudCommands();
    }

    /// <summary>
    /// Check the report commands
    /// </summary>
    private void CheckReportCommands()
    {
      RunReportCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Check the crud commands
    /// </summary>
    private void CheckCrudCommands()
    {
      InsertCommand.RaiseCanExecuteChanged();
      UpdateCommand.RaiseCanExecuteChanged();
      DeleteCommand.RaiseCanExecuteChanged();
    }
    #endregion

    #region Property Change Handlers

    /// <summary>
    /// Attach the current feature
    /// </summary>
    private void AttachFeature(EditableFeature editableFeature)
    {
      if (editableFeature != null)
      {
        editableFeature.PropertyChanged += EditableFeature_PropertyChanged;
        foreach (var element in editableFeature.EditableFields)
        {
          if (element.AllowUpdate)
          {
            element.PropertyChanged += EditableFeatureField_PropertyChanged;
          }
        }

        CheckCrudCommands();
        RaiseEditabilityEvents();
      }
    }

    /// <summary>
    /// Detach the current feature
    /// </summary>
    private void DetachFeature(EditableFeature editableFeature)
    {
      if (editableFeature != null)
      {
        editableFeature.PropertyChanged -= EditableFeature_PropertyChanged;
        foreach (var element in editableFeature.EditableFields)
        {
          if (element.AllowUpdate)
          {
            element.PropertyChanged -= EditableFeatureField_PropertyChanged;
          }
        }
        CheckCrudCommands();
      }
    }

    /// <summary>
    /// Called whenever the current feature changes
    /// </summary>
    protected override void OnFeatureChanged(EditableFeature oldFeature, EditableFeature newFeature)
    {
      // Make sure we no longer have a candidate for attaching to
      CandidateForAttach = null;
      CandidateGeometryForAttach = null;

      // For starters, let's assume we've got no geometry field that actived this feature
      // This will be set later
      ActivatedViaGeometryField = null;

      base.OnFeatureChanged(oldFeature, newFeature);

      // Set up the Reports for the feature (well, for its collectionRecipe, 
      // since a feature can be treated as a collectionRecipe as well)
      var feature = (newFeature != null) ? newFeature.Feature : null;

      this.Reports.SetupFor(feature);

      // Detach the oldFeature properties watching
      if (oldFeature != null)
      {
        DetachFeature(oldFeature);
      }

      // Attach the newFeature properties watching
      if (newFeature != null)
      {
        AttachFeature(newFeature);
      }

      if (!SettingFeatureFromHistory)
      {
        // Let's add the new feature to the history (if we're not actually setting the feature from a history-change).
        // Skip in case this is a trail feature, an new editable feature or null
        if (newFeature != null && newFeature.IsExistingFeature)
        {
          var allowAdd = true;

          if (LiteMapTrailViewModel.IsTrailFeature(newFeature.Feature))
          {
            var previousHistory = History.PeekPrevious();
            var previousIsTrail = previousHistory != null && LiteMapTrailViewModel.IsTrailFeature(previousHistory.Feature);
            allowAdd = !previousIsTrail;
          }

          if (allowAdd)
          {
            // Add the element to the history
            this.History.Add(newFeature, false, (f) => f.Feature == newFeature.Feature);
          }
        }
        else if (newFeature == null)
        {
          var removeCurrent = oldFeature != null && oldFeature.IsExistingFeature;
          History.ClearTail(removeCurrent);
        }
      }

      // Raise the editability events in case somethings has changed
      RaiseEditabilityEvents();

      // With a new feature available, check the commands explicitly
      // Although the RunReport command checks itself, any additional
      // commands that might be added may need to be checked.
      CheckCommands();

      CheckAttachCandidates();
    }

    /// <summary>
    /// Gets the caption used when an empty field element is created for a lookup field type
    /// </summary>
    public override string EmptyFieldLookupElementCaption
    {
      // Return a string with one space. An empty string will cause the combobox item to collapse to a thin line.
      get { return " "; }
    }

    /// <summary>
    /// Checks the attach candidates
    /// </summary>
    private void CheckAttachCandidates()
    {
      IList<LiteFeatureDetailsViewModelAttachItem> items = new List<LiteFeatureDetailsViewModelAttachItem>();

      var feature = this.Feature;
      if (feature != null)
      {
        var targetGeometry = new FeatureTargetGeometry(feature, -1);
        var table = feature.TableDescriptor;
        var requestCandidates = new LiteGetAttachCandidatesRequestMessage(this, table);

        // Ask other view models to contribute to attach candidates
        Messenger.Send(requestCandidates);

        // When returned, process the (updated) attach candidates
        if (requestCandidates.AttachCandidates != null)
        {
          foreach (var candidate in requestCandidates.AttachCandidates)
          {
            var messenger = Messenger;
            var insertRequest = candidate.ToNewFeatureRequest(targetGeometry);
            if (insertRequest != null)
            {
              // Add the attachItem that, when activated, sends the appropriate request to the messenger
              var description = string.Format(ApplicationResources.FeatureDetailsAttachNewItem, candidate.Name);
              items.Add(new LiteFeatureDetailsViewModelAttachItem(candidate.Name, description, candidate.PathName, () => messenger.Send(insertRequest)));
            }
          }
        }
      }

      // Set the attached candidates to the build up items
      AttachCandidates = items;
    }

    /// <summary>
    /// A command has been activated for one of the elements in our Details View
    /// </summary>
    /// <param name="field">The field the command was activated for</param>
    /// <param name="viewModel">The corresponding viewModel holding the active content being edited</param>
    /// <param name="commandType">The type of command that is activated</param>
    protected override void OnCommandActivated(FeatureFieldDescriptor field, TypedValueViewModel viewModel, TypedValueViewModel.CommandType commandType)
    {
      var feature = Feature;

      if (feature != null && feature.TableDescriptor.FieldDescriptors[field.Name] != null)
      {
        var geometryField = field as FeatureGeometryFieldDescriptor;
        var joinField = field as FeatureJoinFieldDescriptor;
        if (geometryField != null)
        {
          switch (commandType)
          {
            case TypedValueViewModel.CommandType.GeometryEdit:
              // Stop any editing that we were doing
              Messenger.Send(new LiteStopEditGeometryRequestMessage(this));

              // Start editing the geometry
              Messenger.Send(new LiteStartEditGeometryRequestMessage(this, feature, geometryField));
              break;

            case TypedValueViewModel.CommandType.GeometryCopy:
              // Copy the selected geometry
              var selectedGeometry = this.CandidateGeometryForAttach;
              if (selectedGeometry != null)
              {
                var ok = selectedGeometry.GeometryType == geometryField.FieldType;
                if (!ok && selectedGeometry.GeometryType.IsMulti && geometryField.FieldType.IsSingle)
                {
                  // Multi Geometry with Single Field
                  var asCollection = selectedGeometry as IMultiFeatureGeometry;
                  if (asCollection != null && asCollection.Count > 0)
                  {
                    selectedGeometry = asCollection.First as IFeatureGeometry;
                    ok = selectedGeometry != null && selectedGeometry.GeometryType == geometryField.FieldType;
                  }
                }

                if (ok)
                {
                  // Send a request to clear the trail
                  Messenger.Send(new LiteClearTrailRequestMessage(this, true));

                  // Stop any editing that we were doing
                  Messenger.Send(new LiteStopEditGeometryRequestMessage(this));

                  // Set the geometry to the new value
                  feature[geometryField.Name] = selectedGeometry;

                  // And start editing it immediately
                  Messenger.Send(new LiteStartEditGeometryRequestMessage(this, feature, geometryField));
                }
              }
              break;

            case TypedValueViewModel.CommandType.GeometryClear:
              // Stop any editing that we were doing
              Messenger.Send(new LiteStopEditGeometryRequestMessage(this));

              // Clear the selected geometry
              feature[geometryField.Name] = null;
              break;
          }
        }
        else if (joinField != null)
        {
          if (feature.AllowAttachDetach)
          {
            switch (commandType)
            {
              case TypedValueViewModel.CommandType.JoinCopy:
                // Copy the selected geometry
                var selectedFeature = this.CandidateForAttach;
                if (selectedFeature != null && feature.CanAttach(selectedFeature))
                {
                  var ok = selectedFeature.TableDescriptor == joinField.ResultTableDescriptor;

                  if (ok)
                  {
                    feature.AttachToFeature = CandidateForAttach;
                    CheckCrudCommands();
                    RaiseEditabilityEvents();

                    // And refresh the values
                    Refresh();
                  }
                }
                break;

              case TypedValueViewModel.CommandType.JoinClear:
                feature.AttachToFeature = null;
                CheckCrudCommands();
                RaiseEditabilityEvents();

                // And refresh the values
                Refresh();
                break;
            }
          }
        }
      }
    }

    /// <summary>
    /// A feature value has been activated (ie, a hyperlinked value has been pressed).
    /// This information is sent on the messenger, to be dealt with by a FeatureValueActivation ViewModel
    /// that has been set up specifically for that purpose.
    /// </summary>
    protected override void OnFeatureValueActivated(Feature feature, FeatureFieldDescriptor field, object value)
    {
      // The base will raise the property changed notification
      base.OnFeatureValueActivated(feature, field, value);

      // Create a value activation request
      var request = new LiteActivateFeatureValueRequestMessage(this, feature, field, value);

      // Send the request on the messenger
      Messenger.Send(request);
    }

    /// <summary>
    /// Is called whenever the edit mode isActive changes. 
    /// </summary>
    /// <param name="editModeActive">The new value for the editModeActive flag</param>
    protected async override void OnEditModeActiveChanged(bool editModeActive)
    {
      // Make sure the base behavior is carried out
      base.OnEditModeActiveChanged(editModeActive);

      // Stop any editing that we were doing
      this.Messenger.Send(new LiteStopEditGeometryRequestMessage(this));

      if (Feature != null)
      {
        if (editModeActive)
        {
          // In case we were activated via a Geometry Field, set that field active
          var geometryFieldForEdit = ActivatedViaGeometryField;

          // For next round, do not use the geometry field
          ActivatedViaGeometryField = null;

          if (geometryFieldForEdit != null && geometryFieldForEdit.EditabilityProperties.AllowUpdate)
          {
            // Send a request to start editing the specified geometry
            this.Messenger.Send(new LiteStartEditGeometryRequestMessage(this, this.Feature, geometryFieldForEdit));
          }
        }
      }

      // Raise the editability events
      RaiseEditabilityEvents();

      // Also make sure to check the reports
      RaisePropertyChanged(ReportsEnabledPropertyName);

      // Notify the outside world of the changed edit mode
      this.Messenger.Send(new LiteFeatureDetailsEditModeChangedMessage(this, editModeActive));

      if (Feature != null && !editModeActive && _refreshValuesOnEditModeDeactivate)
      {
        // Get the correct feature values
        await Feature.EnsureSingleJoinValues(true);

        // And refresh the editor when values have been retrieved
        Refresh();
      }
    }

    /// <summary>
    /// Callback when Authentication changed
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Reset the viewmodel after the authentication changes
        ResetViewModel();
      }
    }

    #endregion

    #region History
    /// <summary>
    /// Indicates whether we are setting the current feature from the history
    /// </summary>
    private bool SettingFeatureFromHistory { get; set; }

    /// <summary>
    /// The viewModel that takes care of the history of elements
    /// </summary>
    public HistoryViewModel<EditableFeature> History { get; private set; }

    /// <summary>
    /// The helper class that is capable of wrapping multiple features into one feature
    /// for display purposes.
    /// </summary>
    public LiteFeatureDetailFeatureDetailsProvider FeatureDetailsProvider { get; private set; }

    /// <summary>
    /// Callback for changes in the History. If the current element has changed, make sure
    /// we set up the feature for the viewModel to this history-element
    /// </summary>
    void History_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      // Check whether the current feature has changed
      if (e.PropertyName == HistoryViewModel.CurrentPropertyName)
      {
        try
        {
          SettingFeatureFromHistory = true;

          // First cancel the current edit mode
          StopEditing(false);

          this.Feature = History.Current;
        }
        finally
        {
          SettingFeatureFromHistory = false;
        }
      }
    }
    #endregion

    #region Reports
    /// <summary>
    /// Holds the Reports for this definition; automatically setting
    /// its contents dependent on this query 
    /// </summary>
    public ReportsViewModel Reports { get; private set; }

    /// <summary>
    /// Can the lot be exported
    /// </summary>
    public bool ReportsEnabled
    {
      get { return !EditModeActive && !Reports.IsBusy; }
    }

    /// <summary>
    /// The availability of the reports
    /// </summary>
    public Visibility ReportsVisibility
    {
      get { return Feature != null && Reports.Reports.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Gets or sets if the view should be active or not
    /// Only used to force set the view active from the viewmodel point of view
    /// </summary>
    public Boolean ViewIsActive
    {
      get { return _viewIsActive; }
      set
      {
        if (value != _viewIsActive)
        {
          _viewIsActive = value;
          RaisePropertyChanged();
        }
      }
    }

    /// <summary>
    /// Callback when the report collection changes
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
        RaisePropertyChanged(ReportsVisibilityPropertyName);
      }
    }
    #endregion

    #region Editability Logic

    /// <summary>
    /// Is the user allowed to create or update items
    /// </summary>
    public bool AllowEdits
    {
      get { return _allowEdits; }
      private set
      {
        if (value != _allowEdits)
        {
          _allowEdits = value;
          RaisePropertyChanged();
          RaiseEditabilityEvents();
        }
      }
    }

    /// <summary>
    /// Candidate for attaching to the current feature
    /// </summary>
    private Feature CandidateForAttach
    {
      get;
      set;
    }

    /// <summary>
    /// Candidate for attaching to the current feature
    /// </summary>
    private IFeatureGeometry CandidateGeometryForAttach
    {
      get;
      set;
    }


    /// <summary>
    /// The selection's geometryField that caused the feature to be actived
    /// </summary>
    private FeatureGeometryFieldDescriptor ActivatedViaGeometryField
    {
      get;
      set;
    }

    /// <summary>
    /// The attach candidates
    /// </summary>
    public IList<LiteFeatureDetailsViewModelAttachItem> AttachCandidates
    {
      get { return _attachCandidates; }
      set
      {
        _attachCandidates = value;

        // Notify the 
        RaisePropertyChanged(AttachCandidatesPropertyName);
        RaisePropertyChanged(AttachCandidatesVisibilityPropertyName);
      }
    }

    /// <summary>
    /// Returns the visibility for the attach candidates
    /// </summary>
    public Visibility AttachCandidatesVisibility
    {
      get
      {
        // In case there are candidates we are visible, otherwise collapsed
        return AllowEdits && _attachCandidates != null && _attachCandidates.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
      }
    }

    /// <summary>
    /// The new attachment command
    /// </summary>
    public RelayCommand<LiteFeatureDetailsViewModelAttachItem> NewAttachmentCommand
    {
      get;
      private set;
    }

    /// <summary>
    /// Stop the editing, optionally indicating whether the feature should be refreshed
    /// </summary>
    private void StopEditing(bool refreshValuesOnEditModeDeactivate = true)
    {
      // Indicate whether refreshing of the feature is allowed
      _refreshValuesOnEditModeDeactivate = refreshValuesOnEditModeDeactivate;

      // Switch edit mode
      EditModeActive = false;

      // Set back to default value (true)
      _refreshValuesOnEditModeDeactivate = true;
    }

    /// <summary>
    /// Create a new attachment
    /// </summary>
    /// <param name="item">Creates a new attachment</param>
    private async void NewAttachment(LiteFeatureDetailsViewModelAttachItem item)
    {
      if (item != null)
      {
        // Give the UI some time to respond; do via a yield in case this is not
        // called from the ui thread.
        await TaskFunctions.Yield();
        await TaskFunctions.Delay(100);

        // Start the insert by invoking the Attach Item's start action
        item.StartAction();
      }
    }

    /// <summary>
    /// Can the current feature be inserted
    /// </summary>
    /// <returns>true if it can be inserted</returns>
    private bool CanInsertFeature()
    {
      return Feature != null && Feature.CanInsert;
    }

    /// <summary>
    /// Can the current feature be updated
    /// </summary>
    /// <returns>true if it can be updated</returns>
    private bool CanUpdateFeature()
    {
      return Feature != null && Feature.CanUpdate;
    }

    /// <summary>
    /// Can the current feature be deleted
    /// </summary>
    /// <returns>true if it can be deleted</returns>
    private bool CanDeleteFeature()
    {
      return Feature != null && Feature.CanDelete;
    }

    /// <summary>
    /// Inserts the current feature
    /// </summary>
    private async Task InsertFeatureAsync()
    {
      var currentFeature = Feature;

      if (currentFeature != null && currentFeature.AllowInsert)
      {
        var changedFields = currentFeature.ChangedFields();
        var transactionResult = await currentFeature.InsertAsync();

        // Stop editing, indicating that we don't need to refresh the active feature
        StopEditing(false);

        if (transactionResult.Succeeded)
        {
          // Insert succeeded
          var resultFeature = transactionResult.Element;

          // The existing feature
          this.Feature = new EditableFeature(resultFeature, true);

          // Let the rest know
          this.Messenger.Send(new LiteFeatureTransactionMessage(this, resultFeature, LiteFeatureTransactionMessage.TransactionType.Inserted, changedFields));
        }
        else
        {
          // Not ok
          await this.MessageBoxService.ShowAsync(ApplicationResources.TransactionInsertFailedMessage, ApplicationResources.TransactionInsertFailedCaption, MessageBoxButton.OK);
        }
      }
    }

    /// <summary>
    /// Updates the current feature 
    /// </summary>
    private async Task UpdateFeatureAsync()
    {
      var currentFeature = Feature;

      if (currentFeature != null && currentFeature.AllowUpdate)
      {
        var changedFields = currentFeature.ChangedFields();
        var transactionResult = await currentFeature.UpdateAsync();

        StopEditing(false);

        if (transactionResult.Succeeded)
        {
          // Update succeeded
          var resultFeature = transactionResult.Element;

          // The existing feature
          this.Feature = new EditableFeature(resultFeature, true);

          // Let the rest know
          this.Messenger.Send(new LiteFeatureTransactionMessage(this, resultFeature, LiteFeatureTransactionMessage.TransactionType.Updated, changedFields));
        }
        else
        {
          // Not ok
          await this.MessageBoxService.ShowAsync(ApplicationResources.TransactionUpdateFailedMessage, ApplicationResources.TransactionUpdateFailedCaption, MessageBoxButton.OK);
        }
      }
    }

    /// <summary>
    /// Deletes the current feature
    /// </summary>
    private async Task DeleteFeatureAsync()
    {
      var currentFeature = Feature;

      if (currentFeature != null && currentFeature.AllowDelete)
      {
        var messageResult = await this.MessageBoxService.ShowAsync(ApplicationResources.TransactionDeleteQuestionMessage, ApplicationResources.TransactionDeleteQuestionCaption, MessageBoxButton.OKCancel, MessageBoxResult.Cancel);

        if (messageResult == MessageBoxResult.OK)
        {
          var changedFields = currentFeature.ChangedFields();
          var transactionResult = await currentFeature.DeleteAsync();

          // Stop editing, indicating that we don't need to refresh the active feature
          StopEditing(false);

          if (transactionResult.Succeeded)
          {
            // Delete succeeded
            var resultFeature = transactionResult.Element;

            Feature = null;

            // Let the rest know
            this.Messenger.Send(new LiteFeatureTransactionMessage(this, resultFeature, LiteFeatureTransactionMessage.TransactionType.Deleted, changedFields));
          }
          else
          {
            await this.MessageBoxService.ShowAsync(ApplicationResources.TransactionDeleteFailedMessage, ApplicationResources.TransactionDeleteFailedCaption, MessageBoxButton.OK);
          }
        }
      }
    }

    /// <summary>
    /// Is editing of the current feature allowed
    /// </summary>
    private Boolean AllowEditCurrentFeature()
    {
      var result = false;

      if (Feature != null)
      {
        var allowed = LiteClientSettingsViewModel.Instance.IsGeoNoteAllowed(Feature.TableDescriptor.ExternalName);
        result = allowed && AllowEdits && (Feature.AllowInsert || Feature.AllowUpdate || Feature.AllowDelete);
      }

      return result;
    }

    /// <summary>
    /// Edit mode visibility
    /// </summary>
    public Visibility EditModeVisibility
    {
      get { return AllowEditCurrentFeature() ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Edit view visibility
    /// </summary>
    public Visibility EditViewVisibility
    {
      get { return (EditModeActive) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Insert feature visibility
    /// </summary>
    public Visibility InsertVisibility
    {
      get { return (EditModeActive && Feature != null && Feature.AllowInsert) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Update feature visibility
    /// </summary>
    public Visibility UpdateVisibility
    {
      get { return (EditModeActive && Feature != null && Feature.AllowUpdate) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Delete feature visibility
    /// </summary>
    public Visibility DeleteVisibility
    {
      get { return (EditModeActive && Feature != null && Feature.AllowDelete) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Raises all editability events
    /// </summary>
    private void RaiseEditabilityEvents()
    {
      RaisePropertyChanged(EditModeVisibilityPropertyName);
      RaisePropertyChanged(InsertVisibilityPropertyName);
      RaisePropertyChanged(UpdateVisibilityPropertyName);
      RaisePropertyChanged(DeleteVisibilityPropertyName);
      RaisePropertyChanged(EditViewVisibilityPropertyName);
    }

    /// <summary>
    /// A property of the current feature has changed
    /// </summary>
    private void EditableFeature_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      CheckCrudCommands();
      RaiseEditabilityEvents();
    }

    /// <summary>
    /// A field value of the current feature has changed
    /// </summary>
    void EditableFeatureField_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      var field = sender as EditableFeatureField;

      if (field != null && field.FieldDescriptor.IsGeometry)
      {
        // Change in geometry; make it visible in the editor
        Refresh();
      }

      CheckCrudCommands();
    }
    #endregion

    #region Messenger Changes
    /// <summary>
    /// The feature that was active when a run command was initiated; in case
    /// the run action is finished and we are still looking at this feature, we
    /// can refresh it
    /// </summary>
    private EditableFeature RunningFeature { get; set; }

    /// <summary>
    /// Is this busy retrieving some information
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

          // Check the (availability of the) commands
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
    /// Is the query running
    /// </summary>
    public Visibility IsRunningVisibility
    {
      get { return (IsRunning) ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Handles the state change - displaying the correct state on the UI via
    /// progress indicators. Only do this, when we've actually initiated the
    /// state change.
    /// </summary>
    private void HandleRunningStateChange(LiteActionRunningStateMessage runningStateChange)
    {
      if (runningStateChange.Source == this)
      {
        // We've initiated the evaluation/displaying of a collection; this could have been
        // done because we activated a join-field.

        // Dependent on the busy flag, we could do something here
        RunningStateDescription = runningStateChange.RunningStateDescription;
        IsRunning = runningStateChange.IsRunning;

        if (IsRunning)
        {
          RunningFeature = this.Feature;
        }
        else
        {
          if (Object.ReferenceEquals(Feature, RunningFeature))
          {
            // Notify our UI that things might have changed (causing join fields to display their correct values; if at all)
            this.Refresh();
          }

          this.RunningFeature = null;
        }
      }
      else
      {
        // We have not initiated the lot
        // No-op
      }
    }
    #endregion

    #region Display Coordinate System handling
    /// <summary>
    /// The current display coordinate system
    /// </summary>
    private EpsgCoordinateSystemReference DisplayCoordinateSystem
    {
      get { return _displayCS; }
      set { _displayCS = value; }
    }

    /// <summary>
    /// Updates the trail feature to contain the correct length/area properties
    /// </summary>
    private Task<Feature> UpdatedTrailFeature(Feature trailFeature)
    {
      return TaskFunctions.FromResult(trailFeature);
    }
    #endregion

  }
}
