using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using SpatialEye.Framework.ComponentModel.Design;
using SpatialEye.Framework.Client.Converters;

namespace Lite
{
  /// <summary>
  /// The application class that is the entry class for the application
  /// </summary>
  public partial class App : Application
  {
    #region Constructors
    /// <summary>
    /// Default Constructor
    /// </summary>
    public App()
    {
      if (!DesignModeHelper.IsInDesignMode)
      {
        // Setup some application callbacks
        this.Startup += this.Application_Startup;
        this.Exit += this.Application_Exit;
        this.UnhandledException += this.Application_UnhandledException;

        // Intitialize the base resources (the rest will be done on startup)
        InitializeBaseResources();
      }
      else
      {
        // Initinalize the application (only in design mode)
        InitializeComponent();
      }
    }

    #endregion

    #region Callbacks
    /// <summary>
    /// Callback when the application starts up
    /// </summary>
    private void Application_Startup(object sender, StartupEventArgs e)
    {
      // Store the startup params as resources
      if (e.InitParams != null)
      {
        foreach (var item in e.InitParams)
        {
          // Add a new resource (with a lowercase key)
          this.Resources.Add(item.Key.ToLower(), item.Value);
        }
      }

      // Load the application
      StartupApplication();
    }

    /// <summary>
    /// Load the dictionaries which contains branded resources
    /// </summary>
    private async void StartupApplication()
    {
      // Load the remote resource library
      // No problem if it fails, the file could be missing or corrupt
      try
      {
        WebClient client = new WebClient();
        var result = await client.DownloadStringTaskAsync(GetSourceUriFor("LiteResources.xaml"));

        if (!String.IsNullOrEmpty(result))
        {
          var dict = XamlReader.Load(result) as ResourceDictionary;

          if (dict != null)
          {
            foreach (var resourceKey in dict.Keys)
            {
              // Replace existing resources with the remote ones, do not add new ones
              if (Application.Current.Resources.Contains(resourceKey))
              {
                object resourceValue = dict[resourceKey];
                var resourceStringValue = resourceValue as String;

                // Skip empty values, keep original resources
                if (!String.IsNullOrWhiteSpace(resourceStringValue))
                {
                  // Special case, the bitmap logo. Replace the filename with the full uri
                  var resourceKeyString = ((String)resourceKey).ToLower();
                  if (resourceKeyString == "lite.logo.bitmap")
                  {
                    resourceValue = GetSourceUriFor(Path.GetFileName(resourceStringValue)).ToString();
                  }

                  Application.Current.Resources.Remove(resourceKey);
                  Application.Current.Resources.Add(resourceKey, resourceValue);

                }
                else if (resourceValue is Color)
                {
                  // Get the color
                  var resourceColorValue = (Color)resourceValue;
                  Application.Current.Resources.Remove(resourceKey);
                  Application.Current.Resources.Add(resourceKey, resourceValue);

                  // Check whether this is a Lite.Color
                  var resKey = resourceKey.ToString();

                  var colorPrefix = "Lite.Color";
                  var brushPrefix = "Lite.Brush";
                  if (resKey.StartsWith(colorPrefix))
                  {
                    var brushName = string.Concat(brushPrefix, resKey.Substring(colorPrefix.Length));
                    if (Application.Current.Resources.Contains(brushName))
                    {
                      Application.Current.Resources.Remove(brushName);
                      Application.Current.Resources.Add(brushName, new SolidColorBrush(resourceColorValue));
                    }
                  }
                }
              }
            }
          }
        }
      }
      catch
      { }

      // Load the rest of the resource dictionaries
      LoadResourceDictionaries(true);

      // Set the browser document title
      BrowserAPI.Instance.SetDocumentTitle(ViewModelLocator.LiteNameInBrowser);

      // Startup the application
      this.RootVisual = new MainPage();

      // Setup the mouse manager after the rootvisual is set
      SpatialEye.Framework.Client.MouseManager.Instance.Setup();
    }

    /// <summary>
    /// Callback when the application exists
    /// </summary>
    private void Application_Exit(object sender, EventArgs e)
    { }
    #endregion

    #region Resource Handling

    /// <summary>
    ///  Load the theme resource dictionaries
    /// </summary>
    private void InitializeBaseResources()
    {
      //// First load the theme colors
      AddResourceDictionary(new Uri("/Lite;component/Resources/Themes/ColorScheme1.xaml", UriKind.Relative));
      // Load the internal branded resources
      AddResourceDictionary(new Uri(ViewModelLocator.LITERESOURCEFILE, UriKind.Relative));

      // Some globals
      this.Resources.Add("UnitResourceConverter", new UnitDefinitionToResourceStringConverter());
      this.Resources.Add("Locator", new ViewModelLocator());
    }

