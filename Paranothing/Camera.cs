using Microsoft.Xna.Framework;

namespace Paranothing
{
    internal sealed class Camera : IUpdatable
    {
        private readonly GameController _control = GameController.GetInstance();
        public int X, Y;
        public readonly int Width;
        public readonly int Height;
        public readonly float Scale;
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
            var player = _control.Player;
            var level = _control.Level;

            X = (int)player.X - (int)(Width/ Scale / 2);
            Y = (int)player.Y - (int)(Height/ Scale/ 2);

            if (X > level.Width - Width/Scale)
                X = level.Width - (int)(Width/Scale);
            if (Y > level.Height - Height/Scale)
                Y = level.Height - (int)(Height/Scale);
            if (X < 0)
                X = 0;
            if (Y < 0 || level.Height < Height/Scale)
                Y = 0;
            if (Height/Scale > level.Height)
                Y = -(int)((Height/Scale - level.Height) / 2);
        }
    }
}
