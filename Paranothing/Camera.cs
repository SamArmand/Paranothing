using Microsoft.Xna.Framework;

namespace Paranothing
{
    sealed class Camera : IUpdatable
    {
        readonly GameController _gameController = GameController.GetInstance();
        internal int X, Y;
        internal readonly int Width, Height;
        internal readonly float Scale;

		public Camera(int x, int y, int width, int height, float scale)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Scale = scale;
        }

        public void Update(GameTime time)
        {
            var player = _gameController.Player;

            X = (int)player.X - (int)(Width/ Scale / 2);
            Y = (int)player.Y - (int)(Height/ Scale/ 2);

            var level = _gameController.Level;
            var levelWidth = level.Width;

            if (X > levelWidth - Width/Scale)
                X = levelWidth - (int)(Width/Scale);

            var levelHeight = level.Height;

            if (Y > levelHeight - Height/Scale)
                Y = levelHeight - (int)(Height/Scale);
            if (X < 0)
                X = 0;
            if (Y < 0 || levelHeight < Height/Scale)
                Y = 0;
            if (Height/Scale > levelHeight)
                Y = -(int)((Height/Scale - levelHeight) / 2);
        }
    }
}
