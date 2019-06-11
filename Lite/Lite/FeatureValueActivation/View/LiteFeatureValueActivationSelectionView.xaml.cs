using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ComponentModel.Design;

namespace Lite
{
  /// <summary>
  /// The Lite Feature Value activation dialog that displays a set of values
  /// that can be activated (ie smart-links); allows the user to select one of 
  /// these values.
  /// </summary>
  public partial class LiteFeatureValueActivationSelectionView : UserControl
  {
    #region Constructor
    /// <summary>
    /// Default constructor for the activation view model
    /// </summary>
    public LiteFeatureValueActivationSelectionView()
    {
      InitializeComponent();
      if (!DesignModeHelper.IsInDesignMode)
      {
        DataContextChanged += LiteFeatureValueActivationSelectionView_DataContextChanged;
      }
    }
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
    #endregion

    #region Property Changes
    /// <summary>
    /// Callback for change in DataContext; registers for property change handling
    /// </summary>
    void LiteFeatureValueActivationSelectionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var model = this.DataContext as AuthenticationViewModel;
      if (model != null)
      {
        model.PropertyChanged += LiteFeatureValueActivationViewModelPropertyChanged;
      }
    }

    /// <summary>
    /// Callback for property changes in the Authentication View Model, allowing the dialog
    /// to automatically kick in when there is no authentication context or when that context
    /// is not authenticated
    /// </summary>
    void LiteFeatureValueActivationViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == AuthenticationViewModel.CurrentAuthenticationContextPropertyName)
      {
      }
    }
    #endregion
  }
}
