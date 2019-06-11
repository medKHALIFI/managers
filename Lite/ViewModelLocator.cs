using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using SpatialEye.Framework.Authentication;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Client.Analytics;
using SpatialEye.Framework.Client.Animation;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.ServiceProviders.XY;
using SpatialEye.Framework.Services;
using SpatialEye.Framework.GeoLocators.Services;
using SpatialEye.Framework.Client.Resources;
using SpatialEye.Framework.Maps;

namespace Lite
{

  /// <summary>
  /// The ViewModelLocator is the main entry into the ViewModels being used inside Lite.
  /// An instance of it is set up as a Resource in App.xaml; all controls can reference
  /// this ViewModelLocator via that resource and use it to bind to the ViewModels that
  /// have been declared here.
  /// </summary>
  public partial class ViewModelLocator : ViewModelLocatorBase
  {
    #region Supported Cultures
    /// <summary>
    /// All cultures to be supported in the application; each culture does need to be explicitly resourced
    /// The cultures below are supported in the framework as well as Lite.
    /// </summary>
    private static string[] _supportedCultures = new string[] { "de", "es", "fr", "pt", "ko", "ru", "it", "nl", "tr", "sv", "et", "fi", "cs", "ja" };
    #endregion

    #region Initialization
    /// <summary>
    /// Initialize the ViewModel Locator, setting up the Service Providers
    /// and all the ViewModels required to build up the application.
    /// </summary>
    protected override void Initialize()
    {
      // Base initialization
      base.Initialize();

      // Initialize the client
      SetupAnalyticsTracker();
      SetupServiceProviders();
      SetupViewModels();
      SetupAdditionalViewModels();
    }

    /// <summary>
    /// Register additional services with the service locator.
    /// </summary>
    protected override void InitializeServiceLocator(SimpleIoC serviceLocator)
    {
      // If not in design-mode (Blend/VS), register some UI services
      if (!IsInDesignMode)
      {
        // Register the UI Services 
        // MessageBox Service
        MessageBoxViewModel = new LiteMessageBoxViewModel();
        serviceLocator.Register<IMessageBoxService>(() => MessageBoxViewModel);

        // SaveFileDialog Service
        serviceLocator.Register<ISaveFileDialogService, SaveFileDialogService>();
      }
    }

    /// <summary>
    /// Registers an Analytics Tracker to be used to track usage of the application. In own code,
    /// the AnalysisTracker can be used to store usage information by using the singleton.
    /// <list>
    /// AnalysisTracker.Instance.TrackApplication()
    /// AnalysisTracker.Instance.TrackPageView(string page)
    /// AnalysisTracker.Instance.TrackEvent(string category, string action)
    /// AnalysisTracker.Instance.TrackEvent(string category, string action, string label)
    /// AnalysisTracker.Instance.TrackEvent(string category, string action, string label, int value)
    /// </list>
    /// Dependent on the AnalyticsTracker that is register, the information when will be
    /// stored to different environments.
    /// </summary>
    private void SetupAnalyticsTracker()
    {
      // A flag, indicating whether we want to use the GoogleAnalyticsTracker. 
      // In case the Google AnalyticsTracker is used, the appropriate JavaScript must
      // have been loaded in the application's html page for the tracking to work.
      // In case the JS code is not found, the GoogleAnalyticsTracker will resort
      // to no-ops, so there is no problem in automatically switching it on.
      // Note: Lite uses the LiteAnalysisTracker that internally uses the setup
      // tracker (here Google), but provides a more suitable API for tracking
      // Lite behavior. See the LiteAnalysisTracker for more info.
      bool useGoogleAnalyticsTracker = true;

      if (useGoogleAnalyticsTracker)
      {
        // Store the Google Analytics Tracker as the analytics tracker to be used to store usage of the application/client framework
        AnalyticsTracker.SetTracker(new GoogleAnalyticsTracker());
      }
    }

    /// <summary>
    /// Create the required service providers
    /// </summary>
    private void SetupServiceProviders()
    {
      // X&Y
      // Create the X&Y Service Provider with default connection parameters as specified in ServiceReferences.ClientConfig
      // Note that this will automatically register the service provider; only keeping the reference for convenience
      this.MainXYServiceProvider = new XYServiceProvider { Name = this.ServerName };
    }

