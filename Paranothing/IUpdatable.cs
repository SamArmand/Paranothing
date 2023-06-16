using Microsoft.Xna.Framework;

namespace Paranothing;

/// <summary>
///     An interface for objects that can be updated.
/// </summary>
interface IUpdatable
{
    /// <summary>
    ///     Updates the state of the object as a function of the game's time.
    /// </summary>
    /// <param name="time">The game's time.</param>
    void Update(GameTime time);
}