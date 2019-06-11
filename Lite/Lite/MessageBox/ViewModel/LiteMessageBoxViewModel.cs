using System;
using System.Threading.Tasks;
using System.Windows;

using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The view model for handling IMessageBoxService requests
  /// </summary>
  public class LiteMessageBoxViewModel : ViewModelBase, IMessageBoxService
  {
    #region Static Property Names
    /// <summary>
    /// Handles the visibility of the message box
    /// </summary>
    public static string ViewVisibilityPropertyName = "ViewVisibility";

    /// <summary>
    /// Handles the visibility of the cancel button
    /// </summary>
    public static string CancelVisibilityPropertyName = "CancelVisibility";

    /// <summary>
    /// Handles the caption of the message box
    /// </summary>
    public static string CaptionPropertyName = "Caption";

    /// <summary>
    /// Handles the text of the message box
    /// </summary>
    public static string TextPropertyName = "Text";
    #endregion

    #region Fields
    /// <summary>
    /// The visibility of the message box
    /// </summary>
    private Visibility _viewVisibility = Visibility.Collapsed;

    /// <summary>
    /// The visibility of the cancel button
    /// </summary>
    private Visibility _cancelVisibility = Visibility.Collapsed;

    /// <summary>
    /// The caption of the box
    /// </summary>
    private string _caption;

    /// <summary>
    /// The message of the box
    /// </summary>
    private string _text;

    /// <summary>
    /// The result to be sent back
    /// </summary>
    private MessageBoxResult _result = MessageBoxResult.OK;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructs the messageBox view model
    /// </summary>
    public LiteMessageBoxViewModel(Messenger messenger = null)
      : base(messenger)
    {
      Resources = new Lite.Resources.Localization.ApplicationResources();
      if (!IsInDesignMode)
      {
        SetupCommands();
      }
    }
    #endregion

    #region Commands
    /// <summary>
    /// Sets up the commands to use in the Message box
    /// </summary>
    private void SetupCommands()
    {
      OKCommand = new RelayCommand(() =>
        {
          Result = MessageBoxResult.OK;
          ViewVisibility = Visibility.Collapsed;
        });

      CancelCommand = new RelayCommand(() =>
      {
        Result = MessageBoxResult.Cancel;
        ViewVisibility = Visibility.Collapsed;
      });
    }

    /// <summary>
    /// The command to be used for an OK action
    /// </summary>
    public RelayCommand OKCommand { get; private set; }

    /// <summary>
    /// The command to be used for a Cancel action
    /// </summary>
    public RelayCommand CancelCommand { get; private set; }
    #endregion

    #region Public Properties
    /// <summary>
    /// The visibility of the message box
    /// </summary>
    public Visibility ViewVisibility
    {
      get { return _viewVisibility; }
      set
      {
        if (_viewVisibility != value)
        {
          _viewVisibility = value;
          RaisePropertyChanged(ViewVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// The visibility of the cancel button
    /// </summary>
    public Visibility CancelVisibility
    {
      get { return _cancelVisibility; }
      set
      {
        if (_cancelVisibility != value)
        {
          _cancelVisibility = value;
          RaisePropertyChanged(CancelVisibilityPropertyName);
        }
      }
    }

    /// <summary>
    /// The caption of the message box
    /// </summary>
    public string Caption
    {
      get { return _caption; }
      set
      {
        if (_caption != value)
        {
          _caption = value;
          RaisePropertyChanged(CaptionPropertyName);
        }
      }
    }

    /// <summary>
    /// The text of the message box
    /// </summary>
    public string Text
    {
      get { return _text; }
      set
      {
        if (_text != value)
        {
          _text = value;
          RaisePropertyChanged(TextPropertyName);
        }
      }
    }

    /// <summary>
    /// Returns the last result of the message box
    /// </summary>
    public MessageBoxResult Result
    {
      get { return _result; }
      private set { _result = value; }
    }
    #endregion

    #region The Message Box Service
    /// <summary>
    /// Shows the message box with the specifiec caption
    /// </summary>
    /// <param name="messageBoxText">The main text to display</param>
    /// <param name="caption">The caption</param>
    /// <param name="button">The button (combination) to use</param>
    /// <param name="defaultResult">The default result of the message box</param>
    /// <returns>The user actived result</returns>
    public async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult)
    {
      Text = messageBoxText;
      Caption = caption;
      Result = defaultResult;
      CancelVisibility = button == MessageBoxButton.OKCancel ? Visibility.Visible : Visibility.Collapsed;

      ViewVisibility = Visibility.Visible;

      while (ViewVisibility == Visibility.Visible)
      {
        await TaskFunctions.Delay(100);
      }

      return Result;
    }

    /// <summary>
    /// Shows the message box with the specifiec caption with a default result of ok
    /// </summary>
    /// <param name="messageBoxText">The main text to display</param>
    /// <param name="caption">The caption</param>
    /// <param name="button">The button (combination) to use</param>
    /// <returns>The user actived result</returns>
    public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button)
    {
      return ShowAsync(messageBoxText, caption, button, MessageBoxResult.OK);
    }

    /// <summary>
    /// Shows the message box with the specifiec caption with a default result of ok
    /// and one button OK
    /// </summary>
    /// <param name="messageBoxText">The main text to display</param>
    /// <param name="caption">The caption</param>
    /// <returns>The user actived result</returns>
    public Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption)
    {
      return ShowAsync(messageBoxText, caption, MessageBoxButton.OK, MessageBoxResult.OK);
    }

    /// <summary>
    /// Shows the message box with the specifiec caption with a default result of ok
    /// and one button OK 
    /// </summary>
    /// <param name="messageBoxText">The main text to display</param>
    /// <returns>The user actived result</returns>    
    public Task<MessageBoxResult> ShowAsync(string messageBoxText)
    {
      return ShowAsync(messageBoxText, String.Empty, MessageBoxButton.OK, MessageBoxResult.OK);
    }
    #endregion
  }
}