    /// <summary>
    /// Create the view models for the views to databind - these provide the logic for the Views/UI and themselves
    /// access the underlying model by utilizing the services created.
    /// </summary>
    private void SetupViewModels()
    {
      // Get the prefered culture name before setting up the cultures.
      // Changes in culture selection (even automatic ones) will be stored in the Isolated Storage
      var preferedCultureName = LiteIsolatedStorageManager.Instance.CultureName;

      // The application view model holds some basic information on the application, like resources and mechanisms
      // to change the current culture. 
      ApplicationViewModel = new ApplicationViewModel() { ApplicationId = APPLICATIONID };
      ApplicationViewModel.Resources = new Lite.Resources.Localization.ApplicationResources();

      // Add (additional) cultures you want to use in the application
      foreach (var culture in _supportedCultures)
      {
        ApplicationViewModel.Cultures.Add(new ApplicationCultureViewModel(culture, true));
      }

      // Now try to set the previously used culture as the current one
      var preferedCulture = ApplicationViewModel.Culture(preferedCultureName);
      if (preferedCulture != null)
      {
        // Set the prefered culture (the last one used)
        ApplicationViewModel.CurrentCulture = preferedCulture;
      }
      else
      {
        // Try to select the OS culture as the application culture
        // if the OS culture is not in the available application cultures it defaults to "en-us", the neutral culture
        ApplicationViewModel.SelectDefaultCulture();
      }

      // Create the client settings view model that provides typed access to
      // the client settings that are defined in the application and set via
      // the management client.
      this.ClientSettingsViewModel = new LiteClientSettingsViewModel();

      // Set up the LiteTraceLogger, capable of processing log messages
      this.TraceLogger = new LiteTraceLogger() { LogLevel = 5 };

      // Store the TraceLogger, so all Framework's log messages use our logger
      SpatialEye.Framework.Client.Diagnostics.TraceLogger.SetLogger(TraceLogger);

      // Use the base authentication view model using our ApplicationId
      this.AuthenticationViewModel = new AuthenticationViewModel(APPLICATIONID)
      {
        // Set to true, in case you want to ask the user 'are you sure...' upon signing out
        AskConfirmationOnSignOut = false,

        // In case the server is not running, this flag indicates whether we want the server 
        // address to be included in the error message on the dialog.
        // false: 'Can not connect to Server'
        // true :
        IncludeServerAddressInConnectionErrorMessage = true
      };

      // Set up an opacity animator that we will be using upon signing-in/out; it allows views to 
      // bind to an animated opacity value.
      this.AuthenticationOpacity = new OpacityAnimator();

      // MessageBox view model dedicated to the toolbox area; is bound to a MessageBoxView
      // that is displayed on top of the Toolbox for those viewModels that want to display
      // messages just in the toolbox area
      this.ToolboxMessageBoxViewModel = new LiteMessageBoxViewModel();

      // The view model responsible for getting Server info in a decent format
      ServerInfoViewModel = new LiteServerInfoViewModel(MainXYServiceProvider);

      // Set up the specific MapsViewModel that is capable of setting
      // up the default maps, as well as get user defined maps/layers from
      // the isolated storage (manager).
      this.MapsViewModel = new LiteMapsViewModel()
      {
        MessageBoxService = this.ToolboxMessageBoxViewModel,

        // Only use a Single Backdrop layer (ie Bing) that is switched on, 
        // instead of all available Backdrop layers (with the first one on).
        UseSingleBackdropLayer = true,

        // The minimum zoom level that will be requested for any map. This means
        // the user can not zoom any more out than the value specified here.
        // Valid range = [1, 15]. The default is 1.
        MinZoomLevel = 1,

        // The maximum zoom level that will be requested for any map. This means
        // the user can not zoom any deeper than the value specified here.
        // Valid range = [19, 23], although sensibly the maximum zoom level should be
        // in the range [21, 23]. The default value is 22.
        MaxZoomLevel = 22
      };

      // The map hover event tracker
      this.MapHoverViewModel = new LiteMapHoverViewModel();

      // The map select-hover event tracker
      this.MapSelectHoverViewModel = new LiteMapSelectHoverViewModel();

      // The handler for custom selection in case a mouse press didn't result in selection
      // The TableName and FieldName properties can be used to indicate the custom selection
      // required. The MaxDistance (in meters) can be used to specify the maximum distance
      // used for searching the elements with.
      this.MapCustomSelectionViewModel = new LiteMapCustomSelectionViewModel();

      // The viewModel controlling the behavior of the MapBar, also set the units used by the scalebar based on the application units
      this.MapBarViewModel = new LiteMapBarViewModel();

      // A variable specifying the single layer to be used for the overview map
      // It needs to refer to the name of a provider that is either directly
      // added or retrieved from the main service-provider via its reference providers.
      string overviewMapLayerProviderName = "MyPreferedProvider";

      // The viewModel handling the logic for the overview map
      this.OverviewMapViewModel = new LiteOverviewMapViewModel()
      {
        // Indicate the extra zoom levels to use; with each zoom level
        // introducing an extra factor of 2 to be used in the extent ratio
        ExtraZoomLevels = 4,

        // Is the user allowed to move the rectangle to define a new extent
        // for the source map
        AllowMovingSourceRectangle = true,

        // Do we allow the scroll wheel to control the extraZoomLevels further
        AllowScrollWheel = true,

        // Indicates whether the overview needs to keep tracking dynamically
        // during animated zooms
        AllowTrackingDuringZoomAnimation = true,

        // Indicate whether you prefer to only use backdrop layers in the overview map
        BackdropOnly = false,

        // The stroke of the source rectangle in the overview map
        RectangleStroke = new SolidColorBrush(Color.FromArgb(204, 66, 97, 145)),

        // The stroke width of the source rectangle in the overview map
        RectangleStrokeWidth = 2.0,

        // The fill of the source rectangle in the overview map
        RectangleFill = new SolidColorBrush(Color.FromArgb(20, 140, 185, 249)),

        // Do we want to animate the Rectangle with the Overview animation, when
        // the user has moved the animation rectangle. In case this is true, the
        // rectangle will be moved together with the moving map; otherwise, it
        // will immediately display in the centre
        RectangleUseAnimation = true,

        // The stroke of the source rectangle in the overview map in case we are
        // animating the rectangle back to the centre after it has been moved
        // by the user. Only in effect when RectangleUseAnimation is set to true.
        RectangleAnimationStroke = new SolidColorBrush(Color.FromArgb(102, 66, 97, 145)),

        // The stroke width to use in case we are animating the rectangle back to
        // the centre after it has been moved by the user. Only in effect when 
        // RectangleUseAnimation is set to true.
        RectangleAnimationStrokeWidth = 1.0,

        // The fill of the source rectangle in the overview map in case of animating 
        // back to the centre. Only in effect when RectangleUseAnimation is set to true.
        RectangleAnimationFill = new SolidColorBrush(Color.FromArgb(26, 140, 185, 249)),

        // The dimensions of the overview map
        Width = 200,
        Height = 200,

        // The opacity of the overview map, in the range <0.0, 100.0]
        Opacity = 100.0,

        // Indicates whether changes in the On/Off state of layers should 
        // be tracked in the overview map as well. If not, the initial
        // on/off states of the layers will be used in the overview map
        TrackLayerOnOffChanges = true,

        // Indicates whether changes in the Selected Mode of (backdrop) layers
        // should be tracked in the overview map as well. Otherwise, the
        // default modes will be picked of the layer definitions and will
        // not change.
        // Note: in case a SingleLayerDeterminator is used, the resulting
        // layer of that view model will be used for all situations and
        // this flag has no effect.
        TrackLayerSelectedModeChanges = true,

        // In case a fixed layer needs to be used, set up a function
        // that yields the single layer from the set of available layers
        SingleLayerDeterminator = (layers) => layers.Where(l => l.ServiceProvider != null && l.ServiceProvider.Name == overviewMapLayerProviderName).FirstOrDefault()
      };

      // The viewModel handling the active mode of backdrop layers, it is used to
      // display a mode-selector on the Map.
      this.MapBackdropLayerModeSelectionViewModel = new LiteMapBackdropLayerModeSelectionViewModel()
      {
        // Indicate which maps are allowed to have their layer selection
        // be carried out via this viewModel
        MapFilter = map => UseBackdropLayerSelector,

        // Allow all backdrop layers (of allowed maps) to be used
        MapLayerFilter = layer => true

        // Use this line if you want to show only layers that have multiple modes defined
        // MapLayerFilter = layer => layer.LayerDefinition != null && layer.LayerDefinition.Modes.Count > 1
      };


      // The themes view model holds the information needed to display 
      // the layers of the MapsViewModel's current map
      this.MapThemesViewModel = new LiteMapThemesViewModel()
      {
        MapLayerFilter = layer => layer.Name != LiteMapsViewModel.RestrictionMapLayerName
      };

      // The measure view model is responsible for the measure interaction modes
      this.MapMeasureViewModel = new LiteMapMeasureViewModel()
      {
        // Indicate whether we want to show a checkbox for actively selecting the default mode.
        ShowDefaultMode = false,

        // Take care of the extra height of the Map Bar
        TopMargin = 40
      };

      this.MapMeasureISViewModel = new LiteMapMeasureISViewModel()
      {
          // Indicate whether we want to show a checkbox for actively selecting the default mode.
          ShowDefaultMode = false,

          // Take care of the extra height of the Map Bar
          TopMargin = 40
      };

      // Create the MapTrail ViewModel for interaction with the trail
      this.MapTrailViewModel = new LiteMapTrailViewModel();
      this.MapTrailISViewModel = new MapTrailISViewModel();
      this.MapPointISViewModel = new MapPointISViewModel();

      // Create the Edit Geometry ViewModel for interaction with editable geometry
      this.MapEditGeometryViewModel = new LiteMapEditGeometryViewModel();

      // Create the view model responsible for the RMC popup menu
      this.MapPopupViewModel = new LiteMapPopupMenuViewModel();

      // Create the Street View ViewModel for interacting with Google Street View.
      // This interaction will only be available/visible in case a Google map is shown on the active map.
      this.StreetViewViewModel = new StreetViewViewModel();

      // The feature details view model is a PropertyGrid that displays
      // the content of a (selected) feature
      this.FeatureDetailsViewModel = new LiteFeatureDetailsViewModel()
      {
        MessageBoxService = this.ToolboxMessageBoxViewModel
      };

      // The feature insert view model, responsible for getting and managing the insertable features
      this.FeatureInsertViewModel = new LiteFeatureInsertViewModel();

      // Set up a default way to categorize the fields by using the FieldDescriptorType (alpha/geom/smartlink/relation)
      this.FeatureDetailsViewModel.Properties.FieldToCategoryConverter = FeatureDetailsViewModelProperties.FieldDescriptorTypeCategoryConverter;

      // The collection restriction view model that is responsible for restricting collections' content
      // before using them. This viewModel is a template that can be filled out further. The viewModel
      // is loosely coupled (via messenger requests) and other viewModels use its restriction capabilities
      // by just placing a LiteRestrictFeatureCollectionRequestMessage request on the messenger.

      // Set up the default restrictions (which means that default geometry-restrictions will be set up for those
      // tables for which no explicit table/field key-value pair has been set up.
      bool includeDefaultRestrictions = true;
      FeatureCollectionRestrictionViewModel = new LiteFeatureCollectionRestrictionViewModel(Messenger, includeDefaultRestrictions,
        new LiteFeatureCollectionRestrictionViewModel.TableFieldRestrictions
        {
          // Add table/geomfield pairs for indicating an explicit geometry-field selection
          // To leave a table out explicitly, include it with an empty field
          // { "table1", "field1" },
          // { "table2", ""}
        });

      // The collection result view model is capable of displaying results of queries and followed joins
      this.FeatureCollectionResultViewModel = new LiteFeatureCollectionResultViewModel();
      this.FeatureCollectionResultViewModel.Properties.TrackSelectionInFeatureDetails = true;
      this.FeatureCollectionResultViewModel.Properties.TrackSelectionInMap = false;

      // Set a default batch size - this can be determined per table (descriptor) as well,
      // by using the TableProperties API (for the Properties for the active table) or 
      // via the TablePropertiesCache where tableProperties can be accessed for any
      // table.
      this.FeatureCollectionResultViewModel.GridProperties.DefaultBatchSize = 200;

      // Create a viewModel that will be responsible for all feature values that are activated,
      // meaning clicking on a Geometry, SmartLink or Relation Field. The values that are placed
      // on the databus will be picked up by this viewModel and acted upon accordingly.
      this.FeatureValueActivationViewModel = new LiteFeatureValueActivationViewModel();

      // The view model containing all the queries
      this.QueriesViewModel = new LiteQueriesViewModel()
      {
        MessageBoxService = this.ToolboxMessageBoxViewModel
      };

      // The Place Finder ViewModel
      this.PlaceFinderViewModel = new LiteMapPlaceFinderViewModel()
      {
        // Set the typing delay to 500 ms; all characters typed within this
        // timespan will still be used in one call to the active geoLocator
        TypingDelayBeforeSendingRequest = 500,

        // The maximum number of results
        MaximumNumberOfResults = 10
      };

      // Print View Model holds all information to print, including the 
      // client-side templates that have been set up for Lite
      this.PrintViewModel = new LitePrintViewModel();

    // FMEJIA: Se agrega instancia de Factibilidad
      FactibilidadViewModel = new Lite.LiteFactibilidadView();
      MapMarkViewModel = new Lite.LiteMapMarkViewModel();
      InventarioCajaViewModel = new Lite.LiteInventarioCajaViewModel();

      EdicionSwViews = new Lite.EdicionSwView();
      MapMarkISViewModel = new Lite.LiteMapMarkISViewModel();
      MapTrailISVM = new Lite.MapTrailISViewModel();
      this.MapTrailISViewModel = new MapTrailISViewModel();
      this.MapPointISViewModel = new MapPointISViewModel();
      this.EdicionFeatureDetails = new EdicionSwViewModel();


      //MapBarIS = new Lite.LiteMapBarIS();

      // The viewModel responsible for the long polling for checking changes
      this.MapLayerChangeRetriever = new LiteMapLayerChangesRetriever();
    }
    #endregion