    /// <summary>
    /// Load the dictionaries which contains shared resources
    /// </summary>
    private void LoadResourceDictionaries(bool loadTheme = false)
    {
      var sharedResources = new Uri[] 
      {
        new Uri("/Lite;component/Resources/Common/SharedResources.xaml", UriKind.Relative),
        new Uri("/Lite;component/Resources/Framework/SharedResources.xaml", UriKind.Relative),
        new Uri("/Lite;component/Resources/Application/SharedResources.xaml", UriKind.Relative),
        
        new Uri("/Lite;component/Resources/Common/Resources.xaml", UriKind.Relative),
        new Uri("/Lite;component/Resources/Framework/Resources.xaml", UriKind.Relative),
        new Uri("/Lite;component/Resources/Application/Resources.xaml", UriKind.Relative)
      };

      foreach (var lib in sharedResources)
      {
        AddResourceDictionary(lib);
      }
    }

    /// <summary>
    /// Adds the given dictionary to the application dictionaries
    /// </summary>
    /// <param name="dictionary">dictionary to add</param>
    private void AddResourceDictionary(ResourceDictionary dictionary)
    {
      if (dictionary != null)
      {
        Resources.MergedDictionaries.Add(dictionary);
      }
    }

    /// <summary>
    /// Adds the given dictionary to the application dictionaries
    /// </summary>
    /// <param name="dictionary">dictionary to add</param>
    private void RemoveResourceDictionary(Uri sharedResourceLocation)
    {
      var sharedLocationString = sharedResourceLocation.ToString();
      var dictionary = Resources.MergedDictionaries.FirstOrDefault(md => sharedLocationString.EndsWith(md.Source.ToString()));

      if (dictionary != null)
      {
        dictionary.Clear();
        Resources.MergedDictionaries.Remove(dictionary);
      }
    }

    /// <summary>
    /// Adds the given dictionary to the application dictionaries
    /// </summary>
    /// <param name="uri">uri of the dictionary to add</param>
    private void AddResourceDictionary(Uri uri)
    {
      AddResourceDictionary(LoadResourceDictionary(uri));
    }

    /// <summary>
    /// Manually load a resource dictionary
    /// </summary>
    /// <param name="uri"></param>
    private ResourceDictionary LoadResourceDictionary(Uri uri)
    {
      ResourceDictionary dict = null;

      try
      {
        var info = Application.GetResourceStream(uri);

        if (info != null)
        {
          string xaml;
          using (var reader = new StreamReader(info.Stream))
          {
            xaml = reader.ReadToEnd();
          }

          var xamlLoad = XamlReader.Load(xaml);
          dict = xamlLoad as ResourceDictionary;
        }
      }
      catch { }

      return dict;
    }

    /// <summary>
    /// Return the source uri for the give resource using the local path
    /// </summary>
    /// <param name="resourceName">name of the resource to locate</param>
    /// <returns>full uri</returns>
    public static Uri GetSourceUriFor(string resourceName)
    {
      var builder = new UriBuilder(Application.Current.Host.Source);
      builder.Fragment = null;
      builder.Query = null;

      builder.Path = Path.Combine(Path.GetDirectoryName(builder.Path), resourceName);

      return builder.Uri;
    }
    #endregion

    #region Exception Handling
    /// <summary>
    /// Unhandled exceptions are routed via this handler, it is the point to intervene 
    /// </summary>
    private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
    {
      // If the app is running outside of the debugger then report the exception using
      // the browser's exception mechanism. On IE this will display it a yellow alert 
      // icon in the status bar and Firefox will display a script error.
      if (!System.Diagnostics.Debugger.IsAttached)
      {
        // NOTE: This will allow the application to continue running after an exception has been thrown
        // but not handled. 
        // For production applications this error handling should be replaced with something that will 
        // report the error to the website and stop the application.
        e.Handled = true;
        Deployment.Current.Dispatcher.BeginInvoke(delegate { ReportErrorToDOM(e); });
      }
    }

    /// <summary>
    /// Reports the error to the outside world
    /// </summary>
    /// <param name="e"></param>
    private void ReportErrorToDOM(ApplicationUnhandledExceptionEventArgs e)
    {
      string errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
      BrowserAPI.Instance.ReportErrorToDOM(errorMsg);
    }

    #endregion
  }
}
