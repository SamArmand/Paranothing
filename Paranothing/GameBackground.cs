using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal class GameBackground
    {
        # region Attributes

        private readonly Texture2D _backgroundTexture;

        # endregion

        # region Constructor

        protected GameBackground(Texture2D inTexture, Rectangle inRect)
        {
            _backgroundTexture = inTexture;
            BackgoundRectangle = inRect;
        }

        # endregion

        # region Methods

        //Accessor
        protected Rectangle BackgoundRectangle { get; }

        //Draw
        protected void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_backgroundTexture, BackgoundRectangle, Color.White);
        }

        # endregion
    }
}
