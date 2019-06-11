using Lite.Resources.Localization;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Client.Analytics;

namespace Lite
{
  /// <summary>
  /// The analytics tracker for Lite uses the framework's AnalyticsTracker
  /// functionality to take care of the actual tracking. 
  /// Provides a unified/typed API for tracking usage of the Lite application
  /// </summary>
  public static class LiteAnalyticsTracker
  {
    #region Set Server/Application Name
    /// <summary>
    /// Holds a combination of ServerName/AppName as a prefix for the Category
    /// </summary>
    public static string AnalyticsAppName = string.Empty;
    #endregion

    #region Enums
    /// <summary>
    /// The categories to track for
    /// </summary>
    public enum Category
    {
      Authentication,
      Maps,
      Data
    }

    /// <summary>
    /// The actions corresponding with the categories
    /// </summary>
    public enum Action
    {
      SignIn,
      SignOut,
      Error,
      View,
      Create,
      Query,
      Report,
      Export,
      Print
    }

    /// <summary>
    /// The actions corresponding with the categories
    /// </summary>
    public enum Source
    {
      Queries,
      Details,
      ResultList,
      Map
    }
    #endregion

    #region Static Fields
    /// <summary>
    /// The last tracked sign in name, to be used when signing out
    /// </summary>
    private static string _trackedSignInName = string.Empty;
    #endregion

    #region Generic API
    /// <summary>
    /// Returns the category prefixed with a combination of server and
    /// application name.
    /// </summary>
    private static string ResolvedCategoryName(string categoryName)
    {
      if (!string.IsNullOrEmpty(AnalyticsAppName))
      {
        return string.Format("{0}{1}", AnalyticsAppName, categoryName);
      }

      // Backstop to the category name
      return categoryName;
    }

    /// <summary>
    /// Returns the category prefixed with a combination of server and
    /// application name.
    /// </summary>
    private static string ResolvedCategoryName(Category category)
    {
      return ResolvedCategoryName(category.ToString());
    }

    /// <summary>
    /// Tracks the specified category and action using the default
    /// Analytics Tracker
    /// </summary>
    public static void TrackEvent(Category category, Action action)
    {
      TrackEvent(category, action.ToString());
    }

    /// <summary>
    /// Tracks the specified category and action using the default
    /// Analytics Tracker
    /// </summary>
    public static void TrackEvent(Category category, string action)
    {
      var categoryName = ResolvedCategoryName(category);
      var actionName = action;

      LiteTraceLogger.Instance.WriteInfo(string.Format("User Action - {0}-{1}", categoryName, actionName));
      AnalyticsTracker.Instance.TrackEvent(categoryName, actionName);
    }

    /// <summary>
    /// Tracks the specified category, action and label using the default
    /// Analytics Tracker
    /// </summary>
    public static void TrackEvent(Category category, Action action, string label)
    {
      TrackEvent(category, action.ToString(), label);
    }

    /// <summary>
    /// Tracks the specified category, action and label using the default
    /// Analytics Tracker
    /// </summary>
    public static void TrackEvent(Category category, string action, string label)
    {
      var categoryName = ResolvedCategoryName(category);
      var actionName = action;

      LiteTraceLogger.Instance.WriteInfo(string.Format("User Action - {0}-{1}: {2}", categoryName, actionName, label));
      AnalyticsTracker.Instance.TrackEvent(categoryName, actionName, label);
    }

    /// <summary>
    /// Tracks the specified category, action, label and value using the default
    /// Analytics Tracker
    /// </summary>
    public static void TrackEvent(Category category, Action action, string label, int value)
    {
      TrackEvent(category, action.ToString(), label, value);
    }

    /// <summary>
    /// Tracks the specified category, action, label and value using the default
    /// Analytics Tracker
    /// </summary>
    public static void TrackEvent(Category category, string action, string label, int value)
    {
      var categoryName = ResolvedCategoryName(category);
      var actionName = action.ToString();

      LiteTraceLogger.Instance.WriteInfo(string.Format("User Action - {0}-{1}: {2} {3}", categoryName, actionName, label, value.ToString()));
      AnalyticsTracker.Instance.TrackEvent(categoryName, actionName, label, value);
    }
    #endregion

    #region Lite Specific API
    /// <summary>
    /// Track the Sign-In event
    /// </summary>
    /// <param name="userName">The name of the user</param>
    public static void TrackSignIn(string userName)
    {
      _trackedSignInName = userName;
      TrackEvent(Category.Authentication, Action.SignIn, _trackedSignInName);
    }

    /// <summary>
    /// Track the Sign-Out event
    /// </summary>
    public static void TrackSignOut()
    {
      if (!string.IsNullOrEmpty(_trackedSignInName))
      {
        TrackEvent(Category.Authentication, Action.SignOut, _trackedSignInName);
      }

      // Set the tracked signIn name to empty
      _trackedSignInName = string.Empty;
    }

    /// <summary>
    /// Track the server connection error
    /// </summary>
    public static void TrackAuthServerError(string message)
    {
      TrackEvent(Category.Authentication, Action.Error, message);
    }

    /// <summary>
    /// Track the credentials error
    /// </summary>
    public static void TrackAuthCredentialsError(string userName)
    {
      var message = string.Format("Credentials for user {0}", userName);
      TrackEvent(Category.Authentication, Action.Error, message);
    }

    /// <summary>
    /// Track the credentials error
    /// </summary>
    public static void TrackAuthInvalidApplicationError(string userName)
    {
      var message = string.Format("Application for user {0}", userName);
      TrackEvent(Category.Authentication, Action.Error, message);
    }

    /// <summary>
    /// Track the credentials error
    /// </summary>
    public static void TrackAuthInvalidApplicationUserError(string userName)
    {
      var message = string.Format("Application for user {0}", userName);
      TrackEvent(Category.Authentication, Action.Error, message);
    }

    /// <summary>
    /// Track viewing a specific map
    /// </summary>
    public static void TrackMapView(MapViewModel map)
    {
      if (map != null)
      {
        TrackEvent(Category.Maps, Action.View, map.ExternalName);
      }
    }

    /// <summary>
    /// Track viewing a specific map
    /// </summary>
    public static void TrackMapPrint()
    {
      TrackEvent(Category.Maps, Action.Print);
    }

    /// <summary>
    /// Track viewing a specific map
    /// </summary>
    public static void TrackMapCreate(MapViewModel map)
    {
      if (map != null)
      {
        TrackEvent(Category.Maps, Action.Create, map.ExternalName);
      }
    }

    /// <summary>
    /// Track the report creation
    /// </summary>
    public static void TrackQuery(LiteQueryViewModel query, Source source)
    {
      TrackEvent(Category.Data, Action.Query, string.Format("{0} from {1}", query.Name, source.ToString()));
    }

    /// <summary>
    /// Track the report creation
    /// </summary>
    public static void TrackQueryCreate(LiteQueryViewModel query)
    {
      TrackEvent(Category.Data, Action.Create, query.Name);
    }

    /// <summary>
    /// Track the report creation
    /// </summary>
    public static void TrackReport(ReportViewModel report, Source source)
    {
      TrackEvent(Category.Data, Action.Report, string.Format("{0} from {1}", report.Name, source.ToString()));
    }

    /// <summary>
    /// Track the export creation
    /// </summary>
    public static void TrackExport(ExportViewModel export, Source source)
    {
      TrackEvent(Category.Data, Action.Export, string.Format("{0} from {1}", export.Name, source.ToString()));
    }
    #endregion
  }
}