    #region Applying Client Settings to ViewModels
    /// <summary>
    /// Apply the Client Settings to the main view models
    /// </summary>
    private void ApplyClientSettingsToViewModels()
    {
      // Setup the default settings for the legend image generator
      // The background color if a legend items comes from an application resource and can be overridden in de 'LiteResources.xaml'
      MapThemeLayerLegendViewModel.Settings = new MapLayerLegendSettings
      {
        BackgroundColor = (Color)Application.Current.Resources["Lite.Color.Legend.Background"],
        ImageSize = new Size(16, 16)
      };

      // Set up the a ServiceProvider for the GeoLocator in case the user's settings 
      // indicate that the client side geoLocator must be used. 
      var mainProviderServiceLocator = this.MainXYServiceProvider.ServiceLocator;

      // A flag, driving whether we want to use the Client side defined GeoLocator
      bool useServerGeoLocator = LiteClientSettingsViewModel.Instance.UseServerGeoLocator;

      if (!useServerGeoLocator)
      {
        // In case of using the Client GeoLocator, unregister the GeoLocator service from the main
        // service provider. That way, it won't take part in determination of available geoLocators
        mainProviderServiceLocator.Unregister<IGeoLocatorService>();

        // Create the OpenStreetMap provider, providing access to the OSM GeoLocator service implementation
        // This drives the OSM's GeoLocator functionality completely from the client; for implementation, look
        // at the corresponding LiteOsmServiceProvider class and the accompanying LiteOsmGeoLocatorServiceAgent
        // class that is the IGeoLocatorService implementation talking to the Osm provider.
        // There are abstract base classes available that can be used as starting point to implement your
        // own Client side GeoLocator to other providers than the OpenStreetMap one. 
        this.OsmServiceProvider = new LiteOsmServiceProvider()
        {
          // Setup some rerouting logic to have the OSM GeoLocator service call the X&Y/GSA Server that forwards the call to the OSM provider.
          // This mechanism circumvents issues that some service provider cause by not having  a ClientAccessPolicy (handler) implemented.
          RerouteGeoLocatorVia = MainXYServiceProvider
        };
      }

      // Get the overview mode from the Client Settings
      bool useOverviewAllLayers = LiteClientSettingsViewModel.Instance.OverviewModeShowAllLayers;

      // Apply to the OverviewMapViewModel
      this.OverviewMapViewModel.BackdropOnly = !useOverviewAllLayers;
    }
    #endregion

