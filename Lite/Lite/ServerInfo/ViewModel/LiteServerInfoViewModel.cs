using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.ServiceProviders.XY;

namespace Lite
{
  /// <summary>
  /// The viewModel that handles server information retrieval; which holds
  /// the set of service providers.
  /// </summary>
  public class LiteServerInfoViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The property name of the server name property
    /// </summary>
    public static string ServerNamePropertyName = "ServerName";

    /// <summary>
    /// The property name of the server description property
    /// </summary>
    public static string ServerDescriptionPropertyName = "ServerDescription";
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the ServerInfoViewModel for the specified provider
    /// </summary>
    /// <param name="provider">The provider to retrieve server information from</param>
    public LiteServerInfoViewModel(XYServiceProvider provider)
    {
      Provider = provider;
    }
    #endregion

    #region Properties
    /// <summary>
    /// The provider to retrieve server information from
    /// </summary>
    public XYServiceProvider Provider
    {
      get;
      private set;
    }
    #endregion

    #region API
    /// <summary>
    /// The derived ServerName information
    /// </summary>
    public string ServerName
    {
      get 
      {
        var info = ServiceProviderInfo;
        return info != null ? info.Name : string.Empty; 
      }
    }

    /// <summary>
    /// The derived server description information
    /// </summary>
    public string ServerDescription
    {
      get
      {
        if (IsInDesignMode)
        {
          return "Server Description";
        }

        var info = ServiceProviderInfo;
        if (info != null)
        {
          bool includeServerNameAndBits = false;

          if (includeServerNameAndBits)
          {
            var serverName = this.ServerName;
            var nameString = !String.IsNullOrEmpty(serverName) ? string.Concat(" ", serverName) : string.Empty;
            var bitString = info.Is64BitProcess ? "64-bit" : "32-bit";
            return string.Format("{0}{1} ({2})", ViewModelLocator.SERVERNAME, nameString, bitString);
          }
          else
          {
            return this.ServerName ?? string.Empty;
          }
        }

        return string.Empty;
      }
    }

    /// <summary>
    /// The server information that is retrieved from the server
    /// </summary>
    private ServiceProviderInfo ServiceProviderInfo
    {
      get { return AuthenticationContext != null ? AuthenticationContext.ServiceProviderInfo : null; }
    }

    /// <summary>
    /// The authentication has changed, notify the changes in serviceProviderInfo
    /// </summary>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      RaisePropertyChanged(ServerNamePropertyName);
      RaisePropertyChanged(ServerDescriptionPropertyName);
    }
    #endregion

    #region API
    /// <summary>
    /// Gets the reference service providers for our Service Provider asynchronously.
    /// </summary>
    public async Task<List<ServiceProvider>> GetReferenceServiceProvidersAsync()
    {
      var providers = new List<ServiceProvider>();
      if (Provider != null && IsAuthenticated)
      {
        var service = Provider.GetService<IServiceProviderInfoService>();

        try
        {
          var xyProviders = await service.GetReferenceServiceProvidersAsync();
          if (xyProviders != null)
          {
            foreach (var provider in xyProviders)
            {
              providers.Add(provider);
            }
          }
        }
        finally
        { }
      }

      return providers;
    }
    #endregion
  }
}
