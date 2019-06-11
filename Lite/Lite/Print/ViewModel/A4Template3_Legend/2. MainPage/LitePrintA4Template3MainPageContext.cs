using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The main page-specific context (viewModel), allowing the main page (view) to bind to
  /// </summary>
  public class LitePrintA4Template3MainPageContext : PrintPageContext
  {
    /// <summary>
    /// Holds the typed top-level settings
    /// </summary>
    private LitePrintA4Template3SettingsContext SettingsContext { get { return PrintContext as LitePrintA4Template3SettingsContext; } }
  }
}
