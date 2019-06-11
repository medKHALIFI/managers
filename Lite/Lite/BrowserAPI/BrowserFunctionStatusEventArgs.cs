using System;
using System.Net;
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
  /// The typed event arguments for browser function status changes
  /// </summary>
  public class BrowserFunctionStatusEventArgs : EventArgs
  {
    /// <summary>
    /// The constructor for the event arguments
    /// </summary>
    public BrowserFunctionStatusEventArgs(Boolean succes)
    {
      Succes = succes;
    }

    /// <summary>
    /// Indicates whether the function succeeded
    /// </summary>
    public Boolean Succes
    {
      get;
      private set;
    }
  }
}
