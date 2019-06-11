using System;

using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The application names and resource paths
  /// </summary>
  public partial class ViewModelLocator : ViewModelLocatorBase
  {
    /// <summary>
    /// The name of the server
    /// </summary>
    public static string SERVERNAME = "Server";

    /// <summary>
    /// The resource file for Lite
    /// </summary>
    public static string LITERESOURCEFILE = "/Lite;component/Resources/Application/LiteResources.xaml";

    /// <summary>
    /// The application id to be used by the server to identify the client by.
    /// </summary>
    //Lite 5.1.0.1 Pruebas TP
    private Guid APPLICATIONID = Guid.Parse("{C9A645E6-B7DF-41F9-B086-C126EBEBC5C2}");
    //private Guid APPLICATIONID = Guid.Parse("{DA0696B8-FC44-44E9-A3D4-DC812A73FC6F}");
    //Lite 5.1.0.1 DEV
    //private Guid APPLICATIONID = Guid.Parse("{67CD3795-5EC0-4DF9-9309-CEB2E2D92E7D}");
    // LiteFV TOTALPLAY PRODUCCION
    //private Guid APPLICATIONID = Guid.Parse("{C9A645E6-B7DF-41F9-B086-C126EBEBC5C2}");
    //Lite 5.1.0.1 DEV-IS
    //private Guid APPLICATIONID = Guid.Parse("{67CD3795-5EC0-4DF9-9309-CEB2E2D92E7D}");
    /// <summary>
    /// Indicates whether the layer selector should be used
    /// </summary>
    private bool UseBackdropLayerSelector = false;
  }
}
