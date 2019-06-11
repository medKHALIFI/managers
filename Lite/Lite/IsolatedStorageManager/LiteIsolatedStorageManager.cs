using System.Linq;
using System.Collections.Generic;
using System.IO.IsolatedStorage;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ComponentModel.Design;
using SpatialEye.Framework.Transactions;

namespace Lite
{
  /// <summary>
  /// The client side storage manager, handling storage of settings that
  /// need to survice the session.
  /// </summary>
  public class LiteIsolatedStorageManager : IsolatedStorageManager
  {
    #region Static Members
    /// <summary>
    /// The instance
    /// </summary>
    public static LiteIsolatedStorageManager _clientInstance;
    #endregion

    #region API
    /// <summary>
    /// The singleton, to be used for accessing the storage
    /// </summary>
    public static new LiteIsolatedStorageManager Instance
    {
      get
      {
        if (_clientInstance == null)
        {
          _clientInstance = new LiteIsolatedStorageManager();
        }
        return _clientInstance as LiteIsolatedStorageManager;
      }
    }
    #endregion

    #region Culture
    /// <summary>
    /// The name to store the selected culture under
    /// </summary>
    private readonly static string CulturePropertyName = "Culture";

    /// <summary>
    /// The default culture to use
    /// </summary>
    private readonly static string DefaultCulture = "";

    /// <summary>
    /// Returns the name of the last used Culture
    /// </summary>
    public string CultureName
    {
      get
      {
        if (DesignModeHelper.IsInDesignMode)
        {
          return DefaultCulture;
        }

        var settings = IsolatedStorageSettings.ApplicationSettings;
        string result;
        return settings.TryGetValue(CulturePropertyName, out result) ? result : DefaultCulture;
      }
      set
      {
        if (!DesignModeHelper.IsInDesignMode && value != null)
        {
          var settings = IsolatedStorageSettings.ApplicationSettings;

          settings[CulturePropertyName] = value;
          settings.Save();
        }
      }
    }
    #endregion

    #region Queries
    /// <summary>
    /// The name to store user queries under
    /// </summary>
    private readonly static string UserQueriesPropertyName = "UserQueries";

    /// <summary>
    /// An empty set of user queries
    /// </summary>
    private readonly static IList<LiteUserQueryStorageModel> _emptyUserQueries = new List<LiteUserQueryStorageModel>();

    /// <summary>
    /// The user queries
    /// </summary>
    public IList<LiteUserQueryStorageModel> UserQueries
    {
      get
      {
        if (DesignModeHelper.IsInDesignMode)
        {
          return _emptyUserQueries;
        }

        var settings = IsolatedStorageSettings.ApplicationSettings;
        IList<LiteUserQueryStorageModel> result;
        if (settings.TryGetValue(UserQueriesPropertyName, out result))
        {
          result = new List<LiteUserQueryStorageModel>(result.Where(p => p.ProjectName != null && p.ProjectName == TransactionContext.ActiveContext.ProjectName));
        };

        return result;
      }
      set
      {
        if (!DesignModeHelper.IsInDesignMode && value != null)
        {
          var settings = IsolatedStorageSettings.ApplicationSettings;

          List<LiteUserQueryStorageModel> queries;
          if (!settings.TryGetValue(UserQueriesPropertyName, out queries))
          {
            queries = new List<LiteUserQueryStorageModel>();
          }

          foreach (var existing in queries.ToArray())
          {
            if (existing.ProjectName == TransactionContext.ActiveContext.ProjectName)
            {
              queries.Remove(existing);
            }
          }

          if (value != null)
          {
            queries.AddRange(value);
          }

          settings[UserQueriesPropertyName] = queries;
          settings.Save();
        }
      }
    }


    /// <summary>
    /// Removes all the user queries
    /// </summary>
    public void RemoveAllUserQueries()
    {
      var settings = IsolatedStorageSettings.ApplicationSettings;

      // Set up and save an empty list of queries
      var queries = new List<LiteUserQueryStorageModel>();
      settings[UserQueriesPropertyName] = queries;
      settings.Save();
    }
    #endregion

    #region Maps
    /// <summary>
    /// The name to store user maps
    /// </summary>
    private readonly static string UserMapsPropertyName = "UserMaps";

    /// <summary>
    /// An empty set of user maps
    /// </summary>
    private readonly static IList<LiteUserMapStorageModel> _emptyUserMaps = new List<LiteUserMapStorageModel>();

    /// <summary>
    /// The user maps
    /// </summary>
    public IList<LiteUserMapStorageModel> UserMaps
    {
      get
      {
        if (DesignModeHelper.IsInDesignMode)
        {
          return _emptyUserMaps;
        }

        var settings = IsolatedStorageSettings.ApplicationSettings;
        IList<LiteUserMapStorageModel> result;
        if (settings.TryGetValue(UserMapsPropertyName, out result))
        {
          result = new List<LiteUserMapStorageModel>(result.Where(p => p.ProjectName != null && p.ProjectName == TransactionContext.ActiveContext.ProjectName));
        };

        return result;
      }
      set
      {
        if (!DesignModeHelper.IsInDesignMode && value != null)
        {
          var settings = IsolatedStorageSettings.ApplicationSettings;

          List<LiteUserMapStorageModel> maps;
          if (!settings.TryGetValue(UserMapsPropertyName, out maps))
          {
            maps = new List<LiteUserMapStorageModel>();
          }

          foreach (var existing in maps.ToArray())
          {
            if (existing.ProjectName == TransactionContext.ActiveContext.ProjectName)
            {
              maps.Remove(existing);
            }
          }

          if (value != null)
          {
            maps.AddRange(value);
          }

          settings[UserMapsPropertyName] = maps;
          settings.Save();
        }
      }
    }

    /// <summary>
    /// Removes all the user maps
    /// </summary>
    public void RemoveAllUserMaps()
    {
      var settings = IsolatedStorageSettings.ApplicationSettings;

      // Set up and save an empty list of maps
      var maps = new List<LiteUserMapStorageModel>();
      settings[UserMapsPropertyName] = maps;
      settings.Save();
    }
    #endregion
  }
}

