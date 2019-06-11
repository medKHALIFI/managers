using System.Windows;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Geometry.CoordinateSystems;
using SpatialEye.Framework.Geometry;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Maps;
using System.Linq;
using System.Globalization;



namespace Lite
{   
  public class LiteFactibilidadViewModel : LiteMapViewModel
  {
   
    #region Constructor
        public LiteFactibilidadViewModel(Messenger messenger, MapDefinition definition, MapInteractionHandler interactionHandler, EpsgCoordinateSystemReferenceCollection epsgCSs, World world = null, Envelope envelope = null, Feature owner = null)
      : base(messenger, definition, false, interactionHandler, epsgCSs, world, envelope, owner)
    {

       }
    #endregion
   
  }
}
