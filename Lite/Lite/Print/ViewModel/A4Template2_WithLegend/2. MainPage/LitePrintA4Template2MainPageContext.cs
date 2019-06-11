using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The main page-specific context (viewModel), allowing the main page (view) to bind to
  /// </summary>
  public class LitePrintA4Template2MainPageContext : PrintPageContext
  {
    /// <summary>
    /// Holds the typed top-level settings
    /// </summary>
    private LitePrintA4Template2SettingsContext SettingsContext { get { return PrintContext as LitePrintA4Template2SettingsContext; } }
  }
}
