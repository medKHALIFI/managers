using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The footer page-specific context (viewModel), allowing the footer page (view) to bind to
  /// </summary>
  public class LitePrintA4Template1FooterPageContext : PrintPageContext
  {
    /// <summary>
    /// Holds the typed top-level settings
    /// </summary>
    private LitePrintA4Template1SettingsContext SettingsContext { get { return PrintContext as LitePrintA4Template1SettingsContext; } }
  }
}
