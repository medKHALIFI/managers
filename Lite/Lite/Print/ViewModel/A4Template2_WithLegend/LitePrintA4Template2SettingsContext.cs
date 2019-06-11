using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// The TemplateContext, that contains all Data the Template can bind to.
  /// It can also be used by a Settings Control to bind to the same elements as well.
  /// </summary>
  public class LitePrintA4Template2SettingsContext : PrintContext
  {
    #region Property Notification
    /// <summary>
    /// The title property name
    /// </summary>
    public const string TitlePropertyName = "Title";
    #endregion

    #region Fields
    /// <summary>
    /// The title
    /// </summary>
    private string _title;
    #endregion

    #region Constructor
    /// <summary>
    /// Constructor that sets up the Template Settings using the specified title
    /// </summary>
    /// <param name="title"></param>
    public LitePrintA4Template2SettingsContext(string title)
    {
      _title = title;
      SetupCommands();
    }
    #endregion

    #region Commands
    /// <summary>
    /// The reset title command, that sets the title to its default (empty) value
    /// </summary>
    public RelayCommand ResetTitleCommand { get; set; }

    /// <summary>
    /// Sets up the commands available to manipulate the Template Settings
    /// </summary>
    private void SetupCommands()
    {
      ResetTitleCommand = new RelayCommand(ResetTitel);
    }

    /// <summary>
    /// Resets the title
    /// </summary>
    public void ResetTitel()
    {
      Title = null;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Holds the title to be used in the Template
    /// </summary>
    public string Title
    {
      get { return _title ?? ViewModelLocator.LiteName; }
      set
      {
        if (_title != value)
        {
          _title = value;

          RaisePropertyChanged(TitlePropertyName);
        }
      }
    }

    /// <summary>
    /// The copyright text to display
    /// </summary>    
    public string CopyrightText
    {
      get
      {
        return LiteClientSettingsViewModel.Instance.PrintCopyrightText;
      }
    }
    #endregion
  }
}
