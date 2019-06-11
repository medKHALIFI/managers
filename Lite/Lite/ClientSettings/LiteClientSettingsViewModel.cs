using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using SpatialEye.Framework.Authentication;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Export;
using SpatialEye.Framework.Maps;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Maps.Services;
using SpatialEye.Framework.Text;

namespace Lite
{
  /// <summary>
  /// The client settings view model that acts as a strongly typed wrapper for
  /// the settings that have been defined for the Lite application.
  /// </summary>
  public class LiteClientSettingsViewModel : ViewModelBase
  {
    #region Singleton
    /// <summary>
    /// The singleton holding the client settings for the active session
    /// </summary>
    public static LiteClientSettingsViewModel Instance { get; private set; }
    #endregion

    #region ClientSetting Ids
    /// <summary>
    /// The id of the setting that indicates whether automatic signing in is allowed
    /// </summary>
    private const string AutoSignInSettingId = "allowautosignin";

    /// <summary>
    /// The id for the setting that indicates whether the server-side or client-side
    /// geoLocator must be picked up.
    /// </summary>
    private const string GeolocatorSettingId = "geolocator";

    /// <summary>
    /// The value indicating that the GeoLocator should be run server-side
    /// </summary>
    private const string GeoLocatorServer = "Server";

    /// <summary>
    /// The id for the setting that holds the postfix for searching
    /// </summary>
    private const string GeolocatorPostfixSettingId = "geolocatorpostfix";

    /// <summary>
    /// The value indicating the SRID to use
    /// </summary>
    private const string SRIDSettingId = "SRID";

    /// <summary>
    /// The value indicating the overviewMode to use
    /// </summary>
    private const string OverviewModeSettingId = "overviewmode";

    /// <summary>
    /// The default value for the overview mode
    /// </summary>
    private const string OverviewModeAllLayers = "Show all layers";

    /// <summary>
    /// The id for the allow print setting
    /// </summary>
    private const string AllowPrintSettingId = "allowprint";

    /// <summary>
    /// The id for the allow custom maps setting
    /// </summary>
    private const string AllowCustomMapsSettingId = "allowcustommaps";

    /// <summary>
    /// Restrict by areas
    /// </summary>
    private const string RestrictByAreasSettingId = "restrictbyareas";

    /// <summary>
    /// Restrict areas
    /// </summary>
    private const string RestrictAreasSettingId = "restrictareas";

    /// <summary>
    /// Display restriction areas
    /// </summary>
    private const string DisplayRestrictionAreasId = "displayrestrictionareas";

    /// <summary>
    /// The id for the allow custom queries setting
    /// </summary>
    private const string AllowCustomQueriesSettingId = "allowcustomqueries";

    /// <summary>
    /// Use restriction areas for queries
    /// </summary>
    private const string QueriesUseRestrictionAreasId = "queriesuserestrictionareas";

    /// <summary>
    /// The id for the allow multi selection setting
    /// </summary>
    private const string AllowMultiSelectionSettingId = "allowmultiselection";

    /// <summary>
    /// The id for the allow Street View setting
    /// </summary>
    private const string AllowStreetViewSettingId = "allowstreetview";

    /// <summary>
    /// The id for the maximum number of records
    /// </summary>
    private const string ExportMaximumRecordsId = "maximumexportrecords";

    /// <summary>
    /// The id for the allowed formats to export
    /// </summary>
    private const string ExportAllowedFormatsId = "allowedexports";

    /// <summary>
    /// The id for allowing to set the CS for export
    /// </summary>
    private const string ExportAllowSetCSId = "allowexportsetcs";

    /// <summary>
    /// The id for allowing to set the Units for export
    /// </summary>
    private const string ExportAllowSetUnitsId = "allowexportsetunits";

    /// <summary>
    /// The id for allowing the user to create or modify geonotes
    /// </summary>
    private const string AllowGeoNoteEditsId = "allowgeonoteedits";

    /// <summary>
    /// The id for the geonotes filter
    /// </summary>
    private const string AllowedGeoNotesId = "allowedgeonotes";

    /// <summary>
    /// The id for the copyright text for the print environment
    /// </summary>
    private const string PrintCopyrightTextId = "printcopyrighttext";

    /// <summary>
    /// The id for the maximum number of records in the result list
    /// </summary>
    private const string ResultListMaxRecordsID = "resultlistmaxrecords";
    #endregion

    #region Static Export Helpers
    /// <summary>
    /// The absolute maximum number of records to export; used to top
    /// the maximum number as set via the Client Settings.
    /// </summary>
    private static int ExportAbsoluteMaximumRecords = 100000;

    /// <summary>
    /// The absolute maximum number of records for display in the result list
    /// </summary>
    private static int ResultListAbsoluteMaximumRecords = 100000;

