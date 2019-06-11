using System;
using SpatialEye.Framework.Client;

namespace Lite
{
  /// <summary>
  /// A message that indicates the current running state that is the result of 
  /// an activity carried out/requested by the sender
  /// </summary>
  public class LiteActionRunningStateMessage : MessageBase
  {
    /// <summary>
    /// Constructor for the runningState message
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="source">The source that is responsible for the running state</param>
    /// <param name="isBusy">Are we busy</param>
    /// <param name="message">The corresponding message</param>
    public LiteActionRunningStateMessage(Object sender, Object source, bool isBusy, string message = "")
      : base(sender)
    {
      this.Source = source;
      this.IsRunning = isBusy;
      this.RunningStateDescription = message;
    }

    /// <summary>
    /// Holds the source of the running state
    /// </summary>
    public Object Source
    {
      get;
      private set;
    }

    /// <summary>
    /// Are we running
    /// </summary>
    public bool IsRunning
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns the running state description
    /// </summary>
    public string RunningStateDescription
    {
      get;
      private set;
    }
  }
}
