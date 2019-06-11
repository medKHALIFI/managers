using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Lite
{
  /// <summary>
  /// The Map Bar Control
  /// </summary>
  public partial class LiteMapBar : UserControl
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteMapBar()
    {
      InitializeComponent();

      SetupSubBars();
    }

    /// <summary>
    /// Setup subbars, subscribe to the changed event
    /// </summary>
    private void SetupSubBars()
    {
      foreach (var sub in this.GetDescendants<LiteSubMapBar>())
      {
        sub.IsActiveChanged += sub_IsActiveChanged;
      }
    }

    /// <summary>
    /// Callback from the changed event, make sure only one bar is active
    /// </summary>
    void sub_IsActiveChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var control = sender as LiteSubMapBar;

      if (control.IsActive)
      {
        foreach (var sub in this.GetDescendants<LiteSubMapBar>().Where((a) => a != control && a.IsActive))
        {
          sub.IsActive = false;
        }
      }
    }
  }
}