    #region PostInitialization
    /// <summary>
    /// Post Initializaton, which means that the application has initialized fully.
    /// It is a straight callback from the current Application Startup event.
    /// </summary>
    protected override void PostInitialize()
    {
      var provider = this.MainXYServiceProvider;

      // Set the server host address on the provider
      provider.ServiceAddress = DetermineServerHostAddress();
      provider.ServicePathPostfix = GetServicePathPostfix();

      // Try to automatically log on after we've set up everything
      DoAutoSignInAsync();
    }

    /// <summary>
    /// Automatically log on, in case credentials have been used before
    /// </summary>
    private Task DoAutoSignInAsync()
    {
      // Leave the auto logon to the authentication view model
      return AuthenticationViewModel.AutoSignInAsync();
    }
    #endregion

    #region Server/Client Names and Logo Properties
    /// <summary>
    /// The Server Name
    /// </summary>
    public string ServerName
    {
      get { return SERVERNAME; }
    }

    /// <summary>
    /// The Lite Name
    /// </summary>
    public static string LiteName
    {
      get { return Application.Current.Resources["Lite.Title"] as String; }
    }

    /// <summary>
    /// The Analytics Name
    /// </summary>
    public static string AnalyticsName
    {
      get { return Application.Current.Resources["Lite.AnalyticsTitle"] as String; }
    }

