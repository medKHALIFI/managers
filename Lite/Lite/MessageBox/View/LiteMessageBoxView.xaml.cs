using System.Windows;
using System.Windows.Controls;

using SpatialEye.Framework.ComponentModel.Design;

namespace Lite
{
  /// <summary>
  /// The Lite MessageBox user interface
  /// </summary>
  public partial class LiteMessageBoxView : UserControl
  {
    #region Constructor
    /// <summary>
    /// Constructs the appearance of the Lite MessageBox
    /// </summary>
    public LiteMessageBoxView()
    {
      InitializeComponent();

      if (!DesignModeHelper.IsInDesignMode)
      {
        Loaded += LiteMessageBoxView_Loaded;
      }
    }
    #endregion

    #region The Content Max Width
    /// <summary>
    /// The maximum width of the message box
    /// </summary>
    public int ContentMaxWidth
    {
      get { return (int)GetValue(ContentMaxWidthProperty); }
      set { SetValue(ContentMaxWidthProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ContentMaxWidth.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ContentMaxWidthProperty =
        DependencyProperty.Register("ContentMaxWidth", typeof(int), typeof(LiteMessageBoxView), new PropertyMetadata(300));
    #endregion

    #region Helpers
    /// <summary>
    /// Sets the focus to the control
    /// </summary>
    private void SetFocus()
    {
      // First give the silverlight control focus
      try
      {
        // Doesn't work outside browser - catch any issues
        System.Windows.Browser.HtmlPage.Plugin.Focus();
      }
      catch
      { }

      // And then ourself
      this.Focus();
    }

    /// <summary>
    /// The user control has loaded; used via binding as a callback for the loaded event
    /// </summary>
    void LiteMessageBoxView_Loaded(object sender, RoutedEventArgs e)
    {
      // Set focus to this control
      SetFocus();
    }
    #endregion
  }
}
