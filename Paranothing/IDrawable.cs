using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    /// <summary>
    /// An interface for any object that can be drawn.
    /// The implementing class decides how it handles the drawing, but it must implement the draw() method.
    /// </summary>
    interface Drawable
    {
        /// <summary>
        /// Return the object's image
        /// </summary>
        /// <returns>The texture of the object</returns>
        Texture2D getImage();
        /// <summary>
        /// Draw the object
        /// </summary>
        /// <param name="renderer">The spritebatch with which to draw</param>
        /// <param name="tint">A colour to use as a tint</param>
        void draw(SpriteBatch renderer, Color tint);
    }
}
