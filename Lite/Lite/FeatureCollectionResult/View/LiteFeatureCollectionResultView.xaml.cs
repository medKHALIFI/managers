using System.Windows.Controls;

namespace Lite
{
  /// <summary>
  /// The Result View, displaying the Feature Collection in a Grid. When
  /// bound to a LiteFeatureCollectionResultViewModel, will display the contents
  /// that the viewModel represents.
  /// </summary>
  public partial class LiteFeatureCollectionResultView : UserControl
  {
    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteFeatureCollectionResultView()
    {
      InitializeComponent();
    }
    #endregion
  }
}