    /// <summary>
    /// The export type mapping
    /// </summary>
    private static Dictionary<string, ExportType> _exportTypeMapping;

    /// <summary>
    /// The export type mapping
    /// </summary>
    private static Dictionary<string, ExportType> ExportTypeMapping
    {
      get
      {
        if (_exportTypeMapping == null)
        {
          var result = new Dictionary<string, ExportType>
          {
            { "shp", ExportType.ArcShape },
            { "dwg", ExportType.AutoCadDwg },
            { "csv", ExportType.Csv },
            { "xls", ExportType.Excel2003 },
            { "xlsx", ExportType.Excel2007 },
            { "html", ExportType.Html },
            { "kml", ExportType.Kml },
            { "dgn", ExportType.MicrostationDgn },
            { "geojson", ExportType.GeoJson },
            { "gml", ExportType.Gml },
            { "mif", ExportType.MapInfoMifMid},
            { "tab", ExportType.MapInfoTab},
            { "geopackage", ExportType.GeoPackage }
          };

          _exportTypeMapping = result;
        }

        return _exportTypeMapping;
      }
    }
    #endregion

    #region Fields
    /// <summary>
    /// The allowed export types
    /// </summary>
    private IList<ExportType> _allowedExportTypes;

    /// <summary>
    /// The allowed geonotes
    /// </summary>
    private IList<String> _allowedGeoNotes;

    /// <summary>
    /// The map layer areas
    /// </summary>
    private ObservableCollection<MapLayerArea> _availableMapLayerAreas;

    /// <summary>
    /// The map layer areas restriction
    /// </summary>
    private ObservableCollection<MapLayerArea> _restrictionMapLayerAreas;

    /// <summary>
    /// The print copyright text
    /// </summary>
    private string _printCopyrightText;
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteClientSettingsViewModel(Messenger messenger = null)
      : base(messenger)
    {
      Instance = this;
    }
    #endregion

    #region Authentication
    /// <summary>
    /// Called when the client settings have changed
    /// </summary>
    internal async Task SetupFor(AuthenticationContext context, bool isAuthenticated)
    {
      _allowedExportTypes = null;
      _allowedGeoNotes = null;

      // Set up the Roles
      Roles = (context != null) ? context.Roles : null;

      if (isAuthenticated)
      {
        // Await for all map layers definitions to be returned
        var mapService = GetService<IMapService>();

        // Get named areas, which can be used to restrict areas to draw/select for users
        var areas = await mapService.GetMapLayerAreasAsync();

        // The available map layer areas
        AvailableMapLayerAreas = new ObservableCollection<MapLayerArea>(areas);
      }
      else
      {
        AvailableMapLayerAreas = new ObservableCollection<MapLayerArea>();
      }
    }

    /// <summary>
    /// The roles as these are picked up whenever the authentication has changed
    /// </summary>
    private AuthorizationRoleCollection Roles { get; set; }
    #endregion

    #region Settings Helpers
    /// <summary>
    /// Gets the specified client setting using the resolution as specified via the option.
    /// This resolution kicks in, in case there are more groups the user is part of and allows
    /// to indicate whether the highest or lowest value needs to be picked
    /// </summary>
    private object GetValue(string settingId, AuthorizationRoleClientSettingValueSelectionOption option, object defaultValue)
    {
      object result = defaultValue;

      if (Roles != null)
      {
        result = Roles.GetClientSettingValue(settingId, option) ?? result;
      }

      return (defaultValue is Int32) ? Int32.Parse(result.ToString()) : result;
    }

    /// <summary>
    /// Gets the specified client setting using the resolution as specified via the option.
    /// This resolution kicks in, in case there are more groups the user is part of and allows
    /// to indicate whether the highest or lowest value needs to be picked
    /// </summary>
    private TResult GetTypedValue<TResult>(string settingId, AuthorizationRoleClientSettingValueSelectionOption option, TResult defaultValue)
    {
      try
      {
        var val = GetValue(settingId, option, defaultValue);
        return (TResult)val;
      }
      catch { }

      return defaultValue;
    }
    #endregion

    #region Map Layer Areas

    /// <summary>
    /// Returns the restriction areas for the specified available areas
    /// </summary>
    private IList<MapLayerArea> RestrictionMapLayerAreasFor(IList<MapLayerArea> availableAreas)
    {
      var result = new List<MapLayerArea>();

      if (RestrictByMapLayerAreas && availableAreas != null && availableAreas.Count > 0)
      {
        // We have set up to restrict by areas
        var ids = RestrictionMapLayerAreaIds;

        if (ids != null)
        {
          foreach (var id in ids)
          {
            var mapLayerArea = availableAreas.FirstOrDefault(p => p.Id == id);
            if (mapLayerArea != null)
            {
              result.Add(mapLayerArea);
            }
          }
        }

        if (result.Count == 0)
        {
          var defaultArea = availableAreas.FirstOrDefault(l => l.Name.ToLower() == "home") ?? availableAreas[0];
          result.Add(defaultArea);
        }
      }

      return result;
    }

