using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Stairs : IDrawable, ICollideable, Updatable, IInteractable, Saveable
    {
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SpriteSheet sheet;
        public Vector2 position;
        public int X { get { return (int)position.X; } set { position.X = value; } }
        public int Y { get { return (int)position.Y; } set { position.Y = value; } }
        private bool startIntact;
        private bool intact;
        public Direction direction;
        public Texture2D getImage()
        {
            return sheet.image;
        }

        public Stairs(float X, float Y, Direction direction)
        {
            this.sheet = sheetMan.getSheet("stair");
            position = new Vector2(X, Y);
            intact = true;
            this.direction = direction;
            this.startIntact = true;
        }

        public Stairs(float X, float Y, Direction direction, bool startIntact)
        {
            this.sheet = sheetMan.getSheet("stair");
            position = new Vector2(X, Y);
            intact = true;
            this.direction = direction;
            this.startIntact = startIntact;
        }

        public Stairs(string saveString)
        {
            this.sheet = sheetMan.getSheet("stair");
            X = 0;
            Y = 0;
            direction = Direction.Left;
            intact = true;
            startIntact = true;
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndStair") && lineNum < lines.Length)
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
                if (line.StartsWith("direction:"))
                {
                    string dir = line.Substring(10).Trim();
                    if (dir == "Right")
                        direction = Direction.Right;
                    else
                        direction = Direction.Left;
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

        public void update(GameTime time)
        {
            if (control.timePeriod == TimePeriod.Past)
                intact = true;
            else
                intact = startIntact;
            Boy player = control.player;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            SpriteEffects flip = SpriteEffects.None;
            if (direction == Direction.Left)
                flip = SpriteEffects.FlipHorizontally;
            Rectangle sprite;
            if (intact)
                sprite = sheet.getSprite(0);
            else
                sprite = sheet.getSprite(1);
            renderer.Draw(sheet.image, position, sprite, tint, 0f, new Vector2(), 1f, flip, DrawLayer.Stairs);
        }

        public bool IsSolid()
        {
            return intact;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)position.X, (int)position.Y, 146, 112);
        }

        public Rectangle getSmallBounds()
        {
            return new Rectangle((int)position.X + (direction == Direction.Left ? 0 : 24), (int)position.Y + 22, 122, 190);
        }

        public void Interact()
        {
            Boy player = control.player;
            if (direction == Direction.Left)
                player.state = Boy.BoyState.StairsLeft;
            else
                player.state = Boy.BoyState.StairsRight;
            if (player.X + 30 >= X && player.X + 8 <= X)
            {
                player.direction = Direction.Right;
                player.X = X - 14;
            }
            else if (player.X + 30 >= X + GetBounds().Width && player.X + 8 <= X + GetBounds().Width)
            {
                player.direction = Direction.Left;
                player.X = X + getSmallBounds().Width;
            }
        }

        public string saveData()
        {
            return "StartStair\nx:" + X + "\ny:" + Y + "\ndirection:" + direction + "\nintact:" + (startIntact ? "true" : "false") + "\nEndStair";
        }
    }
}
