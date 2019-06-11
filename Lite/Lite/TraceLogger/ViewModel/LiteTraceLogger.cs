using System;
using System.Windows;

using SpatialEye.Framework.Client.Diagnostics;

namespace Lite
{
  /// <summary>
  /// The Lite TraceLogger is the implementation of a trace logger that
  /// can display trace messages (of the framework) to a view
  /// </summary>
  public class LiteTraceLogger : TraceLogger
  {
    #region Static
    /// <summary>
    /// The trace on message
    /// We are not using resources - is internal debugging code
    /// </summary>
    private static string TraceOn = "User Action - Trace Logger switched on";

    /// <summary>
    /// The trace off message
    /// We are not using resources - is internal debugging code
    /// </summary>
    private static string Traceoff = "User Action - Trace Logger switching off";

    /// <summary>
    /// The logger categories to output
    /// </summary>
    private static string[] Categories = new[] { "Error", "Error", "Warning", "Info", "Trace", "Trace" };

    /// <summary>
    /// The indentation levels
    /// </summary>
    private static string[] Indentation = new[] { "", "", "", "", "  ", "    " };

    /// <summary>
    /// The visibility for Logging Enabled items
    /// </summary>
    public static string LoggingEnabledVisibilityPropertyName = "LoggingEnabledVisibility";
    /// <summary>
    /// The allow log visibility
    /// </summary>
    public static string AllowLogVisibilityPropertyName = "AllowLogVisibility";
    #endregion

    #region Static Names
    /// <summary>
    /// The cached lite name for easier access
    /// </summary>
    private static string _liteName;

    /// <summary>
    /// Holds the name of Lite (in the browser)
    /// </summary>
    private static string LiteName
    {
      get
      {
        if (String.IsNullOrEmpty(_liteName))
        {
          _liteName = ViewModelLocator.LiteNameInBrowser;
        }

        return _liteName;
      }
    }
    #endregion

    #region Private Fields
    /// <summary>
    /// Do we allow logging (or at least its visibility)
    /// </summary>
    private bool _loggingEnabled;

    /// <summary>
    /// Do we allow logging
    /// </summary>
    private bool _allowLogging;
    #endregion

    #region Constructor
    /// <summary>
    /// The default constructor
    /// </summary>
    public LiteTraceLogger()
    {
      // The default log level
      LogLevel = 4;
    }
    #endregion

    #region TraceLogger API
    /// <summary>
    /// Write a fatal exception to the logging system
    /// </summary>
    public override void WriteCritical(string message, Boolean onlyInDebugMode)
    {
      AddMessage(1, message);
    }

    /// <summary>
    /// Write an exception to the logging system
    /// </summary>
    public override void WriteException(string message, System.Exception exception, Boolean onlyInDebugMode)
    {
      AddMessage(1, message);
    }

    /// <summary>
    /// Write an error to the logging system (level 1)
    /// </summary>
    public override void WriteError(string message, Boolean onlyInDebugMode)
    {
      AddMessage(1, message);
    }

    /// <summary>
    /// Write a warning to the logging system (level 2)
    /// </summary>
    public override void WriteWarning(string message, Boolean onlyInDebugMode)
    {
      AddMessage(2, message);
    }

    /// <summary>
    /// Write an informational message to the logging system (level 3)
    /// </summary>
    public override void WriteInfo(string message, Boolean onlyInDebugMode)
    {
      AddMessage(3, message);
    }

    /// <summary>
    /// Write a trace message to the logging system (level 4)
    /// </summary>
    public override void WriteVerbose(string message, Boolean onlyInDebugMode)
    {
      AddMessage(4, message);
    }

    /// <summary>
    /// Write a debug message to the logging system (level 5)
    /// </summary>
    public override void WriteInternal(string message, Boolean onlyInDebugMode)
    {
      AddMessage(5, message);
    }
    #endregion

    #region Lite TraceLogger
    /// <summary>
    /// Adds the message to the total set of messages to be displayed
    /// </summary>
    private void AddMessage(int level, string message)
    {
      if (Enabled)
      {
        if (LogLevel >= level)
        {
          var indent = level >= 0 && level < Indentation.Length ? Indentation[level] : string.Empty;
          var category = level >= 0 && level < Categories.Length ? Categories[level] : string.Empty;
          var totalMessage = string.Format("{0}>  {1} {3} {4}", LiteName, DateTime.Now.ToString(), category.PadRight(15), indent, message);

          // Write to the JS Console
          BrowserAPI.Instance.WriteToJSConsole(level, totalMessage);
        }
      }
    }

    /// <summary>
    /// Do we allow logging
    /// </summary>
    public bool AllowLogging
    {
      get { return _allowLogging; }
      set
      {
        if (_allowLogging != value)
        {
          // If we are not allowed to log - switch it off
          if (!value)
          {
            Enabled = false;
          }

          _allowLogging = value;
        }
      }
    }

    /// <summary>
    /// Do we allow logging (to be visible)
    /// </summary>
    public bool Enabled
    {
      get { return _loggingEnabled; }
      set
      {
        if (_loggingEnabled != value && AllowLogging)
        {
          if (_loggingEnabled)
          {
            WriteInfo(Traceoff, false);
          }

          _loggingEnabled = value;

          // Notify changes in view visibility
          RaisePropertyChanged(LoggingEnabledVisibilityPropertyName);

          if (_loggingEnabled)
          {
            WriteInfo(TraceOn, false);
          }
        }
      }
    }

    /// <summary>
    /// The visibility of a Logging Enabled presentation
    /// </summary>
    public Visibility LoggingEnabledVisibility
    {
      get { return Enabled ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Specify the level to log;
    /// 1: Error
    /// 2: Warning
    /// 3: Info
    /// 4: Trace          
    /// 5: Trace Internal
    /// </summary>
    public int LogLevel
    {
      get;
      set;
    }
    #endregion
  }
}