    /// <summary>
    /// The available map layer areas for the main service provider
    /// </summary>
    public ObservableCollection<MapLayerArea> AvailableMapLayerAreas
    {
      get { return _availableMapLayerAreas; }
      set
      {
        if (_availableMapLayerAreas != value)
        {
          _availableMapLayerAreas = value;
          if (_availableMapLayerAreas != null)
          {
            // Set up the restriction areas
            var restrictAreas = RestrictionMapLayerAreasFor(AvailableMapLayerAreas);
            RestrictionMapLayerAreas = new ObservableCollection<MapLayerArea>(restrictAreas);
          }
          else
          {
            RestrictionMapLayerAreas = new ObservableCollection<MapLayerArea>();
          }
        }
      }
    }

    /// <summary>
    /// The available map layer areas to be used for restricting layers with
    /// </summary>
    public ObservableCollection<MapLayerArea> RestrictionMapLayerAreas
    {
      get { return _restrictionMapLayerAreas; }
      set
      {
        if (_restrictionMapLayerAreas != value)
        {
          _restrictionMapLayerAreas = value;

          if (_restrictionMapLayerAreas != null)
          {
            // Calculate one multipolygon from the MapLayerAreas
            MultiPolygon restrictionGeometry = null;

            foreach (var area in _restrictionMapLayerAreas)
            {
              var geometry = area.Geometry;
              if (geometry != null)
              {
                if (restrictionGeometry == null)
                {
                  restrictionGeometry = new MultiPolygon(geometry.World, geometry.CoordinateSystem, geometry);
                }
                else
                {
                  foreach (var polygon in geometry)
                  {
                    restrictionGeometry.Add(polygon);
                  }
                }
              }
            }

            // Set the geometry
            RestrictionGeometry = restrictionGeometry;
          }
          else
          {
            RestrictionGeometry = null;
          }
        }
      }
    }

