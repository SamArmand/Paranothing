using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

/// <summary>
///     An interface for any object that can be drawn.
///     The implementing class decides how it handles the drawing, but it must implement the Draw() method.
/// </summary>
interface IDrawable
{
    /// <summary>
    ///     Draws the game object.
    /// </summary>
    /// <param name="spriteBatch">The sprite batch with which to draw the object.</param>
    /// <param name="tint">A colour to use as a tint</param>
    void Draw(SpriteBatch spriteBatch, Color tint);
}