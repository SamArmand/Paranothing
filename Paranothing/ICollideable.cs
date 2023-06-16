using Microsoft.Xna.Framework;

namespace Paranothing;

/// <summary>
///     An interface for objects that can handle collision.
/// </summary>
interface ICollideable
{
    /// <summary>
    ///     Gets the bounds of the collidable object.
    /// </summary>
    /// <value>A Rectangle representing the bounds of the object.</value>
    Rectangle Bounds { get; }

    /// <summary>
    ///     Determines whether or not the object is solid.
    /// </summary>
    /// <value>Whether or not the object is solid.</value>
    bool IsSolid { get; }
}