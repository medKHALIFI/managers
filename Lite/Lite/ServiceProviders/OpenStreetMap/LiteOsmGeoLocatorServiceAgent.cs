using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows.Browser;

namespace Lite
{
  /// <summary>
  /// The service agent that talks to the OSM Service Provider to use its
  /// GeoLocator service. 
  /// </summary>
  public class LiteOsmGeoLocatorServiceAgent : LiteWebGeoLocatorServiceAgentBase
  {
    #region Constructors
    /// <summary>
    /// Constructs the OSM GeoLocator Service Agent
    /// </summary>
    public LiteOsmGeoLocatorServiceAgent()
      : base("osm", "OpenStreetMap")
    {
      GeoLocatorUri = new Uri(@"http://nominatim.openstreetmap.org/search.php");
    }
    #endregion

    #region Implementation
    /// <summary>
    /// Return the Query for the GeoLocate service call
    /// </summary>
    protected override string GeoLocateServiceQuery(string searchString, int maximumNumberOfResults)
    {
      return HttpUtility.UrlEncode(String.Format("q={0}&format=xml&limit={1}", searchString, maximumNumberOfResults));
    }

    /// <summary>
    /// Get the result for the GeoLocate service call.
    /// The document parameter is the result from the service call, which is assumed 
    /// to be an XML structure
    /// </summary>
    protected override IList<WebGeoLocatorResultAddress> GetGeoLocateServiceResult(XDocument document)
    {
      var result = new List<WebGeoLocatorResultAddress>();

      if (document != null)
      {
        var navi = document.CreateNavigator();
        var iter = navi.SelectDescendants("place", string.Empty, false);

        while (iter.MoveNext())
        {
          var name = iter.Current.GetAttribute("display_name", string.Empty);
          var bb = iter.Current.GetAttribute("boundingbox", string.Empty);

          if (!String.IsNullOrEmpty(bb) && !String.IsNullOrEmpty(name))
          {
            var coords = GetDoublesFromString(bb);

            if (coords.Count == 4)
            {
              var env = CreateWGS84Envelope(coords[2], coords[0], coords[3], coords[1]);

              if (env != null)
              {
                result.Add(new WebGeoLocatorResultAddress { Name = name, Bounds = env });
              }
            }
          }
        }
      }

      return result;
    }
    #endregion
  }
}
