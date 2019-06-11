using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Browser;

using ServiceStack;
using ServiceStack.Text;

using SpatialEye.Framework.Authentication;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Serializers;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Maps;

namespace Lite
{
  public class ServerEventsViewModel : ViewModelBase, IDisposable
  {
    #region Instance
    /// <summary>
    /// The instance of the server events view model
    /// </summary>
    internal static ServerEventsViewModel Instance { get; private set; }
    #endregion

    #region Fields
    /// <summary>
    /// Holds the task that waits for server push events.
    /// </summary>
    private Task _serverEventListenerTask;

    private String _serviceUrl;

    public const String IsConnectedPropertyName = "IsConnected";

    /// <summary>
    /// Backing field for IsConnected property.
    /// </summary>
    private Boolean _isConnected;

    /// <summary>
    /// Holds a reference to the ServerEventsClient instance.
    /// </summary>
    private ServerEventsClient _client;
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new ServerEventsViewModel instance.
    /// </summary>
    /// <param name="baseUri"></param>
    /// <param name="messenger"></param>
    public ServerEventsViewModel(String baseUri, Messenger messenger = null)
    {
      // Store the instance for easy reference from the serverNotificationsReceiver
      Instance = this;

      WebRequest.RegisterPrefix("http://", WebRequestCreator.ClientHttp);

      JsConfig<Envelope>.RawDeserializeFn = (input) =>
      {
        var rawGeometry = new JsonSerializer<GeoJsonObject>().DeserializeFromString(input);

        if (rawGeometry["type"].ToString() == "Polygon")
        {
          var rings = JsonSerializer.DeserializeFromString<double[][][]>(rawGeometry["coordinates"].ToString());

          if (!rings.Any())
          {
            return null;
          }

          var outerRing = rings[0];

          if (outerRing.Count() != 5)
          {
            return null;
          }

          try
          {
            var coordinates = outerRing.Select(x => new Coordinate(x[0], x[1])).ToArray();

            return new Envelope(CoordinateSystemManager.Instance.CoordinateSystem(4326), coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
          }
          catch
          {
            return null;
          }
        }

        return null;
      };

      _serviceUrl = baseUri;
    }
    #endregion

    #region Handle Changes
    /// <summary>
    /// Process the supplied changes
    /// </summary>
    /// <param name="changes"></param>
    internal void ProcessChanges(MapLayerChangeCollection changes)
    {
      // Send the changes on the messenger
      Messenger.Send(new LiteMapLayerChangeMessage(this, changes));
    }
    #endregion

    #region Properties
    public Boolean IsConnected
    {
      get
      {
        return _isConnected;
      }

      set
      {
        _isConnected = value;

        RaisePropertyChanged(IsConnectedPropertyName);
      }
    }

    private ServerEventsClient Client
    {
      get
      {
        if (_client == null)
        {
          _client = new ServerEventsClient(_serviceUrl, "geonotes");

          _client.RegisterNamedReceiver<ServerNotificationsReceiver>("server");
        }

        return _client;
      }
    }
    #endregion

    #region Public API
    protected override void OnAuthenticationChanged(AuthenticationContext context, bool isAuthenticated)
    {
      if (!isAuthenticated)
      {
        return;
      }

      _serverEventListenerTask = Task.Factory.StartNew((task) =>
      {
        Client.Start();
      },

      TaskCreationOptions.LongRunning);
    }

    public void Dispose()
    {
      var client = _client;

      if (client != null)
      {
        client.Dispose();
      }
    }
    #endregion
  }
}