    /// <summary>
    /// The restriction geometry 
    /// </summary>
    public MultiPolygon RestrictionGeometry
    {
      get;
      set;
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Returns a flag indicating whether the specified context is allowed to 
    /// automatically sign in. 
    /// </summary>
    public bool AllowAutoSignIn(AuthenticationContext context)
    {
      var allow = false;
      if (context.IsAuthenticated && context.Roles != null)
      {
        var allowValue = context.Roles.GetClientSettingValue(AutoSignInSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue);

        allow = allowValue is Boolean && (Boolean)allowValue;
      }

      return allow;
    }

    /// <summary>
    /// Returns a flag indicating whether the client's OSM GeoLocator must be used
    /// </summary>
    public bool UseServerGeoLocator
    {
      get { return GetTypedValue(GeolocatorSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, GeoLocatorServer) == GeoLocatorServer; }
    }

    /// <summary>
    /// Returns a string holding the postfix for searching
    /// </summary>
    public string GeoLocatorSearchPostfix
    {
      get { return GetTypedValue(GeolocatorPostfixSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, ""); }
    }

    /// <summary>
    /// Returns the SRID to be used for display purposes on the map
    /// </summary>
    public int SRID
    {
      get { return GetTypedValue(SRIDSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, 4326); }
    }

    /// <summary>
    /// Indicates whether the overviewMode display All Layers
    /// </summary>
    public bool OverviewModeShowAllLayers
    {
      get { return GetTypedValue(OverviewModeSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, OverviewModeAllLayers) == OverviewModeAllLayers; }
    }

    /// <summary>
    /// Are we allowed to print
    /// </summary>
    public bool AllowPrint
    {
      get { return GetTypedValue(AllowPrintSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Are we allowed to add custom maps
    /// </summary>
    public bool AllowCustomMaps
    {
      get { return GetTypedValue(AllowCustomMapsSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Do we restrict by Map Layer Areas
    /// </summary>
    public bool RestrictByMapLayerAreas
    {
      get { return GetTypedValue(RestrictByAreasSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// What are the map layer area ids
    /// </summary>
    public IList<Guid> RestrictionMapLayerAreaIds
    {
      get
      {
        var result = new List<Guid>();

        var stringResult = GetTypedValue(RestrictAreasSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, "");

        if (!string.IsNullOrEmpty(stringResult))
        {
          var elements = stringResult.Split('|');
          if (elements != null)
          {
            foreach (var el in elements)
            {
              Guid guid;

              if (Guid.TryParse(el, out guid))
              {
                result.Add(guid);
              }
            }
          }
        }

        return result;
      }
    }

    /// <summary>
    /// Indicates whether the restriction areas should be displayed on the Map
    /// </summary>
    public bool DisplayRestrictionAreas
    {
      get { return GetTypedValue(DisplayRestrictionAreasId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Are we allowed to add custom queries
    /// </summary>
    public bool AllowCustomQueries
    {
      get { return GetTypedValue(AllowCustomQueriesSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Do we want to restrict the queries by the restriction areas
    /// </summary>
    public bool QueriesUseRestrictionAreas
    {
      get { return GetTypedValue(QueriesUseRestrictionAreasId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Are we allowed to select multiple items in the map
    /// </summary>
    public bool AllowMultiSelection
    {
      get { return GetTypedValue(AllowMultiSelectionSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Are we allowed to start the Street View interaction mode
    /// </summary>
    public bool AllowStreetView
    {
      get { return GetTypedValue(AllowStreetViewSettingId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Returns the allowed formats for exporting
    /// </summary>
    public IList<ExportType> ExportAllowedTypes
    {
      get
      {
        if (_allowedExportTypes == null)
        {
          var result = new List<ExportType>();
          var exportFormats = GetTypedValue(ExportAllowedFormatsId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, "xls, xlsx");

          if (!string.IsNullOrEmpty(exportFormats))
          {
            var allFormats = exportFormats.Split(',');
            foreach (var format in allFormats)
            {
              if (!string.IsNullOrEmpty(format))
              {
                var key = format.Trim();
                ExportType toAdd;
                if (ExportTypeMapping.TryGetValue(key, out toAdd))
                {
                  if (!result.Contains(toAdd))
                  {
                    result.Add(toAdd);
                  }
                }
              }
            }
          }

          _allowedExportTypes = result;
        }

        return _allowedExportTypes;
      }
    }

    /// <summary>
    /// Returns the maximum number of records we are allowed to export
    /// </summary>
    public int ExportMaximumRecords
    {
      get
      {
        var maximumRecords = GetTypedValue(ExportMaximumRecordsId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, 1000);
        return Math.Max(1, Math.Min(ExportAbsoluteMaximumRecords, maximumRecords));
      }
    }

    /// <summary>
    /// Are we allowed to set the CS for exports
    /// </summary>
    public bool ExportAllowSetCS
    {
      get { return GetTypedValue(ExportAllowSetCSId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Are we allowed to set the Units for exports
    /// </summary>
    public bool ExportAllowSetUnits
    {
      get { return GetTypedValue(ExportAllowSetUnitsId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Are we allowed to create or modify geonotes
    /// </summary>
    public bool AllowGeoNoteEdits
    {
      get { return GetTypedValue(AllowGeoNoteEditsId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, true); }
    }

    /// <summary>
    /// Returns a list of genotes allowed
    /// </summary>
    public IList<String> AllowedGeoNotes
    {
      get
      {
        if (_allowedGeoNotes == null)
        {
          var result = new List<String>();
          var allowedGeoNotes = GetTypedValue(AllowedGeoNotesId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, "");

          if (!string.IsNullOrEmpty(allowedGeoNotes))
          {
            var allGeoNotes = allowedGeoNotes.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var geoNote in allGeoNotes)
            {
              var key = geoNote.Trim().ToLowerInvariant();
              if (!string.IsNullOrEmpty(key) && !result.Contains(key))
              {
                result.Add(key);
              }
            }
          }

          _allowedGeoNotes = result;
        }

        return _allowedGeoNotes;
      }
    }

    /// <summary>
    /// Is the geonote with the given name allowed
    /// </summary>
    /// <param name="externalName">external name of the geonote</param>
    /// <returns>true if allowed otherwise false</returns>
    public Boolean IsGeoNoteAllowed(String externalName)
    {
      bool result = AllowedGeoNotes.Count == 0;

      if (!result)
      {
        if (!String.IsNullOrEmpty(externalName))
        {
          var lowerName = externalName.ToLowerInvariant();
          result = AllowedGeoNotes.Any((a) => lowerName.WildCardMatch(a));
        }
      }

      return result;
    }

    /// <summary>
    /// The copyright text to be placed on the print
    /// </summary>
    public string PrintCopyrightText
    {
      get
      {
        if (string.IsNullOrEmpty(_printCopyrightText))
        {
          _printCopyrightText = GetTypedValue(PrintCopyrightTextId, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, "");
        }
        return _printCopyrightText;
      }
    }

    /// <summary>
    /// The copyright text to be placed on the print
    /// </summary>
    public int ResultListMaximumNumberOfRecords
    {
      get
      {
        var maximumRecords = GetTypedValue(ResultListMaxRecordsID, AuthorizationRoleClientSettingValueSelectionOption.HighestValue, 1000);
        return Math.Max(1, Math.Min(ResultListAbsoluteMaximumRecords, maximumRecords));
      }
    }
    #endregion
  }
}
