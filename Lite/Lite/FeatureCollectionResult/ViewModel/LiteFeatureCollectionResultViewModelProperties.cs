using SpatialEye.Framework.ComponentModel;

namespace Lite
{
  /// <summary>
  /// The class holding the Properties for the Lite Collection Result viewmodel
  /// </summary>
  public class LiteFeatureCollectionResultViewModelProperties : BindableObject
  {
    #region Static
    /// <summary>
    /// The Export/Report Elements Mode, indicating which elements are to be exported:
    /// Either the full collection, the batch collection (current batch) or the selected feature.
    /// </summary>
    internal const string ExportReportElementsModePropertyName = "ExportReportElementsMode";

    /// <summary>
    /// The export/report track mode; will the main collection always be exported, or the focused
    /// collection (which can be a join feature collection in a tab)
    /// </summary>
    internal const string ExportReportTrackModePropertyName = "ExportReportTrackMode";

    /// <summary>
    /// Send a Display Feature Details request for every selected feature
    /// </summary>
    internal const string TrackSelectionInFeatureDetailsPropertyName = "TrackSelectionInFeatureDetails";

    /// <summary>
    /// Send a request for going to an envelope for every feature selected
    /// </summary>
    internal const string TrackSelectionInMapPropertyName = "TrackSelectionInMap";

    /// <summary>
    /// Send a request for highlighting the selection
    /// </summary>
    internal const string HighlightSelectionInMapPropertyName = "HighlightSelectionInMap";

    /// <summary>
    /// The available modes for indicating the elements to be exported/reported
    /// </summary>
    public enum ExportReportElementsModes
    {
      FullCollection = 0,
      BatchCollection = 1,
      SelectedFeature = 2
    }

    /// <summary>
    /// The modes which collection will be reported/exported, either the main collection
    /// or the collection that currently has focus.
    /// </summary>
    public enum ExportReportTrackModes
    {
      MainCollection = 0,
      FocusedCollection = 1
    }
    #endregion

    #region Fields
    /// <summary>
    /// The current export/report elements mode
    /// </summary>
    private ExportReportElementsModes _exportReportElementsMode;

    /// <summary>
    /// The current export/report track mode
    /// </summary>
    private ExportReportTrackModes _exportReportTrackMode;

    /// <summary>
    /// Holds a flag indicating whether we should send a display feature request
    /// for every feature that is selected.
    /// </summary>
    private bool _trackSelectionInFeatureDetails;

    /// <summary>
    /// Holds a flag indicating whether we should send a Go-To Envelope request
    /// for every feature selected.
    /// </summary>
    private bool _trackSelectionInMap;

    /// <summary>
    /// Holds a flag indicating whether we should send a Go-To Envelope request
    /// for every feature selected.
    /// </summary>
    private bool _highlightSelectionInMap;
    #endregion

    #region Constructor
    /// <summary>
    /// Internal constructor for the properties
    /// </summary>
    internal LiteFeatureCollectionResultViewModelProperties()
    {
      ExportReportElementsMode = ExportReportElementsModes.FullCollection;
      ExportReportTrackMode = ExportReportTrackModes.FocusedCollection;

      TrackSelectionInFeatureDetails = true;
      TrackSelectionInMap = false;
      HighlightSelectionInMap = true;
    }
    #endregion

    #region Internal
    /// <summary>
    /// Should all elements be exported
    /// </summary>
    internal bool ElementsUseFull
    {
      get { return ExportReportElementsMode == ExportReportElementsModes.FullCollection; }
    }

    /// <summary>
    /// Should the elements go via batch
    /// </summary>
    internal bool ElementsUseBatch
    {
      get { return ExportReportElementsMode == ExportReportElementsModes.BatchCollection; }
    }

    /// <summary>
    /// Should APIs operate on the selected feature
    /// </summary>
    internal bool ElementsUseSelectedFeature
    {
      get { return ExportReportElementsMode == ExportReportElementsModes.SelectedFeature; }
    }

    /// <summary>
    /// Do we want to track the focused collection (instead of the main collection)
    /// </summary>
    internal bool TrackUseFocused
    {
      get { return ExportReportTrackMode == ExportReportTrackModes.FocusedCollection; }
    }

    #endregion

    #region API
    /// <summary>
    /// The Export/Report Elements Mode, indicating which elements are to be exported:
    /// Either the full collection, the batch collection (current batch) or the selected feature.
    /// </summary>
    public ExportReportElementsModes ExportReportElementsMode
    {
      get { return _exportReportElementsMode; }
      set
      {
        if (_exportReportElementsMode != value)
        {
          _exportReportElementsMode = value;
          RaisePropertyChanged(ExportReportElementsModePropertyName);
        }
      }
    }

    /// <summary>
    /// The export/report track mode; will the main collection always be exported, or the focused
    /// collection (which can be a join feature collection in a tab)
    /// </summary>
    public ExportReportTrackModes ExportReportTrackMode
    {
      get { return _exportReportTrackMode; }
      set
      {
        if (_exportReportTrackMode != value)
        {
          _exportReportTrackMode = value;
          RaisePropertyChanged(ExportReportTrackModePropertyName);
        }
      }
    }

    /// <summary>
    /// Send a Display Feature Details request for every selected feature
    /// </summary>
    public bool TrackSelectionInFeatureDetails
    {
      get { return _trackSelectionInFeatureDetails; }
      set
      {
        if (_trackSelectionInFeatureDetails != value)
        {
          _trackSelectionInFeatureDetails = value;
          RaisePropertyChanged(TrackSelectionInFeatureDetailsPropertyName);
        }
      }
    }

    /// <summary>
    /// Send a request for going to an envelope for every feature selected
    /// </summary>
    public bool TrackSelectionInMap
    {
      get { return _trackSelectionInMap; }
      set
      {
        if (_trackSelectionInMap != value)
        {
          _trackSelectionInMap = value;
          RaisePropertyChanged(TrackSelectionInMapPropertyName);
        }
      }
    }

    /// <summary>
    /// Send a request for highlighting the active feature
    /// </summary>
    public bool HighlightSelectionInMap
    {
      get { return _highlightSelectionInMap; }
      set
      {
        if (_highlightSelectionInMap != value)
        {
          _highlightSelectionInMap = value;
          RaisePropertyChanged(HighlightSelectionInMapPropertyName);
        }
      }
    }
    #endregion
  }
}
