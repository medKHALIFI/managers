using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using System.Linq;
using System.Windows.Media;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Maps;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry.CoordinateSystems;
using SpatialEye.Framework.Geometry.Services;
using SpatialEye.Framework.Export;
using SpatialEye.Framework.Client.Styles;
using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// A view model holding the logic for a single Map. It is an extension
  /// of the framework's MapViewModel, to allow for Lite specific additions
  /// to be incorporated
  /// </summary>
  public class LiteMapViewModel : MapViewModel
  {
    #region Allowed export types for Map
    /// <summary>
    /// Are we allowed to export to the specified target
    /// </summary>
    public static bool IsAllowedForExport(ExportType exportType)
    {
      // Check management client settings first
      if (!LiteClientSettingsViewModel.Instance.ExportAllowedTypes.Contains(exportType))
      {
        // Not allowed by management client
        return false;
      }

      switch (exportType)
      {
        case ExportType.ArcShape:
        case ExportType.MicrostationDgn:
        case ExportType.Kml:
          return true;

        default: return false;
      }
    }

    /// <summary>
    /// Are we allowed to export the specified group type
    /// </summary>
    public static bool IsAllowedForExport(ServiceProviderGroupType groupType)
    {
      return groupType == ServiceProviderGroupType.Business || groupType == ServiceProviderGroupType.Analysis;
    }
    #endregion

    #region Request remove delegate
    /// <summary>
    /// Request to remove the map
    /// </summary>
    internal delegate void RequestRemoveMapDelegate(LiteMapViewModel map);
    #endregion

    #region Static Property Names
    /// <summary>
    /// The center coordinate in a human readable format (and coordinate system)
    /// </summary>
    public const string CenterCoordinateDescriptionPropertyName = "CenterCoordinateDescription";

    /// <summary>
    /// The view visibility of the center coordinate
    /// </summary>
    public const string CenterCoordinateViewVisibilityPropertyName = "CenterCoordinateViewVisibility";

    /// <summary>
    /// The spatial reference IDs available for selection
    /// </summary>
    public const string EpsgCoordinateSystemsPropertyName = "EpsgCoordinateSystems";

    /// <summary>
    /// The selected SRID for displaying the center with
    /// </summary>
    public const string SelectedEpsgCoordinateSystemPropertyName = "SelectedEpsgCoordinateSystem";

    /// <summary>
    /// Is the map control expanded
    /// </summary>
    public const string IsExpandedPropertyName = "IsExpanded";

    /// <summary>
    /// Is the map element control visible (expanded)
    /// </summary>
    public const string IsExpandedVisibilityPropertyName = "IsExpandedVisibility";

    /// <summary>
    /// Is the map element control expand option visisble
    /// </summary>
    public const string ExpandOptionVisibilityPropertyName = "ExpandOptionVisibility";

    /// <summary>
    /// The indentation of the heading
    /// </summary>
    public const string HeadingIndentPropertyName = "HeadingIndent";

    /// <summary>
    /// Is there an export enabled
    /// </summary>
    public const string ExportsEnabledPropertyName = "ExportsEnabled";

    /// <summary>
    /// The dynamic interaction mode description
    /// </summary>
    public const string DynamicInteractionModeDescriptionPropertyName = "DynamicInteractionModeDescription";

    /// <summary>
    /// The dynamic interaction mode view visibility
    /// </summary>
    public const string DynamicInteractionModeViewVisibilityPropertyName = "DynamicInteractionModeViewVisibility";
    #endregion

    #region Static Properties

    /// <summary>
    /// The number format to use to display the centre coordinate
    /// </summary>
    private readonly NumberFormatInfo CoordinateSystemDisplayFormat = new NumberFormatInfo { NumberDecimalDigits = 3, NumberGroupSeparator = "" };

    /// <summary>
    /// The DMS coordinate system SRID
    /// </summary>
    private static int DMSCoordinateSystemSrid = 4326;

    /// <summary>
    /// The default named srid
    /// </summary>
    internal static EpsgCoordinateSystemReference DefaultEpsgCoordinateSystem = new EpsgCoordinateSystemReference { SRId = 4326, Name = "WGS 84" };

    /// <summary>
    /// Indicates whether tracking of centre should be done immediately in case 
    /// the transformation to a user CS can be done client side, else display of
    /// the new centre will be done when animation/panning is finished.
    /// </summary>
    private static bool TrackCentreContinuouslyIfClientSideTransformPossible = false;

    /// <summary>
    /// The selection color resource keys
    /// </summary>
    private const string _SelectionAreaColorResourceKey = "Lite.Color.Selection.Area";
    private const string _SelectionLineColorResourceKey = "Lite.Color.Selection.Line";
    #endregion

    #region Static Comparer
    /// <summary>
    ///  A function that can be used as comparer between maps for sorting purposes
    /// </summary>
    public static Func<LiteMapViewModel, LiteMapViewModel, int> LiteMapViewModelComparer = new Func<LiteMapViewModel, LiteMapViewModel, int>(
      (a, b) =>
      {
        int valueA = (int)a.MapType;
        int valueB = (int)b.MapType;
        int compare = valueA.CompareTo(valueB);

        if (compare == 0)
        {
          compare = a.ExternalName.CompareTo(b.ExternalName);
        }

        return compare;
      });
    #endregion

    #region Fields
    /// <summary>
    /// Can we transform the coordinate systems client side?
    /// </summary>
    private bool? _canTransformClientSide;

    /// <summary>
    /// The named srids
    /// </summary>
    private EpsgCoordinateSystemReferenceCollection _epsgCoordinateSystems;

    /// <summary>
    /// The selected srid to display the center with
    /// </summary>
    private EpsgCoordinateSystemReference _selectedEpsgCoordinateSystem = DefaultEpsgCoordinateSystem;

    /// <summary>
    /// The description of the center coordinate of the map
    /// </summary>
    private string _centerCoordinateDescription = string.Empty;

    /// <summary>
    /// A flag indicating whether the map is expanded
    /// </summary>
    private bool _isExpanded;

    /// <summary>
    /// Indentation of the view header
    /// </summary>
    private int _headingIndent = 8;

    /// <summary>
    /// The dynamic interaction mode description
    /// </summary>
    private string _dynamicInteractionModeDescriptionResource;
    #endregion

    #region Constructors
    /// <summary>
    /// Constructs a LiteMapViewModel for a specified definition, world and default envelope.
    /// </summary>
    /// <param name="messenger">The messenger to use for exchanging messages</param>
    /// <param name="definition">The definition to use for determining layers</param>
    /// <param name="epsgCSs">The spatialReference IDs available for display of Coords</param>
    /// <param name="world">The world to use (should match with definition's universe)</param>
    /// <param name="envelope">The default envelope to use</param>
    public LiteMapViewModel(Messenger messenger, MapDefinition definition, EpsgCoordinateSystemReferenceCollection epsgCSs, World world = null, Envelope envelope = null, Feature owner = null)
      : this(messenger, definition, false, null, epsgCSs, world, envelope, owner)
    { }

    /// <summary>
    /// Constructs a LiteMapViewModel for a specified definition, world and default envelope.
    /// </summary>
    /// <param name="messenger">The messenger to use for exchanging messages</param>
    /// <param name="definition">The definition to use for determining layers</param>
    /// <param name="epsgCSs">The spatialReference IDs available for display of Coords</param>
    /// <param name="world">The world to use (should match with definition's universe)</param>
    /// <param name="envelope">The default envelope to use</param>
    public LiteMapViewModel(Messenger messenger, MapDefinition definition, bool isUserMap, EpsgCoordinateSystemReferenceCollection epsgCSs, World world = null, Envelope envelope = null, Feature owner = null)
      : this(messenger, definition, isUserMap, null, epsgCSs, world, envelope, owner)
    { }

    /// <summary>
    /// Constructs a LiteMapViewModel for a specified definition, interactionHandler, world and default
    /// envelope. 
    /// </summary>
    /// <param name="messenger">The messenger to use for exchanging messages</param>
    /// <param name="definition">The definition to use for determining layers</param>
    /// <param name="interactionHandler">The interaction handler that contains the available interaction modes</param>
    /// <param name="epsgCSs">The spatialReference IDs available for display of Coords</param>
    /// <param name="world">The world to use (should match with definition's universe)</param>
    /// <param name="envelope">The default envelope to use</param>
    public LiteMapViewModel(Messenger messenger, MapDefinition definition, MapInteractionHandler interactionHandler, EpsgCoordinateSystemReferenceCollection epsgCSs, World world = null, Envelope envelope = null, Feature owner = null)
      : this(messenger, definition, false, interactionHandler, epsgCSs, world, envelope, owner)
    { }

    /// <summary>
    /// Constructs a LiteMapViewModel for a specified definition, interactionHandler, world and default
    /// envelope. 
    /// </summary>
    /// <param name="messenger">The messenger to use for exchanging messages</param>
    /// <param name="definition">The definition to use for determining layers</param>
    /// <param name="interactionHandler">The interaction handler that contains the available interaction modes</param>
    /// <param name="epsgCSs">The spatialReference IDs available for display of Coords</param>
    /// <param name="world">The world to use (should match with definition's universe)</param>
    /// <param name="envelope">The default envelope to use</param>
    public LiteMapViewModel(Messenger messenger, MapDefinition definition, bool isUserMap, MapInteractionHandler interactionHandler, EpsgCoordinateSystemReferenceCollection epsgCSs, World world = null, Envelope envelope = null, Feature owner = null)
      : base(messenger, definition, interactionHandler, world, envelope, owner)
    {
      EpsgCoordinateSystems = epsgCSs;

      SetMapType(definition, isUserMap);

      SetupCommands();

      SetupExport();

      // Setup styles
      SetupSelectionStyles();
      SetupHighlightStyles();
    }


    /// <summary>
    /// Sets up the default coloring for selection on the map
    /// </summary>
    private void SetupSelectionStyles()
    {
      // Set the selection color properties from the resources
      SelectionLineColor = (Color)Application.Current.Resources[_SelectionLineColorResourceKey];
      SelectionAreaColor = (Color)Application.Current.Resources[_SelectionAreaColorResourceKey];

      // Alternative way to control the whole selection style is to create a custom  style
      // See sample below.

      //// The selection colors
      //var selectionLineColor = Color.FromArgb(150, 20, 122, 251);
      //var selectionAreaColor = Color.FromArgb(75, 20, 122, 251);

      //// Selection Point Style
      //Properties.SelectionPointStyle = new SimplePointStyle()
      //  {
      //    StyleType = SimplePointStyleType.Ellipse,
      //    Fill = new SolidColorBrush(Color.FromArgb(102, 0, 0, 255)),
      //    Stroke = new SolidColorBrush(selectionLineColor),
      //    Width = 25,
      //    Height = 25,
      //    StrokeThickness = 5,
      //    SelectedEffect = SimplePointStyleEffect.Glow,
      //    SelectedEffectColor = selectionLineColor
      //  };

      //// Selection Line Style
      //Properties.SelectionLineStyle = new SimpleLineStyle()
      //  {
      //    Stroke = new SolidColorBrush(selectionLineColor),
      //    Width = 5
      //  };

      //// Selection Area Style
      //Properties.SelectionAreaStyle = new SimpleAreaStyle()
      //  {
      //    Fill = new SolidColorBrush(selectionAreaColor),
      //    Stroke = new SolidColorBrush(selectionLineColor),
      //    StrokeThickness = 5
      //  };
    }

    /// <summary>
    /// Sets up the default coloring for highlight on the map
    /// </summary>
    private void SetupHighlightStyles()
    {
      // The highlight colors
      var highlightLineColor = Color.FromArgb(255, 246, 185, 77);
      var highlightLineColorDim = Color.FromArgb(204, 246, 185, 77);
      var highlightAreaColor = Color.FromArgb(144, 246, 185, 77);

      // Highlight Point Style
      Properties.HighlightPointStyle = new SimplePointStyle()
      {
        StyleType = SimplePointStyleType.Ellipse,
        Fill = new SolidColorBrush(highlightAreaColor),
        Stroke = new SolidColorBrush(highlightLineColor),
        Width = 20,
        Height = 20,
        StrokeThickness = 3,
        SelectedEffect = SimplePointStyleEffect.GrowShrink
      };

      // Highlight Line Style
      Properties.HighlightLineStyle = new SimpleLineStyle()
      {
        Stroke = new SolidColorBrush(highlightLineColorDim),
        Width = 9
      };

      // Highlight Area Style
      Properties.HighlightAreaStyle = new SimpleAreaStyle()
      {
        Fill = new SolidColorBrush(highlightAreaColor),
        Stroke = new SolidColorBrush(highlightLineColorDim),
        StrokeThickness = 6
      };
    }
    #endregion

    #region Commands
    /// <summary>
    /// Gets the command to run the export
    /// </summary>
    public RelayCommand<object> RunExportCommand { get; private set; }

    /// <summary>
    /// The command that removes the map
    /// </summary>
    public RelayCommand RemoveCommand { get; private set; }

    /// <summary>
    /// Can the map be removed
    /// </summary>
    public bool CanRemove
    {
      get
      {
        var isInternal = (World != null && World.IsPartOfMultiWorld);
        return IsUserMap || isInternal;
      }
    }

    /// <summary>
    /// A visibility, indicating whether this item can be removed
    /// </summary>
    public Visibility RemoveVisibility
    {
      get { return CanRemove ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Setup the commands
    /// </summary>
    private void SetupCommands()
    {
      RemoveCommand = new RelayCommand(RemoveMap, () => CanRemove);

      // Create the command to run an export for the active map
      RunExportCommand = new RelayCommand<object>
       ((exportToRun) =>
       {
         var model = exportToRun as ExportViewModel;

         if (model != null)
         {
           // Do Analytics Tracking
           LiteAnalyticsTracker.TrackExport(model, LiteAnalyticsTracker.Source.Map);

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
    /// Check the commands' enabled state
    /// </summary>
    private void CheckCommands()
    {
      RemoveCommand.RaiseCanExecuteChanged();
      RunExportCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Request to be removed
    /// </summary>
    private void RemoveMap()
    {
      var handler = this.RequestRemoveMap;
      if (handler != null)
      {
        handler(this);
      }
    }
    #endregion

    #region Culture change handling

    protected override void OnCurrentCultureChanged(CultureInfo currentCultureInfo)
    {
      base.OnCurrentCultureChanged(currentCultureInfo);

      // Description changed
      RaisePropertyChanged(DynamicInteractionModeDescriptionPropertyName);
    }

    #endregion

    #region Export Setup
    private void SetupExport()
    {
      this.Exports = new MapExportsViewModel(Messenger)
      {
        AutoGetAvailableDefinitions = true,
        AllowCoordinateSystemSettings = LiteClientSettingsViewModel.Instance.ExportAllowSetCS,
        AllowUnitSettings = LiteClientSettingsViewModel.Instance.ExportAllowSetUnits,
        DefinitionsFilter = definition => LiteClientSettingsViewModel.Instance.ExportAllowedTypes.Contains(definition.ExportType),
        ExportCoordinateSystemsFilter = cs => cs.SRId != 3785,
        ExportMapEnvelopeProvider = () => Envelope,
        ExportMapViewScaleProvider = () => ViewScale,
        ExportClipPolygonProvider = () => LiteClientSettingsViewModel.Instance.RestrictionGeometry,
        ExportMapViewMaxRecords = LiteClientSettingsViewModel.Instance.ExportMaximumRecords,
        ExportMapViewDoClip = false
      };

      // Refilter
      RefilterExport();
    }

    /// <summary>
    /// Set up the export
    /// </summary>
    private void RefilterExport()
    {
      // Get the main provider - we are only allowing export for maps in that provider
      var mainProvider = ServiceProviderManager.Instance.MainServiceProvider;

      // Initialize map layers to be exported
      var mapLayers = new List<MapLayerDefinition>();

      foreach (var mapLayer in this.Layers)
      {
        var definition = mapLayer.LayerDefinition;

        if (mapLayer.IsOn && definition != null && definition.ServiceProvider == mainProvider && IsAllowedForExport(definition.ServiceProviderGroup.GroupType))
        {
          mapLayers.Add(mapLayer.LayerDefinition);
        }
      }

      // Set up the exports for the active layers
      this.Exports.SetupForGenerator(_d =>
        {
          var allowedExports = new List<ExportViewModel>();
          if (mapLayers.Count > 0 && _d.CanHandleMultipleCollections && IsAllowedForExport(_d.ExportType))
          {
            allowedExports.Add(new ExportViewModel(_d, mapLayers));
          }

          return allowedExports;
        });

      // Notify the outside world
      RaisePropertyChanged(ExportsEnabledPropertyName);
      CheckCommands();
    }
    #endregion

    #region Properties
    /// <summary>
    /// Is this a user map
    /// </summary>
    public Boolean IsUserMap
    {
      get { return MapType == LiteMapType.User; }
    }

    /// <summary>
    /// Holds the Exports for the map's Layers
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
      get { return Exports.Exports.Count > 0 && !Exports.IsBusy; }
    }
    #endregion

    #region Interaction Mode

    protected override void OnCurrentDynamicInteractionModeChanged(MapInteractionMode mode)
    {

      DynamicInteractionModeDescription = (mode != null) ? mode.DescriptionResourceKey : null;
    }

    public string DynamicInteractionModeDescription
    {
      get { return (String.IsNullOrEmpty(_dynamicInteractionModeDescriptionResource)) ? null : ApplicationResources.ResourceManager.GetString(_dynamicInteractionModeDescriptionResource); }
      set
      {

        if (value != _dynamicInteractionModeDescriptionResource)
        {
          _dynamicInteractionModeDescriptionResource = value;
          RaisePropertyChanged(DynamicInteractionModeDescriptionPropertyName);
          RaisePropertyChanged(DynamicInteractionModeViewVisibilityPropertyName);
        }
      }
    }

    public Visibility DynamicInteractionModeViewVisibility
    {
      get { return (String.IsNullOrEmpty(DynamicInteractionModeDescription)) ? Visibility.Collapsed : Visibility.Visible; }
    }

    #endregion

    #region Type
    /// <summary>
    /// Sets the visibility flags of the images that can be used to represent
    /// this map
    /// </summary>
    private void SetMapType(MapDefinition definition, bool isUserMap)
    {
      if (isUserMap)
      {
        this.MapType = LiteMapType.User;
      }
      else
      {
        this.MapType = LiteMapType.Geographic;

        if (definition != null)
        {
          var universe = definition.Universe;
          if (universe != null)
          {
            if (universe.IsMultiWorld)
            {
              this.MapType = LiteMapType.Internal;
            }
            else if (definition.ServiceProviderGroup != null)
            {
              if (definition.ServiceProviderGroup.GroupType == ServiceProviderGroupType.Analysis)
              {
                this.MapType = LiteMapType.Analysis;
              }
              else
              {
                this.MapType = LiteMapType.Geographic;
              }
            }
            else
            {
              // No service provider group
              this.MapType = LiteMapType.User;
            }
          }
        }
      }

      // Set path geometry as well
      this.PathGeometry = Application.Current.Resources[MapType.PathGeometryKey()] as string;

      RaisePropertyChanged(ExpandOptionVisibilityPropertyName);
    }

    /// <summary>
    /// Holds the type of Lite Map
    /// </summary>
    public LiteMapType MapType
    {
      get;
      private set;
    }

    /// <summary>
    /// The named Spatial Reference IDs that are available; which are key-value
    /// pairs of a SRID (integer) and a descriptive Name (string)
    /// </summary>
    public EpsgCoordinateSystemReferenceCollection EpsgCoordinateSystems
    {
      get { return _epsgCoordinateSystems; }
      set
      {
        if (_epsgCoordinateSystems != value)
        {
          _epsgCoordinateSystems = value;
          RaisePropertyChanged(EpsgCoordinateSystemsPropertyName);

          // Try to pick the prefered CS from the SRID as set for this user
          var useCS = _epsgCoordinateSystems.FirstOrDefault(cs => cs.SRId == LiteClientSettingsViewModel.Instance.SRID);

          if (useCS == null)
          {
            // Backstop to any default CS from the Project's Coordinate Systems.
            // This is the Main Map's Default CS IFF it is an epgs cs
            // If all else fails; resort to our local backstop (which is wgs84)
            useCS = _epsgCoordinateSystems.Default ?? DefaultEpsgCoordinateSystem;
          }

          this.SelectedEpsgCoordinateSystem = useCS;
        }
      }
    }

    /// <summary>
    /// The coordinate system to transfer to
    /// </summary>
    public EpsgCoordinateSystemReference SelectedEpsgCoordinateSystem
    {
      get { return _selectedEpsgCoordinateSystem; }
      set
      {
        if (_selectedEpsgCoordinateSystem != value && value != null)
        {
          _selectedEpsgCoordinateSystem = value;

          RaisePropertyChanged(SelectedEpsgCoordinateSystemPropertyName);
        }
      }
    }

    /// <summary>
    /// Holds the path geometry
    /// </summary>
    public string PathGeometry
    {
      get;
      private set;
    }

    /// <summary>
    /// The event to raise when requesting to remove this map
    /// </summary>
    internal event RequestRemoveMapDelegate RequestRemoveMap;
    #endregion

    #region Layer Handling
    /// <summary>
    /// The property of one of the layers changed
    /// </summary>
    protected override void OnLayerPropertyChanged(MapLayerViewModel layer, System.ComponentModel.PropertyChangedEventArgs e)
    {
      base.OnLayerPropertyChanged(layer, e);
      RefilterExport();
    }
    #endregion

    #region Envelope and Storing envelope
    /// <summary>
    /// The envelope has changed, notifies the outside world and store the 
    /// extent in case we want to do this automatically
    /// </summary>
    protected override void OnEnvelopeChanged(Envelope envelope)
    {
      base.OnEnvelopeChanged(envelope);

      if (!IsAnimating && !IsPanning && Universe.HasGeographicCoordinateSystems)
      {
        StoreExtentAsStartup();
      }
    }
    #endregion

    #region Centre Coordinate (and Description)
    /// <summary>
    /// Can we transform client side
    /// </summary>
    private bool CanTransformClientSide
    {
      get
      {
        if (!_canTransformClientSide.HasValue)
        {
          var geometryService = GetService<IGeometryService>();
          _canTransformClientSide = geometryService.CanTransformClientSide(CoordinateSystem.EPSGCode, SelectedEpsgCoordinateSystem.SRId);
        }

        return _canTransformClientSide.Value;
      }
    }

    /// <summary>
    /// The centre has changed
    /// </summary>
    protected override async void OnCentreChanged(Coordinate centre)
    {
      // Notify others first
      base.OnCentreChanged(centre);

      // Only do this in case we are geographic
      if (IsGeographic)
      {
        // Only use the transformation API in case we are not animating
        if ((!IsAnimating && !IsPanning) || (TrackCentreContinuouslyIfClientSideTransformPossible && CanTransformClientSide))
        {
          var geometryService = GetService<IGeometryService>();
          var centreInUserCs = await geometryService.TransformAsync(centre, CoordinateSystem.EPSGCode, SelectedEpsgCoordinateSystem.SRId);

          if (centreInUserCs != null)
          {
            if (SelectedEpsgCoordinateSystem.SRId == DMSCoordinateSystemSrid)
            {
              // This is the CS that should display as DMS
              CenterCoordinateDescription = CoordinateSystem.CoordinateToDmsString(centreInUserCs);
            }
            else
            {
              CenterCoordinateDescription = String.Format("{0}, {1}", centreInUserCs.X.ToString("N", CoordinateSystemDisplayFormat), centreInUserCs.Y.ToString("N", CoordinateSystemDisplayFormat));
            }
          }
        }
      }
    }

    /// <summary>
    /// The center coordinate of the active map in a human readable format (and coordinate system)
    /// </summary>
    public string CenterCoordinateDescription
    {
      get { return _centerCoordinateDescription; }
      set
      {
        if (value != _centerCoordinateDescription)
        {
          _centerCoordinateDescription = value;
          RaisePropertyChanged(CenterCoordinateDescriptionPropertyName);
        }
      }
    }

    /// <summary>
    /// The center coordinate view visibility
    /// </summary>
    public Visibility CenterCoordinateViewVisibility
    {
      get { return IsGeographic ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion

    #region View Properties



    /// <summary>
    /// Indentation of the view header
    /// </summary>
    public int HeadingIndent
    {
      get { return _headingIndent; }
      set
      {
        if (_headingIndent != value)
        {
          _headingIndent = value;
          RaisePropertyChanged(HeadingIndentPropertyName);
        }
      }
    }

    /// <summary>
    /// Are we allowed to expand/collapse
    /// </summary>
    public Visibility ExpandOptionVisibility
    {
      get { return CanRemove ? Visibility.Visible : Visibility.Collapsed; }
    }

    /// <summary>
    /// Is the map expanded
    /// </summary>
    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        if (value != _isExpanded)
        {
          _isExpanded = value;
          RaisePropertyChanged(IsExpandedPropertyName);
          RaisePropertyChanged(IsExpandedVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Is the query view visible (expanded)
    /// </summary>
    public Visibility IsExpandedVisibility
    {
      get { return (IsExpanded) ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