    /// <summary>
    /// The Lite Name in the browser
    /// </summary>
    public static string LiteNameInBrowser
    {
      get { return Application.Current.Resources["Lite.BrowserTitle"] as String; }
    }

    /// <summary>
    /// The Lite Logo as a png resource
    /// </summary>
    public static string LiteLogo
    {
      get { return Application.Current.Resources["Lite.Logo.Bitmap"] as String; }
    }

    /// <summary>
    /// The logo as a vector/path resource
    /// </summary>
    public static string LiteVectorLogo
    {
      get { return Application.Current.Resources["Lite.Logo.Vector"] as String; }
    }
    #endregion

    #region Providers
    /// <summary>
    /// The main XY service provider
    /// </summary>
    private XYServiceProvider MainXYServiceProvider
    {
      get;
      set;
    }

    /// <summary>
    /// The OpenStreetMap service Provider
    /// </summary>
    private LiteOsmServiceProvider OsmServiceProvider
    {
      get;
      set;
    }
    #endregion

    #region Application ViewModel
    /// <summary>
    /// The application view model, holding the UI information for the active application like
    /// resources as well as the current culture
    /// </summary>
    public ApplicationViewModel ApplicationViewModel { get; set; }
    #endregion

    #region Authentication
    /// <summary>
    /// An opacity animator for animating everyting on the UI, except
    /// the Map that has its own opacity
    /// </summary>
    public OpacityAnimator AuthenticationOpacity
    {
      get;
      set;
    }

