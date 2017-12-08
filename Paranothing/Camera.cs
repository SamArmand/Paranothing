using Microsoft.Xna.Framework;

namespace Paranothing
{
    class Camera : Updatable
    {
        private GameController control = GameController.getInstance();
        public int X, Y, Width, Height;
        public float scale;
        public Camera(int X, int Y, int Width, int Height, float scale)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
            this.scale = scale;
        }

        public void update(GameTime time)
        {
            Boy player = control.player;
            Level level = control.level;

            X = (int)player.X - (int)(Width/ scale / 2);
            Y = (int)player.Y - (int)(Height/ scale/ 2);

            if (X > level.Width - Width/scale)
                X = level.Width - (int)(Width/scale);
            if (Y > level.Height - Height/scale)
                Y = level.Height - (int)(Height/scale);
            if (X < 0)
                X = 0;
            if (Y < 0 || level.Height < Height/scale)
                Y = 0;
            if (Height/scale > level.Height)
                Y = -(int)((Height/scale - level.Height) / 2);
        }
    }
}
