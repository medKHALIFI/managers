using System.Collections.ObjectModel;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.ServiceProviders;

using Lite.Resources.Localization;

namespace Lite
{
  /// <summary>
  /// A viewModel that handles setting up a New Client Query using one of 
  /// the available modes that a new query can be created with.
  /// </summary>
  public class LiteNewUserQueryViewModel : ViewModelBase
  {
    #region Property Names
    /// <summary>
    /// The selected mode
    /// </summary>
    public static string SelectedModePropertyName = "SelectedMode";

    /// <summary>
    /// Is the expression builder enabled
    /// </summary>
    public static string ExpressionBuilderIsEnabledPropertyName = "ExpressionBuilderIsEnabled";

    /// <summary>
    /// New Label
    /// </summary>
    public const string NewQueryLabelPropertyName = "NewQueryLabel";

    /// <summary>
    /// The Query Name
    /// </summary>
    public const string QueryNamePropertyName = "QueryName";
    #endregion

    #region Fields
    /// <summary>
    /// The selected mode, that handles the way a new query is to be created
    /// </summary>
    private LiteNewUserQueryViewModelMode _selectedMode;

    /// <summary>
    /// The expression builder mode; that allows creating a new query by setting up
    /// an expression using the builder
    /// </summary>
    private LiteNewUserQueryViewModelBuilderMode _expressionBuilderMode;

    /// <summary>
    /// Is the expression builder enabled; ie, is it the selected mode
    /// </summary>
    private bool _expressionBuilderIsEnabled;

    /// <summary>
    /// The query name
    /// </summary>
    private string _queryName;
    #endregion

    #region Constructor
    /// <summary>
    /// The new Query View Model constructor
    /// </summary>
    internal LiteNewUserQueryViewModel(Messenger messenger = null)
      : base(messenger)
    {
      FeatureTableViewModel = new FeatureTableComboBoxViewModel(messenger);
      FeatureTableViewModel.ServiceProviderGroupTypeProperties.AllowedGroupTypes = new ServiceProviderGroupType[] 
      { 
        ServiceProviderGroupType.Business,
        ServiceProviderGroupType.Analysis
      };

      FeatureTableViewModel.PropertyChanged += TableSelection_PropertyChanged;

      SetCurrentCultureLabels();

      SetupModes();
    }
        
    /// <summary>
    /// Current culture has changed
    /// </summary>
    protected override void OnCurrentCultureChanged(System.Globalization.CultureInfo currentCultureInfo)
    {
      base.OnCurrentCultureChanged(currentCultureInfo);

      SetCurrentCultureLabels();
    }

    /// <summary>
    /// Set the labels for the current culture
    /// </summary>
    private void SetCurrentCultureLabels()
    {
      FeatureTableViewModel.ServiceProviderProperties.Name = ApplicationResources.QueriesNewQueryTableServiceProvider;
      FeatureTableViewModel.ServiceProviderGroupTypeProperties.Name = ApplicationResources.QueriesNewQueryTableServiceProviderGroupType;
      FeatureTableViewModel.ServiceProviderGroupProperties.Name = ApplicationResources.QueriesNewQueryTableServiceProviderGroup;
      FeatureTableViewModel.DatumProperties.Name = ApplicationResources.QueriesNewQueryTableServiceProviderTable;

      // New Query Label
      RaisePropertyChanged(NewQueryLabelPropertyName);
    }
    #endregion

    #region Setup
    /// <summary>
    /// Set up all available modes and set the default Selected Mode from these
    /// </summary>
    private void SetupModes()
    {
      Modes = new ObservableCollection<LiteNewUserQueryViewModelMode>();

      // Using the Builder Mode only
      _expressionBuilderMode = new LiteNewUserQueryViewModelBuilderMode();
      Modes.Add(_expressionBuilderMode);

      // Set the first mode to be the selected mode
      SelectedMode = Modes[0];
    }
    #endregion

    #region Table Properties Changed
    /// <summary>
    /// A new table has been selected, set up the available modes using the selected table 
    /// </summary>
    void TableSelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == FeatureTableComboBoxViewModel.SelectedFeatureTableDescriptorPropertyName)
      {
        var table = FeatureTableViewModel.SelectedFeatureTableDescriptor;
        if (table != null)
        {
          this.SetupForTable(FeatureTableViewModel.SelectedFeatureTableDescriptor);
        }
      }
    }

    /// <summary>
    /// Set up the modes for the specified table descriptor
    /// </summary>
    /// <param parameterName="table"></param>
    private async void SetupForTable(FeatureTableDescriptor table)
    {
      // Make sure the table has its field descriptors
      await table.EvaluateAsync();

      // Indicate table descriptor change
      foreach (var mode in Modes)
      {
        mode.TableDescriptor = table;
      }

      if (SelectedMode != null && SelectedMode.IsEnabled)
      {
        // The active mode is still enabled
      }
      else
      {
        LiteNewUserQueryViewModelMode newSelectedMode = null;
        foreach (var mode in Modes)
        {
          if (mode.IsEnabled)
          {
            newSelectedMode = mode;
            break;
          }
        }

        // Set the new mode
        SelectedMode = newSelectedMode;
      }
    }

    /// <summary>
    /// Reset the name of the query
    /// </summary>
    internal void ResetNewQueryName()
    {
      this.QueryName = ApplicationResources.Query;
    }
    #endregion

    #region API
    /// <summary>
    /// The view model that handles selection of a table(descriptor) that
    /// will be used for setting up a new client query for
    /// </summary>
    public FeatureTableComboBoxViewModel FeatureTableViewModel
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns the label for 'Query:'
    /// </summary>
    public string NewQueryLabel
    {
      get { return ApplicationResources.Query; }
    }

    /// <summary>
    /// The query name
    /// </summary>
    public string QueryName
    {
      get { return _queryName; }
      set
      {
        if (_queryName != value)
        {
          _queryName = value;
          RaisePropertyChanged(QueryNamePropertyName);
        }
      }
    }

    /// <summary>
    /// Holds the available modes for setting up a new query
    /// </summary>
    public ObservableCollection<LiteNewUserQueryViewModelMode> Modes
    {
      get;
      private set;
    }

    /// <summary>
    /// The selected mode that will be used to create a new query with
    /// </summary>
    public LiteNewUserQueryViewModelMode SelectedMode
    {
      get { return _selectedMode; }
      set
      {
        if (_selectedMode != value)
        {
          _selectedMode = value;
          RaisePropertyChanged(SelectedModePropertyName);

          ExpressionBuilderIsEnabled = _selectedMode == _expressionBuilderMode;
        }
      }
    }

    /// <summary>
    /// The epression builder, as owned by the expressionBuilder mode
    /// </summary>
    public ExpressionBuilderViewModel ExpressionBuilder
    {
      get { return _expressionBuilderMode.ExpressionBuilder; }
    }

    /// <summary>
    /// Is the expression builder enabled
    /// </summary>
    public bool ExpressionBuilderIsEnabled
    {
      get { return _expressionBuilderIsEnabled; }
      private set
      {
        if (_expressionBuilderIsEnabled != value)
        {
          _expressionBuilderIsEnabled = value;
          RaisePropertyChanged(ExpressionBuilderIsEnabledPropertyName);
        }
      }
    }
    #endregion
  }
}