    /// <summary>
    /// The authentication view model that contains all the logic
    /// for an Application Dialog to work against.
    /// </summary>
    public AuthenticationViewModel AuthenticationViewModel
    {
      get;
      set;
    }

    /// <summary>
    /// Called whenever auto SignIn has succeeded and the context has been determined.
    /// Whether a user is allowed to automatically sign-in (instead of providing 
    /// credentials) might well be dependent on Client Settings. This method is called
    /// directly after the signing in with stored credentials has succeeded. 
    /// In case it is decided this user/group is not allowed to sign in automatically,
    /// it can be revoked here; this forces the user to sign in by providing credentials.
    /// </summary>
    /// <param name="context">The set up AuthenticationContext</param>
    /// <returns>A flag indicating whether autoSignIn is allowed for this user</returns>
    protected override bool AuthenticationIsAutoSignInAllowed(AuthenticationContext context)
    {
      return LiteClientSettingsViewModel.Instance.AllowAutoSignIn(context);
    }

    /// <summary>
    /// Called whenever the authentication is about to change. The order in which calls are made are:
    /// 
    /// ViewModelLocator - OnAuthenticationChanging
    /// 
    /// ViewModels       - OnAuthenticationChanged
    /// ViewModelLocator - OnAuthenticationChanged
    /// 
    /// This method gives the possibility to change global settings before individual viewModels are
    /// setting their state when the user has been authenticated. (ie add ServiceProviders depending
    /// on some server setting).
    /// </summary>
    protected override async Task OnAuthenticationChanging(AuthenticationContext context, bool isAuthenticated)
    {
      // Get some base behavior happening and wait for it to complete
      await base.OnAuthenticationChanging(context, isAuthenticated);

      // Clear all referenced service providers - include the local defined ones
      ServiceProviderManager.Instance.RemoveServiceProviders(includeLocalDefined: true, leaveMainProvider: true);

      // Drive the client settings actively, to make sure these are done before any
      // other viewModel kicks in
      await ClientSettingsViewModel.SetupFor(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Make sure we set up the client side GeoLocator (if it is indicated via the client
        // settings that this user uses the client-side one).
        ApplyClientSettingsToViewModels();

        // Tell the LiteAnalyticsTracker our prefered prefix for the Application.
        // Note: Do not make this language dependent, since different clients' identical categories wouldn't match anymore
        //
        // Do not use in case no category prefix is required in Analytics (ie when this is handled using account properties
        // in Google Analytics).
        // 
        // AnalyticsAppName   Category                                  Event      Label
        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        // Set                'GSA Lite (Cambridge): Authentication'   'SignIn'   'test'
        // Not Set            'Authentication'                         'SignIn'   'test'
        //
        // LiteAnalyticsTracker.AnalyticsAppName = string.Format("{0} ({1}): ", AnalyticsName, context.ServiceProviderInfo.Name ?? "");

        // Track the fact that we are signing in
        LiteAnalyticsTracker.TrackSignIn(context.UserName);

        // Get reference providers
        var referenceProviders = await this.ServerInfoViewModel.GetReferenceServiceProvidersAsync();

        foreach (var provider in referenceProviders)
        {
          ServiceProviderManager.Instance.ServiceProviders.Add(provider);
        }
      }
      else
      {
        // Let's handle the fading out as early as possible
        int fadeOutMs = 1000;

        // When signing in, there is an AuthenticationContext available.
        bool triedLoggingIn = context != null;

        // First of all - fade the application out
        AuthenticationOpacity.FadeOut(fadeOutMs);

        if (triedLoggingIn)
        {
          // Dependent on the state, track errors to the Analytics Module
          switch (context.Status)
          {
            case AuthenticationStatus.FailedConnectionError:
              var mainProvider = ServiceProviderManager.Instance.MainServiceProvider as XYServiceProvider;
              if (mainProvider != null)
              {
                // Notify the user/tracker that we have failed to connect
                var address = mainProvider.ServiceAddress ?? "?";
                var message = String.Format("{0} {1}", FrameworkResources.AuthenticationFailedConnectionError, address);

                // Use the analytics tracker
                LiteAnalyticsTracker.TrackAuthServerError(message);
              }
              break;

            case AuthenticationStatus.FailedInvalidCredentials:
              // Track in analytics
              LiteAnalyticsTracker.TrackAuthCredentialsError(context.UserName);
              break;

            case AuthenticationStatus.FailedInvalidApplication:
              // Track in analytics
              LiteAnalyticsTracker.TrackAuthInvalidApplicationError(context.UserName);
              break;

            case AuthenticationStatus.FailedInvalidApplicationUser:
              // Track in analytics
              LiteAnalyticsTracker.TrackAuthInvalidApplicationUserError(context.UserName);
              break;
          }
        }
        else // so !triedLoggingIn - deliberately signing out
        {
          // Track the signing out
          LiteAnalyticsTracker.TrackSignOut();
        }
      }
    }

    /// <summary>
    /// Called whenever the authentication has changed; act dependent on the authentication state.
    /// </summary>
    /// <param name="context">The authentication context holding information on credentials</param>
    /// <param name="isAuthenticated">A flag specifying whether the user is authenticated</param>
    protected override async void OnAuthenticationChanged(AuthenticationContext context, bool isAuthenticated)
    {
      int fadeInMs = 500;
      base.OnAuthenticationChanged(context, isAuthenticated);

      if (isAuthenticated)
      {
        // Set the new Search Postfix on the GeoLocator
        this.PlaceFinderViewModel.SearchPostfix = ClientSettingsViewModel.GeoLocatorSearchPostfix;

        // Set the maximum number of records for the result list
        this.FeatureCollectionResultViewModel.GridProperties.DefaultBatchSize = ClientSettingsViewModel.ResultListMaximumNumberOfRecords;

        // Set up the MapDefinitions, waiting (asynchronously) for completion
        await this.MapsViewModel.GetServiceProviderMapDefinitionsAsync();

        // Wait a bit to have some map information drawn, showing some more interesting
        // Map when it comes available
        await TaskEx.Delay(fadeInMs);

        // With everything loaded and set up; start showing the application
        AuthenticationOpacity.FadeIn(fadeInMs);

        // We could do something with the number of sessions currently active on this server.
        // Note that not signing off will retain the session's token active on the server (for a 
        // period equal to the lease time).
        int numberOfActiveSessions = await MainXYServiceProvider.GetActiveSessionsCountAsync();
      }

      // Only allow (visibility of) trace logging in case of admin rights
      this.TraceLogger.AllowLogging = isAuthenticated && context.HasAdminRights;
    }
    #endregion

    #region Culture Changed
    /// <summary>
    /// In case the current culture has changed, store it in the Isolated Storage manager.
    /// That way, the next time we can start up with this current again
    /// </summary>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      base.OnCurrentCultureChanged(currentCultureInfo);

      if (currentCultureInfo != null)
      {
        LiteIsolatedStorageManager.Instance.CultureName = currentCultureInfo.Name;
      }
    }
    #endregion

