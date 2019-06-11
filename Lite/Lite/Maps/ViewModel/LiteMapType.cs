namespace Lite
{
  #region Type of Map
  /// <summary>
  /// An enumerator, holding information on what type of map this is
  /// </summary>
  public enum LiteMapType
  {
    User = 0,
    Geographic = 1,
    Analysis = 2,
    Internal = 3,
  }

  /// <summary>
  /// The map type extensions
  /// </summary>
  public static class LiteMapTypeExtensions
  {
    public static string PathGeometryKey(this LiteMapType type)
    {
      switch (type)
      {
        case LiteMapType.User: return "MetroIcon.Content.User";
        case LiteMapType.Analysis: return "MetroIcon.Content.ChartBubble";
        case LiteMapType.Internal: return "MetroIcon.Content.InternalWorld";
        default: return "MetroIcon.Content.World";
      }
    }
  }
  #endregion
}
