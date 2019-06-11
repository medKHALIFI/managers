using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.ComponentModel.Design;

namespace Lite
{
  /// <summary>
  /// The authentication dialog, covering a full screen but acts like a modal child window.
  /// </summary>
  public partial class LiteAuthenticationDialog : UserControl
  {
    #region Constructor
    /// <summary>
    /// Default constructorr for the authentication dialog
    /// </summary>
    public LiteAuthenticationDialog()
    {
      InitializeComponent();
      if (!DesignModeHelper.IsInDesignMode)
      {
        DataContextChanged += AuthenticationDialog_DataContextChanged;
      }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Sets the focus to the control
    /// </summary>
    /// <param name="setFocusToPasswordBox">Set explicit focus to the password box</param>
    private void SetFocus(bool setFocusToPasswordBox = false)
    {
      // First give the silverlight control focus
      try
      {
        // Doesn't work outside browser - catch any issues
        System.Windows.Browser.HtmlPage.Plugin.Focus();
      }
      catch
      { }

      //// And then ourself
      //this.Focus();

      // Set focus to the password box
      if (setFocusToPasswordBox)
      {
        PasswordBox.Focus();
      }
      else
      {
        UserNameTextBox.Focus();
      }

    }
    #endregion

    #region Property Changes

    /// <summary>
    /// Callback for change in DataContext; registers for property change handling
    /// </summary>
    void AuthenticationDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var model = this.DataContext as AuthenticationViewModel;
      if (model != null)
      {
        model.PropertyChanged += AuthenticationViewModelPropertyChanged;
      }

    }

    /// <summary>
    /// Callback for property changes in the Authentication View Model, allowing the dialog
    /// to automatically kick in when there is no authentication context or when that context
    /// is not authenticated
    /// </summary>
    void AuthenticationViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == AuthenticationViewModel.CurrentAuthenticationContextPropertyName)
      {
        AuthenticationViewModel model = sender as AuthenticationViewModel;
        if (model != null && (model.CurrentAuthenticationContext == null || !model.CurrentAuthenticationContext.IsAuthenticated))
        {
          PasswordBox.Password = string.Empty;
          SetFocus(!string.IsNullOrEmpty(model.AuthenticationUsername));
        }
      }
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// The user control has loaded; used via binding as a callback for the loaded event
    /// </summary>
    private void AuthenticationDialogLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
      // Set focus to this control
      SetFocus();
    }

    /// <summary>
    /// Callback for presses in the text-boxes; whenever the user presses enter
    /// the dialog attempts to fire the LoginButton's command
    /// </summary>
    private void KeyDownPressed(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Enter && LoginButton.Command != null && LoginButton.Command.CanExecute(null))
      {
        LoginButton.Command.Execute(null);
        e.Handled = true;
      }
    }

    /// <summary>
    /// Callback for immediate changes in the password text box; not done via databinding
    /// where the behavior is to only update the viewmodel when the textbox loses focus
    /// </summary>
    private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
      var model = this.DataContext as AuthenticationViewModel;

      if (model != null)
      {
        model.AuthenticationPassword = PasswordBox.Password;
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// Holds the title property
    /// </summary>
    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(LiteAuthenticationDialog), new PropertyMetadata("Login"));
    #endregion
  }
}