    #region Server Info
    /// <summary>
    /// The view model holding information on the connected server
    /// </summary>
    public LiteServerInfoViewModel ServerInfoViewModel
    {
      get;
      set;
    }

    #endregion

    #region Feature Related
    /// <summary>
    /// The view model responsible for handling display of Details of a Feature
    /// </summary>
    public LiteFeatureDetailsViewModel FeatureDetailsViewModel
    {
      get;
      set;
    }


    public EdicionSwViewModel EdicionFeatureDetails
    {
        get;
        set;
    }

    /// <summary>
    /// The view model responsible for restricting a featureCollection; this behavior is driven 
    /// entirely via the messenger (see the LiteRestrictFeatureCollectionRequestMessage).
    /// The LiteFeatureCollectionResultViewModel uses this behavior to restrict a collectin
    /// before displaying it.
    /// </summary>
    public LiteFeatureCollectionRestrictionViewModel FeatureCollectionRestrictionViewModel
    {
      get;
      set;
    }

    /// <summary>
    /// The view model responsible for displaying a feature result (list)
    /// </summary>
    public LiteFeatureCollectionResultViewModel FeatureCollectionResultViewModel
    {
      get;
      set;
    }

    /// <summary>
    /// The view model responsible for activation requests (going to a SmartLink or Geometry)
    /// </summary>
    public LiteFeatureValueActivationViewModel FeatureValueActivationViewModel
    {
      get;
      set;
    }
    #endregion

    #region Query
    /// <summary>
    /// The view model responsible for displaying/handling queries
    /// </summary>
    public LiteQueriesViewModel QueriesViewModel
    {
      get;
      set;
    }
    #endregion

    #region Maps Related
    /// <summary>
    /// The viewModel holding all maps
    /// </summary>
    public LiteMapsViewModel MapsViewModel { get; set; }

    /// <summary>
    /// The viewModel controlling the behavior of the MapBar
    /// </summary>
    public LiteMapBarViewModel MapBarViewModel { get; set; }

