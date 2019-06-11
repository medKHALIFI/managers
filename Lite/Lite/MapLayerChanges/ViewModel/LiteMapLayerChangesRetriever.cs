using Microsoft.Practices.ServiceLocation;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features.Services;
using SpatialEye.Framework.Maps;
using SpatialEye.Framework.Maps.Services;
using SpatialEye.Framework.ServiceProviders;
using SpatialEye.Framework.Threading;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Lite
{
  /// <summary>
  /// The mapLayer changes retriever, responsible for handling the long pulling
  /// for changes from the server (to circumvent push notification)
  /// </summary>
  public class LiteMapLayerChangesRetriever : ViewModelBase
  {
    #region Static
    /// <summary>
    /// Check changes every n seconds
    /// </summary>
    private static int CheckEditabilityChangesEveryMs = 30000;
    #endregion

    #region Fields
    /// <summary>
    /// Indicates whether we should track changes
    /// </summary>
    private bool _trackChanges = true;

    /// <summary>
    /// Timer creation sync root
    /// </summary>
    private object _timerCreationSyncRoot = new object();

    /// <summary>
    /// The timer
    /// </summary>
    private Timer _timer;

    /// <summary>
    /// The last request
    /// </summary>
    private DateTime _lastRequestDateTime;
    #endregion

    #region Constructor

    /// <summary>
    /// Constructs the changes retriever
    /// </summary>
    public LiteMapLayerChangesRetriever(Messenger messenger = null)
      : base(messenger)
    {
      AttachToMessenger();
    }
    #endregion

    #region Messenger
    /// <summary>
    /// Attach to the messenger
    /// </summary>
    private void AttachToMessenger()
    {
      Messenger.Register<LiteFeatureTransactionMessage>(this, HandleFeatureTransaction);
    }

    /// <summary>
    /// Handles a feature transaction result by requesting changes from the server
    /// </summary>
    private void HandleFeatureTransaction(LiteFeatureTransactionMessage change)
    {
      CheckMapLayerChanges();
    }
    #endregion

    #region Authentication changes
    /// <summary>
    /// When authentication changes, start the application
    /// </summary>
    protected override async void OnAuthenticationChanged(SpatialEye.Framework.Authentication.AuthenticationContext context, bool isAuthenticated)
    {
      StopTimer();

      if (isAuthenticated)
      {
        // Hand over the initial value; which is dateTime.MinValue (with some extra seconds,
        // being picked up by the server to tell the interval)
        _lastRequestDateTime = DateTime.MinValue.AddMilliseconds(CheckEditabilityChangesEveryMs);

        var hasEditable = await HasEditableCollections();
        if (hasEditable)
        {
          StartTimer();
        }
      }
    }

    /// <summary>
    /// Returns a flag if there are any editable collections
    /// </summary>
    private async Task<bool> HasEditableCollections()
    {
      bool hasEditable = false;
      if (IsAuthenticated)
      {
        var service = ServiceLocator.Current.GetInstance<ICollectionService>();

        // Get all the bare descriptors
        var request = new GetDDRequest { IncludeFields = false, GroupTypes = new ServiceProviderGroupType[] { ServiceProviderGroupType.Business } };

        var sourceDescriptors = await service.GetDDAsync(request);
        if (sourceDescriptors != null && sourceDescriptors.Count == 1)
        {
          var businessSourceDescriptor = sourceDescriptors[0];
          foreach (var tableDescriptor in businessSourceDescriptor.TableDescriptors)
          {
            var editProps = tableDescriptor.EditabilityProperties;

            if (editProps.AllowInsert || editProps.AllowUpdate || editProps.AllowDelete)
            {
              hasEditable = true;
              break;
            }
          }
        }
      }

      return hasEditable;
    }
    #endregion

    #region Timer
    /// <summary>
    /// Ensure the timer
    /// </summary>
    private void EnsureTimer()
    {
      if (_timer == null)
      {
        lock (_timerCreationSyncRoot)
        {
          if (_timer == null)
          {
            _timer = new Timer(new TimerCallback(TimerCheckAllChanges), null, Timeout.Infinite, Timeout.Infinite);
          }
        }
      }
    }

    /// <summary>
    /// Starts the timer
    /// </summary>
    private void StartTimer()
    {
      EnsureTimer();

      var timer = _timer;
      if (timer != null)
      {
        _timer.Change(TimeSpan.FromMilliseconds(CheckEditabilityChangesEveryMs), TimeSpan.FromMilliseconds(CheckEditabilityChangesEveryMs));
      }
    }

    /// <summary>
    /// Stops the timer
    /// </summary>
    private void StopTimer()
    {
      var timer = _timer;
      if (timer != null)
      {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
      }
    }

    /// <summary>
    /// Checks all changes, done via timer
    /// </summary>
    private void TimerCheckAllChanges(object state)
    {
      UIDispatcher.BeginInvoke(CheckMapLayerChanges);
    }

    /// <summary>
    /// Checks all the changes in layers since the last time
    /// </summary>
    private async void CheckMapLayerChanges()
    {
      StopTimer();

      if (_trackChanges && IsAuthenticated)
      {
        _trackChanges = false;

        try
        {
          var mapService = ServiceLocator.Current.GetInstance<IMapService>();
          var requestDateTime = _lastRequestDateTime;

          // Wait for the shebang to complete
          var changes = await mapService.GetMapLayerChangesSince(requestDateTime);

          if (changes != null)
          {
            // Pick up the source's last check time
            _lastRequestDateTime = changes.SourceDateTime;

            NotifyMapLayerChanges(changes);
          }
        }
        catch 
        { 
          // In case the server has died on us
        }
        finally
        {
          // If we are still ok to continue
          if (IsAuthenticated)
          {
            StartTimer();
          }
          _trackChanges = true;
        }
      }
    }

    /// <summary>
    /// Notify the changes to the outside world
    /// </summary>
    private void NotifyMapLayerChanges(MapLayerChangeCollection changes)
    {
      if (changes != null)
      {
        var changeMessage = new LiteMapLayerChangeMessage(this, changes);
        Messenger.Send(changeMessage);
      }
    }
    #endregion
  }
}
