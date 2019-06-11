using System;
using System.Windows;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using Lite.Resources.Localization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lite
{
    public class LiteInventarioCajaViewModel : FeatureDetailsViewModel
{    
    #region Static Property Names
    /// <summary>
    /// Are the reports enabled
    /// </summary>
    /// 

    //FMEJIA: Ajustes para despliegue de ventana de Caja y clientesu2000     
   public const string IdCajaPropertyName = "IdCaja";
   public const string NombreCajaPropertyName = "NombreCaja";
   public const string VisibilidadPropertyName = "Visibilidad";
   public const string CuentaClientePropertyName = "CuentaCliente";
   public const string VisibilidadU2000PropertyName = "VisibilidadU2000";

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
    #endregion

    #region Trail Feature Name
    /// <summary>
    /// The name of the trail's table descriptor
    /// </summary>
    private const string TrailTableDescriptorName = "_trail_";
    #endregion

    #region Fields
    /// <summary>
    /// A flag indicating whether the ui is busy retrieving some information
    /// </summary>
    /// 
    private string _idCaja;
    private string _nombreCaja;
    private Visibility _visibilidad;
    private string _cuentaCliente;
    private Visibility _visibilidadU2000;


    private bool _isRunning;

    /// <summary>
    /// Holds the descriptive running state
    /// </summary>
    private string _runningStateDescription;
    #endregion

    #region Constructors
    /// <summary>
    /// Default Constructors
    /// </summary>
    public LiteInventarioCajaViewModel(Messenger messenger = null)
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
      Visibilidad = Visibility.Collapsed;
      VisibilidadU2000 = Visibility.Collapsed;
      CuentaCliente = "";

      IdCaja = "";
      NombreCaja = "";
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



            // Determine the details to be presented for the current set of features
            if (features.Count > 0)
            {
                var featureDetails = FeatureDetailsProvider.DetailsFor(features);

                // Set the feature
                this.Feature = new EditableFeature(featureDetails, true);
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
    #endregion

    #region Commands
    /// <summary>
    /// Gets the command to run the report
    /// </summary>
    public RelayCommand<object> RunReportCommand { get; private set; }

    /// <summary>
    /// Setup the viewmodel commands
    /// </summary>
    private void SetupCommands()
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
    /// Check the execution of the viewmodel commands
    /// </summary>
    private void CheckCommands()
    {
      RunReportCommand.RaiseCanExecuteChanged();
    }
    #endregion

    #region Property Change Handlers
    /// <summary>
    /// Returns a flag indicating whether this is a trail feature; 
    /// we want to recognize those, since we are skipping them in the Details
    /// </summary>
    private bool IsTrailFeature(IList<Feature> features)
    {
      var selectedFeature = features != null && features.Count == 1 ? features[0] : null;
      var selectedTable = selectedFeature != null ? selectedFeature.TableDescriptor : null;
      return selectedTable != null && string.Equals(selectedTable.Name, TrailTableDescriptorName);
    }

    /// <summary>
    /// Called whenever the current feature changes
    /// </summary>
    protected override void OnFeatureChanged(EditableFeature oldFeature, EditableFeature newfeature)
    {
      base.OnFeatureChanged(oldFeature, newfeature);

      var feature = (newfeature != null) ? newfeature.Feature : null;
      //MessageBox.Show(feature.TableDescriptor.Name);
      if (feature.TableDescriptor.Name == "sheath_splice" || feature.TableDescriptor.Name == "sheath splice")
      {
          int indexID = feature.TableDescriptor.FieldDescriptors.IndexOf("id");
          int indexName = feature.TableDescriptor.FieldDescriptors.IndexOf("name");
          IdCaja = feature.Values.GetValue(indexID).ToString();
          NombreCaja = feature.Values.GetValue(indexName).ToString();
          Visibilidad = Visibility.Visible;
          
      }
      else
      {
          IdCaja = "";
          NombreCaja = "";
          Visibilidad = Visibility.Collapsed;
          
      }
      if (feature.TableDescriptor.Name.ToLower().Contains("cliente"))
      {
          int indexCuenta = feature.TableDescriptor.FieldDescriptors.IndexOf("cuenta");
          try
          {
              CuentaCliente = feature.Values.GetValue(indexCuenta).ToString();
              VisibilidadU2000 = Visibility.Visible;
          }
          catch { }
      }
      else
      {
          CuentaCliente = "";
          VisibilidadU2000 = Visibility.Collapsed;
      }
      // Set up the Reports for the feature (well, for its collectionRecipe, 
      // since a feature can be treated as a collectionRecipe as well)
      this.Reports.SetupFor(feature);

      if (!SettingFeatureFromHistory)
      {
        // Let's add the new feature to the history (if we're not actually setting the feature from a history-change).
        // Skip in case this is a trail feature
        this.History.Add(newfeature);
      }

      // With a new feature available, check the commands explicitly
      // Although the RunReport command checks itself, any additional
      // commands that might be added may need to be checked.
      CheckCommands();
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
      get { return !Reports.IsBusy; }
    }

    /// <summary>
    /// The availability of the reports
    /// </summary>
    public Visibility ReportsVisibility
    {
      get { return Feature != null && Reports.Reports.Count > 0 ? Visibility.Visible : Visibility.Collapsed; }
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

    #region Messenger Changes
    /// <summary>
    /// The feature that was active when a run command was initiated; in case
    /// the run action is finished and we are still looking at this feature, we
    /// can refresh it
    /// </summary>
    private Feature RunningFeature { get; set; }

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

    public string IdCaja
    {
        get { return _idCaja; }
        set
        {
            if (_idCaja != value)
            {
                _idCaja = value;
                RaisePropertyChanged(IdCajaPropertyName);
             }
        }
    }
    public string NombreCaja
    {
        get { return _nombreCaja; }
        set
        {
            if (_nombreCaja != value)
            {
                _nombreCaja = value;
                RaisePropertyChanged(NombreCajaPropertyName);
            }
        }
    }
    public Visibility Visibilidad
    {
        get { return _visibilidad; }
        set
        {
            if (_visibilidad != value)
            {
                _visibilidad = value;
                RaisePropertyChanged(VisibilidadPropertyName);
            }
        }
    }
    public string CuentaCliente
    {
        get { return _cuentaCliente; }
        set
        {
            if (_cuentaCliente != value)
            {
                _cuentaCliente = value;
                RaisePropertyChanged(CuentaClientePropertyName);
            }
        }
    }
    public Visibility VisibilidadU2000
    {
        get { return _visibilidadU2000; }
        set
        {
            if (_visibilidadU2000 != value)
            {
                _visibilidadU2000 = value;
                RaisePropertyChanged(VisibilidadU2000PropertyName);
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
        // For now, no-op
      }
    }
    #endregion

  }
}
