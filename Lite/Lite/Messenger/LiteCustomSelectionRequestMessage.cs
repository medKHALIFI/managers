using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// Request for carrying out selection in case selection on the Map
  /// (by mouse click) didn't find anything
  /// </summary>
  public class LiteCustomSelectionRequestMessage : MessageBase
  {
    #region Constructor
    /// <summary>
    /// Constructs the request to carry out selection for the specified map;
    /// only requested when selection by default interaction has not found
    /// anything
    /// </summary>
    /// <param name="sender">The originator of the request, which is the Map</param>
    /// <param name="args">The mouse event args of the click</param>
    public LiteCustomSelectionRequestMessage(MapViewModel sender, MapMouseEventArgs args)
      : base(sender)
    {
      this.Map = sender;
      this.Args = args;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The Map the selection was carried out on
    /// </summary>
    public MapViewModel Map { get; private set; }

    /// <summary>
    /// The MapMouseEvent args that the selection was performed with
    /// </summary>
    public MapMouseEventArgs Args { get; private set; }
    #endregion
  }
}
