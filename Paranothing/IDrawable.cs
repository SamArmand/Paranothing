using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    /// <summary>
    /// An interface for any object that can be drawn.
    /// The implementing class decides how it handles the drawing, but it must implement the draw() method.
    /// </summary>
    internal interface IDrawable
    {
        /// <summary>
        /// Draw the object
        /// </summary>
        /// <param name="renderer">The spritebatch with which to draw</param>
        /// <param name="tint">A colour to use as a tint</param>
        void Draw(SpriteBatch renderer, Color tint);
    }
}
