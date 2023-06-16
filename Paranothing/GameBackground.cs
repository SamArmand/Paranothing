using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

class GameBackground
{
    readonly Texture2D _backgroundTexture;

    protected GameBackground(Texture2D texture, Rectangle rectangle)
    {
        _backgroundTexture = texture;
        BackgoundRectangle = rectangle;
    }

    protected Rectangle BackgoundRectangle { get; }

    protected void Draw(SpriteBatch spriteBatch) =>
        spriteBatch.Draw(_backgroundTexture, BackgoundRectangle, Color.White);
}