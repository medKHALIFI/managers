using System;
using SpatialEye.Framework.Client;
using SpatialEye.Framework.Features;
using SpatialEye.Framework.Geometry;

namespace Lite
{
  /// <summary>
  /// Request for jumping to a specified world; making it the active map
  /// </summary>
  public class LiteGoToWorldRequestMessage : MessageBase
  {
    /// <summary>
    /// Constructs the request for the specified world and owner
    /// </summary>
    /// <param name="sender">The originator of the request</param>
    /// <param name="world">The world to jump to</param>
    /// <param name="owner">The owning feature of the request</param>
    public LiteGoToWorldRequestMessage(Object sender, World world, Feature owner = null)
      : base(sender)
    {
      World = world;
      Owner = owner;
      if (owner != null)
      {
        var table = owner.TableDescriptor;
        if (table != null)
        {
          Description = string.Format("{0} {1}", table.ExternalName, owner.Description);
        }
        else
        {
          Description = owner.Description;
        }
      }
      else
      {
        Description = string.Format("{1} - {0}", World.Universe.Name, World.WorldId.ToString());
      }
    }

    /// <summary>
    /// The description of (the owner of) the world
    /// </summary>
    public string Description
    {
      get;
      private set;
    }

    /// <summary>
    /// The owner of the world to jump to
    /// </summary>
    public Feature Owner
    {
      get;
      private set;
    }

    /// <summary>
    /// The world to jump to
    /// </summary>
    public World World
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns a descriptive text of the requset
    /// </summary>
    public override string ToString()
    {
      return Description;
    }
  }
}
