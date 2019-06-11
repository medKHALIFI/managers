using System.Windows;

using SpatialEye.Framework.Client;
using System.Windows.Controls;

namespace Lite
{
  /// <summary>
  /// The View Model handling all logic of the Map Bar, which is the
  /// Bar above the Map with access to various map/search related
  /// functionality.
  /// </summary>
  public class LiteMapBarViewModel : ViewModelBase
  {
    #region PropertyNames
    /// <summary>
    /// The print button's visibility 
    /// </summary>
    public const string PrintVisibilityPropertyName = "PrintVisibility";
    #endregion

    #region Fields
    /// <summary>
    /// The print button's visibility, which is dependent on the Client Settings
    /// </summary>
    private bool _isPrintVisible = true;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor for the lite MapBar view model
    /// </summary>
    public LiteMapBarViewModel(Messenger messenger = null)
      : base(messenger)
    {
      // Get a handle on the Application's resources, for easy binding
      Resources = new Lite.Resources.Localization.ApplicationResources();
    }
    #endregion

    #region Authentication Changed
    /// <summary>
    /// Callback for changes in authentication (context)
    /// </summary>
    /// <param name="context">The new authentication context</param>
    /// <param name="isAuthenticated">A flag indicating success of authentication</param>
    protected override void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      base.OnAuthenticationChanged(context, isAuthenticated);

      // Get the print settings
      this.IsPrintVisible = LiteClientSettingsViewModel.Instance.AllowPrint;
    }
    #endregion

    #region Print
    /// <summary>
    /// Flag indicating whether the print button is visible
    /// </summary>
    public bool IsPrintVisible
    {
      get { return _isPrintVisible; }
      set
      {
        if (_isPrintVisible != value)
        {
          _isPrintVisible = value;
          RaisePropertyChanged(PrintVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// Returns the print button's visibility
    /// </summary>
    public Visibility PrintVisibility
    {
      get { return _isPrintVisible ? Visibility.Visible : Visibility.Collapsed; }
    }
    #endregion
  }
}
