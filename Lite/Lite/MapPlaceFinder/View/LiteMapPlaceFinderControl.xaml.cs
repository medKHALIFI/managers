using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

using SpatialEye.Framework.Client;
using SpatialEye.Framework.Threading;

namespace Lite
{
  /// <summary>
  /// The Place Finder control, wrapping GeoLocator presentation as well as a history
  /// of found items
  /// </summary>
  public partial class LiteMapPlaceFinderControl : UserControl
  {
    #region Private Fields
    /// <summary>
    /// Timer for delayed input
    /// </summary>
    private Timer _delayTimer;
    #endregion

    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public LiteMapPlaceFinderControl()
    {
      InitializeComponent();
    }
    #endregion

    #region Callbacks
    /// <summary>
    /// Callback when the search text is changed
    /// </summary>
    private void SearchTextChanged(object sender, TextChangedEventArgs e)
    {
      StopTimer();

      _delayTimer = new Timer((o) =>
      {
        StopTimer();

        UIDispatcher.BeginInvoke(() => UpdateSource(sender as TextBox));

      }, null, this.PlaceFinderViewModel.TypingDelayBeforeSendingRequest, Timeout.Infinite);
    }

    /// <summary>
    /// Stops the timer
    /// </summary>
    private void StopTimer()
    {
      var delayTimer = _delayTimer;

      // Reset the member
      _delayTimer = null;

      // Dispose of the timer
      if (delayTimer != null)
      {
        delayTimer.Dispose();
      }
    }

    /// <summary>
    /// Callback for dynamic updates in the text box
    /// </summary>
    /// <param name="box">The text box being updated</param>
    private void UpdateSource(TextBox box)
    {
      var expr = box.GetBindingExpression(TextBox.TextProperty);

      if (expr != null)
      {
        expr.UpdateSource();
      }
    }

    /// <summary>
    /// In case of getting focus, select all elements
    /// </summary>
    private void SearchStringTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
      this.SearchStringTextBox.SelectAll();
    }

    /// <summary>
    /// Callback when a key is pressed in the searchbox
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchTextKeyPressed(object sender, KeyEventArgs e)
    {
      if ((e.Key == Key.Up || e.Key == Key.Down) && CandidatesListBox.Items.Count > 0)
      {
        int direction = (e.Key == Key.Up) ? -1 : 1;

        int index = CandidatesListBox.SelectedIndex;
        int maxIndex = CandidatesListBox.Items.Count - 1;
        int newIndex = index + direction;

        newIndex = (newIndex > maxIndex) ? 0 : (newIndex < 0) ? maxIndex : newIndex;

        CandidatesListBox.SelectedItem = CandidatesListBox.Items[newIndex];
      }
      else if (e.Key == Key.Enter)
      {
        StopTimer();
        UpdateSource(sender as TextBox);
        ExecuteSearchCommand();
      }
      else if (e.Key == Key.Escape)
      {
        GeoLocatorViewModel viewModel = this.GeoLocatorViewModel;
        if (viewModel != null)
        {
          if (viewModel.GeoLocatorResults != null)
          {
            // Close the popup
            viewModel.GeoLocatorResults = null;
          }
        }
      }
    }

    /// <summary>
    /// Callback when a mouse button is pressed in the list box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CandidatesListBoxMousePressed(object sender, MouseButtonEventArgs e)
    {
      if (CandidatesListBox.SelectedItem != null)
      {
        ExecuteSearchCommand();
      }
    }
    #endregion

    #region Search Command
    /// <summary>
    /// Executing the search command
    /// </summary>
    private void ExecuteSearchCommand()
    {
      if (SearchButton.Command != null && SearchButton.Command.CanExecute(SearchButton.CommandParameter))
      {
        SearchButton.Command.Execute(SearchButton.CommandParameter);
      }
    }
    #endregion

    #region Properties
    /// <summary>
    /// The place finder view model
    /// </summary>
    private LiteMapPlaceFinderViewModel PlaceFinderViewModel
    {
      get
      {
        return DataContext as LiteMapPlaceFinderViewModel;
      }
    }

    /// <summary>
    /// The geoLocator view model
    /// </summary>
    private GeoLocatorViewModel GeoLocatorViewModel
    {
      get
      {
        return PlaceFinderViewModel.GeoLocatorViewModel;
      }
    }
    #endregion
  }
}
