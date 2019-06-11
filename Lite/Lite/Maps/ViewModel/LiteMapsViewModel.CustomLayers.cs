using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Lite
{
  /// <summary>
  /// Extra behavior for the LiteMapsViewModel for setting up custom layers 
  /// for the specified maps
  /// </summary>
  public partial class LiteMapsViewModel
  {
    /// <summary>
    /// Adds a client-side custom layer to the map (viewModel)
    /// </summary>
    private Task AddCustomLayers(ObservableCollection<LiteMapViewModel> maps)
    {
      // For now - there are no custom layers to be set up; return a default Task.
      // The easiest way is a cached Task-Result (ie bool).
      return TaskFunctions.FromResult(true);
    }
  }
}
