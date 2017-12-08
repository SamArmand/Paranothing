using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Wall : IDrawable, ICollideable, Updatable, Saveable
    {
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private Vector2 position;
        public int X { get { return (int)position.X; } set { position.X = value; } }
        public int Y { get { return (int)position.Y; } set { position.Y = value; } }
        public int Width, Height;
        private Rectangle Box
        {
            get { return new Rectangle(X, Y, Width, Height); }
        }
        private SpriteSheet sheet;
        private bool intact;
        private bool startIntact;

        public Wall(int X, int Y, int Width, int Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
            this.sheet = sheetMan.getSheet("wall");
            this.startIntact = true;
        }

        public Wall(int X, int Y, int Width, int Height, bool startIntact)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
            this.sheet = sheetMan.getSheet("wall");
            this.startIntact = startIntact;
        }

        public Wall(string saveString)
        {
            this.sheet = sheetMan.getSheet("wall");
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            startIntact = true;
            string line = "";
            while (!line.StartsWith("EndWall") && lineNum < lines.Length)
            {
                line = lines[lineNum];
                if (line.StartsWith("x:"))
                {
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("y:"))
                {
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("width:"))
                {
                    try { Width = int.Parse(line.Substring(6)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("height:"))
                {
                    try { Height = int.Parse(line.Substring(7)); }
                    catch (FormatException) { }
                }
                if (line.StartsWith("intact:"))
                {
                    try { startIntact = bool.Parse(line.Substring(7)); }
                    catch (FormatException) { }
                }
                lineNum++;
            }
        }

        public void reset() { }

        public Rectangle GetBounds()
        {
            return Box;
        }

        public bool IsSolid()
        {
            return intact;
        }

        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void update(GameTime time)
        {
            if (control.timePeriod == TimePeriod.Past)
                intact = true;
            else
                intact = startIntact;
        }
        public void Draw(SpriteBatch renderer, Color tint)
        {
            if (control.timePeriod == TimePeriod.Present || control.timePeriod == TimePeriod.FarPast)
                if (!intact)
                    renderer.Draw(sheet.image, Box, sheet.getSprite(1), tint, 0f, new Vector2(), SpriteEffects.None, DrawLayer.Background - 0.01f);
                else
                    renderer.Draw(sheet.image, Box, sheet.getSprite(0), tint, 0f, new Vector2(), SpriteEffects.None, DrawLayer.Background - 0.01f);
            else if (control.timePeriod == TimePeriod.Past)
                    renderer.Draw(sheet.image, Box, sheet.getSprite(0), tint, 0f, new Vector2(), SpriteEffects.None, DrawLayer.Background - 0.01f);
        }

        public string saveData()
        {
            return "StartWall\nx:" + X + "\ny:" + Y + "\nwidth:" + Width + "\nheight:" + Height + "\nintact:" + (startIntact? "true" : "false") + "\nEndWall";
        }
    }
}