    /// <summary>
    /// The viewModel handling the logic for the overview map
    /// </summary>
    public LiteOverviewMapViewModel OverviewMapViewModel { get; set; }

    /// <summary>
    /// The view model tracking Hover events in the current map
    /// </summary>
    public LiteMapHoverViewModel MapHoverViewModel { get; set; }

    /// <summary>
    /// The view model tracking Select-Hover (press and hold) events in the current map
    /// </summary>
    public LiteMapSelectHoverViewModel MapSelectHoverViewModel { get; set; }

    /// <summary>
    /// The view model that handles custom selection in case nothing is selected on the map
    /// when the user presses the mouse
    /// </summary>
    public LiteMapCustomSelectionViewModel MapCustomSelectionViewModel { get; set; }

    /// <summary>
    /// The View Model that handles recently visited (named) items
    /// </summary>
    public LiteMapRecentlyVisitedItemViewModel MapRecentlyVisitedItemViewModel { get; set; }

    /// <summary>
    /// The viewModel that handles the active mode for backdrop layers
    /// </summary>
    public LiteMapBackdropLayerModeSelectionViewModel MapBackdropLayerModeSelectionViewModel { get; set; }

    /// <summary>
    /// The Themes view model, allowing control of displayed maps and layers
    /// </summary>
    public LiteMapThemesViewModel MapThemesViewModel { get; set; }

    /// <summary>
    /// The view model responsible for the measure interaction modes
    /// </summary>
    public LiteMapMeasureViewModel MapMeasureViewModel { get; set; }
    public LiteMapMeasureISViewModel MapMeasureISViewModel { get; set; }

    /// <summary>
    /// The viewModel for handling trail commands and interaction
    /// </summary>
    public LiteMapTrailViewModel MapTrailViewModel { get; set; }
    public MapTrailISViewModel MapTrailISViewModel { get; set; }
    public MapPointISViewModel MapPointISViewModel { get; set; }

    /// <summary>
    /// The viewModel responsible for editing geometry in the map
    /// </summary>
    public LiteMapEditGeometryViewModel MapEditGeometryViewModel { get; set; }

    /// <summary>
    /// The view model responsible for the map's popup events
    /// </summary>
    public LiteMapPopupMenuViewModel MapPopupViewModel { get; set; }

    /// <summary>
    /// The Street View view model for interacting with the Google Street View API
    /// </summary>
    public StreetViewViewModel StreetViewViewModel { get; set; }

    /// <summary>
    /// The view model responsible for getting and managing the insertable collection
    /// </summary>
    public LiteFeatureInsertViewModel FeatureInsertViewModel { get; set; }

    /// <summary>
    /// The viewModel for retrieving changes from the server
    /// </summary>
    public LiteMapLayerChangesRetriever MapLayerChangeRetriever { get; set; }
    #endregion

    #region Feature Related
    /// <summary>
    /// The place finder view model, wrapping the GeoLocator functionality with some 
    /// previous search results.
    /// </summary>
    public LiteMapPlaceFinderViewModel PlaceFinderViewModel { get; set; }
    #endregion

    #region PrintViewModel
    /// <summary>
    /// The print view model holding the templates that are available for client-side printing
    /// </summary>
    public LitePrintViewModel PrintViewModel { get; set; }
    #endregion

    #region MessageBox
    /// <summary>
    /// The view model taking care of global MessageService requests
    /// </summary>
    public LiteMessageBoxViewModel MessageBoxViewModel { get; set; }

    /// <summary>
    /// The view model taking care of global MessageService requests
    /// </summary>
    public LiteMessageBoxViewModel ToolboxMessageBoxViewModel { get; set; }
    #endregion

    #region Factibilidad
    public LiteFactibilidadView FactibilidadViewModel
    {
        get;
        set;
    }

    public LiteMapMarkViewModel MapMarkViewModel
    {
        get;
        set;
    }


    public LiteInventarioCajaViewModel InventarioCajaViewModel
    {
        get;
        set;
    }


    public LiteMapMarkISViewModel MapMarkISViewModel
    {
        get;
        set;
    }


    public EdicionSwView EdicionSwViews
    {
        get;
        set;
    }

    public MapTrailISViewModel MapTrailISVM
    { get; set; }

      
    #endregion

    #region Client Settings
    /// <summary>
    /// The Client Settings viewModel holding all settings as defined via the
    /// Management Client for the active user
    /// </summary>
    public LiteClientSettingsViewModel ClientSettingsViewModel { get; set; }
    #endregion

    #region Trace Logger
    /// <summary>
    /// The tracelogger
    /// </summary>
    public LiteTraceLogger TraceLogger
    {
      get;
      set;
    }
    #endregion
  }
}
