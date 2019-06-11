using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

using SpatialEye.Framework.GeoLocators;
using SpatialEye.Framework.GeoLocators.Services;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.ServiceProviders.XY;
using SpatialEye.Framework.Services;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// An abstract base class for handling geoLocator requests for a LiteWebServiceProviderBase 
  /// service provider. It uses the rerouting logic of the LiteServiceProvider that it is set up for.
  /// </summary>
  public abstract class LiteWebGeoLocatorServiceAgentBase : ServiceBase, IGeoLocatorService
  {
    #region Internal Simplification of a Web GeoLocator result
    /// <summary>
    /// The intreral helper class for a result address
    /// </summary>
    public class WebGeoLocatorResultAddress
    {
      public String Name { get; set; }
      public Envelope Bounds { get; set; }
    }
    #endregion

    #region Static Envelope Helpers
    /// <summary>
    /// Generates a framework envelope for the given string, expecting
    /// "xmin, ymin, xmax, ymax"
    /// </summary>
    protected static Envelope CreateWGS84Envelope(params double[] coords)
    {
      Envelope result = null;

      if (coords.Length == 4)
      {

        result = new Envelope(CoordinateSystemManager.Instance.CoordinateSystem(4326), coords[0], coords[1], coords[2], coords[3]);
      }
      if (coords.Length == 2)
      {
        double offset = 0.0001;
        result = new Envelope(CoordinateSystemManager.Instance.CoordinateSystem(4326), coords[0] - offset, coords[1] - offset, coords[0] + offset, coords[1] + offset);
      }

      return result;
    }

    /// <summary>
    /// Create a list of doubles from the string
    /// </summary>
    protected static IList<Double> GetDoublesFromString(string coordinateString)
    {
      List<Double> result = new List<double>();

      var splits = coordinateString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
      int len = splits.Length;

      if (len > 0)
      {
        double number;
        bool ok = false;
        var fi = new System.Globalization.NumberFormatInfo() { NumberDecimalSeparator = "." };

        for (int i = 0; i < len; i++)
        {
          ok = Double.TryParse(splits[i], System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowThousands, fi, out number);

          if (!ok)
          {
            result.Clear();
            break;
          }
          else
          {
            result.Add(number);
          }
        }
      }

      return result;
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructs the GeoLocator Agent for the specified Name and ExternalName
    /// </summary>
    /// <param name="name">The name of the geoLocator agent</param>
    /// <param name="externalName"></param>
    public LiteWebGeoLocatorServiceAgentBase(string name, string externalName)
    {
      Name = name;
      ExternalName = externalName;
    }
    #endregion

    #region ServiceProvider Helper
    /// <summary>
    /// The LiteWebServiceProviderBase  service provider
    /// </summary>
    protected LiteWebServiceProviderBase LiteServiceProvider
    {
      get { return ServiceProvider as LiteWebServiceProviderBase; }
    }

    /// <summary>
    /// Holds the X&Y service provider that should be used for rerouting GeoLocator requests through.
    /// </summary>
    protected XYServiceProvider RerouteViaServiceProvider
    {
      get { return LiteServiceProvider != null ? LiteServiceProvider.RerouteGeoLocatorVia : null; }
    }
    #endregion

    #region Locator Helpers
    /// <summary>
    /// Returns the base uri for the service
    /// </summary>
    private Uri ReroutedGeoLocatorServiceUri(Uri locatorUri)
    {
      var rerouteProvider = RerouteViaServiceProvider;
      if (rerouteProvider != null)
      {
        var builder = new UriBuilder(rerouteProvider.ServiceAddress)
        {
          Path = "forward",
          Query = String.Format("Address={0}", Uri.EscapeDataString(locatorUri.ToString()))
        };

        return builder.Uri;
      }

      return locatorUri;
    }
    #endregion

    #region API
    /// <summary>
    /// The name of the GeoLocator
    /// </summary>
    public string Name
    {
      get;
      private set;
    }

    /// <summary>
    /// The name of the GeoLocator
    /// </summary>
    public string ExternalName
    {
      get;
      private set;
    }

    /// <summary>
    /// The uri for the geolocator. Should be set by each implementation.
    /// </summary>
    protected Uri GeoLocatorUri
    {
      get;
      set;
    }

    /// <summary>
    /// Return the QUERY for the GeoLocate service call
    /// </summary>
    protected abstract string GeoLocateServiceQuery(string searchString, int maximumNumberOfResults);

    /// <summary>
    /// Get the result for the GeoLocate service call.
    /// The document parameter is the result from the service call, which is assumed 
    /// to be an XML structure
    /// </summary>
    protected abstract IList<WebGeoLocatorResultAddress> GetGeoLocateServiceResult(XDocument document);

    /// <summary>
    /// Return the URI for the GeoLocate service call; this is a default
    /// implementation that can optionally be overridden
    /// </summary>
    protected Uri GeoLocateServiceUri(string searchString, int maximumNumberOfResults)
    {
      var query = GeoLocateServiceQuery(searchString, maximumNumberOfResults);
      var builder = new UriBuilder(GeoLocatorUri) { Query = query };
      return builder.Uri;
    }
    #endregion

    #region Public IGeoLocatorService Api
    /// <summary>
    /// Gets the geoLocators asynchronously; returns one GeoLocator
    /// </summary>
    public Task<ServiceProviderDatumCollection<GeoLocator>> GetGeoLocatorsAsync()
    {
      var result = new ServiceProviderDatumCollection<GeoLocator>();

      result.Add(new GeoLocator(Name, ExternalName) { ServiceProviderGroup = new ServiceProviderGroup(this.ServiceProvider) });

      return TaskEx.FromResult(result);
    }

    /// <summary>
    /// Performs asynchonous GeoLocation using the information in the specified request
    /// </summary>
    public async Task<GeoLocateResult> GeoLocateAsync(GeoLocateRequest request)
    {
      GeoLocateResult result = new GeoLocateResult();
      result.Addresses = new GeoLocatorResultAddressCollection();
      result.Status = new GeoLocatorResultStatus { Success = false };

      if (!String.IsNullOrEmpty(request.Address.Place) && request.Address.Place.Length > 2)
      {
        try
        {
          var locateServiceUri = GeoLocateServiceUri(request.Address.Place, request.MaximumNumberOfResults);
          var searchUri = ReroutedGeoLocatorServiceUri(locateServiceUri);
          var searchResult = await new WebClient().DownloadStringTaskAsync(searchUri);

          if (!string.IsNullOrEmpty(searchResult))
          {
            var doc = XDocument.Parse(searchResult);
            foreach (var geoAdress in GetGeoLocateServiceResult(doc))
            {
              result.Addresses.Add(new GeoLocatorResultAddress { Place = geoAdress.Name, Description = geoAdress.Name, Envelope = geoAdress.Bounds });
            }

            result.Status.Success = true;
          }

        }
        catch (Exception)
        {}
      }
      else
      {
        result.Status.Message = ApplicationResources.GeoLocatorEmptyRequest;
      }

      return result;
    }
    #endregion
  }
}
