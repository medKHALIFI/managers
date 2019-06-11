using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The footer page-specific context (viewModel), allowing the footer page (view) to bind to
  /// </summary>
  public class LitePrintA4Template3FooterPageContext : PrintPageContext
  {
    /// <summary>
    /// Holds the typed top-level settings
    /// </summary>
    private LitePrintA4Template3SettingsContext SettingsContext { get { return PrintContext as LitePrintA4Template3SettingsContext; } }
  }
}
