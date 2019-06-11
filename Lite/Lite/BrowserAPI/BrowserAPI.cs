using SpatialEye.Framework.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lite
{
  /// <summary>
  /// API to communication to and from the html page this silverlight application is running in.
  /// </summary>
  public class BrowserAPI
  {
    #region Private Statics
    /// <summary>
    /// The name for registration
    /// </summary>
    private static string ScriptingName = "SeAPI";

    /// <summary>
    /// Singleton instance
    /// </summary>
    private static BrowserAPI _instance;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    public BrowserAPI(string scriptingName)
    {
      HtmlPage.RegisterScriptableObject(scriptingName, this);
    }

    /// <summary>
    /// Singleton Instance
    /// </summary>
    public static BrowserAPI Instance
    {
      get
      {
        if (_instance == null)
        {
          _instance = new BrowserAPI(ScriptingName);
        }

        return _instance;
      }
    }

    #endregion

    #region Error Reporting

    /// <summary>
    /// Report error to HTML DOM
    /// </summary>
    /// <param name="errorMsg">message to report</param>
    public void ReportErrorToDOM(string errorMsg)
    {
      try
      {
        errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");
        HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
      }
      catch (Exception)
      { }
    }

    #endregion

    #region Document Title

    /// <summary>
    /// Set the HTML Document title
    /// </summary>
    /// <param name="title"></param>
    public void SetDocumentTitle(string title)
    {
      try
      {
        // Set the page title, title property should be in lower case
        HtmlPage.Document.SetProperty("title", title);
      }
      catch
      { }
    }

    #endregion

    #region Javascript Function Creation

    /// <summary>
    /// Functions created
    /// </summary>
    private static HashSet<string> _jsFunctionIsCreated = new HashSet<string>();

    /// <summary>
    /// The sync root object
    /// </summary>
    private static object _jsFunctionisCreatedSyncRoot = new object();

    /// <summary>
    /// Returns a flag indicating whether the specified function is defined
    /// </summary>
    private static bool JSFunctionNeedsCreation(string functionName)
    {
      var needToCreate = !_jsFunctionIsCreated.Contains(functionName);
      if (needToCreate)
      {
        var isUndefined = HtmlPage.Window.Eval(string.Format("typeof({0}) == 'undefined'", functionName));
        needToCreate = isUndefined is bool && (bool)isUndefined;

        if (!needToCreate)
        {
          lock (_jsFunctionisCreatedSyncRoot)
          {
            // Store automatically
            if (!_jsFunctionIsCreated.Contains(functionName))
            {
              _jsFunctionIsCreated.Add(functionName);
            }
          }
        }
      }

      return needToCreate;
    }

    /// <summary>
    /// Returns the script string for the specified Lite function name and JS function name
    /// </summary>
    private static bool JSCreateFunction(string functionName, string functionDefinition)
    {
      bool created = false;
      if (!_jsFunctionIsCreated.Contains(functionName))
      {
        // Store automatically
        lock (_jsFunctionisCreatedSyncRoot)
        {
          if (!_jsFunctionIsCreated.Contains(functionName))
          {
            if (JSFunctionNeedsCreation(functionName))
            {
              HtmlPage.Window.Eval(functionDefinition);
            }
            _jsFunctionIsCreated.Add(functionName);
            created = true;
          }
        }
      }

      return created;
    }

    #endregion

    #region Console Logging

    /// <summary>
    /// Is there a JS console available
    /// </summary>
    private static bool? _jsIsConsoleAvailable;

    /// <summary>
    /// The js function names
    /// </summary>
    private static string[] JSLogFunctionNames = new[] { "error", "error", "warn", "info", "log", "log", "log" };

    /// <summary>
    /// The lite function names to wrap js functions
    /// </summary>
    private static string[] LogFunctionNames = new[] { "liteerror", "liteerror", "litewarn", "liteinfo", "litelog", "litelog", "litelog" };

    /// <summary>
    /// Returns a flag indicating whether the JS Console is available
    /// </summary>
    private static bool JSIsConsoleAvailable()
    {
      if (!_jsIsConsoleAvailable.HasValue)
      {
        var isConsoleAvailable = HtmlPage.Window.Eval("typeof(console) != 'undefined' && typeof(console.log) != 'undefined'");
        _jsIsConsoleAvailable = isConsoleAvailable is bool && (bool)isConsoleAvailable;
      }

      return _jsIsConsoleAvailable.Value;
    }

    /// <summary>
    /// Returns the script string for the specified Lite function name and JS function name
    /// </summary>
    private static void JSCreateConsoleFunction(string functionName, string jsFunctionName)
    {
      string logFunction = string.Concat("function ", functionName, "(msg) { console.", jsFunctionName, "(msg); }");
      var code = string.Format(@"if(window.execScript) {{ window.execScript('{0}'); }} else {{ eval.call(null, '{0}'); }}", logFunction);
      JSCreateFunction(functionName, code);
    }

    /// <summary>
    /// Write to the JS Console
    /// </summary>
    public void WriteToJSConsole(int level, string message)
    {
      if (JSIsConsoleAvailable())
      {
        level = Math.Min(Math.Max(0, level), LogFunctionNames.Length - 1);

        var logFunctionName = LogFunctionNames[level];

        if (JSFunctionNeedsCreation(logFunctionName))
        {
          JSCreateConsoleFunction(logFunctionName, JSLogFunctionNames[level]);
        }

        var logger = HtmlPage.Window.Eval(logFunctionName) as ScriptObject;

        if (logger != null)
        {
          // Workaround: Cannot call InvokeSelf outside of UI thread, without dispatcher
          UIDispatcher.BeginInvoke(() => logger.InvokeSelf(message));
        }
      }
    }

    #endregion

    #region StreetView

    /// <summary>
    /// Is there a JS streetview available
    /// </summary>
    private static bool? _jsIsStreetViewAvailable;

    /// <summary>
    /// Is there a JS streetview available
    /// </summary>
    private static bool _jsStreetViewFunctionsCreated;

    /// <summary>
    /// The delegate used for the StreetViewStatus event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void StreetViewStatusChangedDelegate(object sender, BrowserFunctionStatusEventArgs e);

    /// <summary>
    /// Raised in the case the StreetView Api returns a location status
    /// </summary>
    public event StreetViewStatusChangedDelegate StreetViewStatusChanged;

    /// <summary>
    /// Returns a flag indicating whether the JS Street View is available
    /// </summary>
    public bool JSIsStreetViewAvailable()
    {
      if (!_jsIsStreetViewAvailable.HasValue)
      {
        var isStreetViewAvailable = HtmlPage.Window.Eval("typeof(google) != 'undefined' && typeof(seApplicationHost) != 'undefined' && document.getElementById('map_canvas') != null && document.getElementById('map_event') != null ");
        _jsIsStreetViewAvailable = isStreetViewAvailable is bool && (bool)isStreetViewAvailable;
      }

      return _jsIsStreetViewAvailable.Value;
    }

    /// <summary>
    /// Called by the JavaScript API when the Street View API returns an image status
    /// </summary>
    /// <param name="message"></param>
    [ScriptableMember]
    public void JSStreetViewStatusChanged(bool available)
    {
      OnStreetViewStatusChanged(available);
    }

    /// <summary>
    /// The Street View status has changed
    /// </summary>
    protected virtual void OnStreetViewStatusChanged(Boolean available)
    {
      var handler = StreetViewStatusChanged;
      if (handler != null)
      {
        handler(this, new BrowserFunctionStatusEventArgs(available));
      }
    }

    /// <summary>
    /// Show the streetview for the given coord on the screen
    /// </summary>
    /// <param name="coord">coord for street view</param>
    public bool ShowStreetView(double coordX, double coordY)
    {
      var result = true;
      try
      {
        if (JSIsStreetViewAvailable())
        {
          JSCreateStreetViewFunctions();
          HtmlPage.Window.Invoke("showStreetviewForLocation", coordX, coordY,  Application.Current.Host.Settings.Windowless);
        }
      }
      catch (Exception)
      {
        // Invoke can fail if JS function is not defined
        result = false;
      }

      return result;
    }

    /// <summary>
    /// Check if street view has data for the given coord
    /// </summary>
    /// <param name="coord">coord to check</param>
    public bool CheckStreetViewStatus(double coordX, double coordY)
    {
      var result = true;
      try
      {
        if (JSIsStreetViewAvailable())
        {
          JSCreateStreetViewFunctions();
          HtmlPage.Window.Invoke("checkStreetviewForLocation", coordX, coordY);
        }
      }
      catch (Exception)
      {
        // Invoke can fail if JS function is not defined
        result = false;
      }

      return result;
    }

    private void JSCreateStreetViewFunctions()
    {
      if (!_jsStreetViewFunctionsCreated)
      {
        var created = false;

        using (var stream = typeof(BrowserAPI).Assembly.GetManifestResourceStream("Lite.Resources.JavaScript.GoogleStreetView.GoogleStreetViewCheck.js"))
        {
          created = JSCreateFunction("checkStreetviewForLocation", new StreamReader(stream).ReadToEnd());
        }

        // Just created also create the remaining
        if (created)
        {
          using (var stream = typeof(BrowserAPI).Assembly.GetManifestResourceStream("Lite.Resources.JavaScript.GoogleStreetView.GoogleStreetViewGlobals.js"))
          {
            JSCreateFunction("defineGlobals", new StreamReader(stream).ReadToEnd());
          }

          using (var stream = typeof(BrowserAPI).Assembly.GetManifestResourceStream("Lite.Resources.JavaScript.GoogleStreetView.GoogleStreetViewStatus.js"))
          {
            JSCreateFunction("setStreetviewStatus", new StreamReader(stream).ReadToEnd());
          }

          using (var stream = typeof(BrowserAPI).Assembly.GetManifestResourceStream("Lite.Resources.JavaScript.GoogleStreetView.GoogleStreetViewForLocation.js"))
          {
            JSCreateFunction("showStreetviewForLocation", new StreamReader(stream).ReadToEnd());
          }
        }

        _jsStreetViewFunctionsCreated = true;
      }
    }

    #endregion

  }
}
