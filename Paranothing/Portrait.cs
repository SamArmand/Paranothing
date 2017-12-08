using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Portrait : IDrawable, ICollideable, IInteractable, Saveable
    {
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        private SoundManager soundMan = SoundManager.getInstance();
        private Vector2 position;
        public int X
        {
            get { return (int)position.X; }
            set { position.X = value; }
        }
        public int Y
        {
            get { return (int)position.Y; }
            set { position.Y = value; }
        }
        private SpriteSheet sheet;
        public bool wasMoved { get; private set; }
        public Vector2 movedPos;
        public TimePeriod inTime { get; private set; }
        public TimePeriod sendTime { get; private set; }

        public Portrait(int x, int y)
        {
            position = new Vector2(x, y);
            this.sheet = sheetMan.getSheet("portrait");
        }

        public Portrait(string saveString, string str)
        {
            this.sheet = sheetMan.getSheet("portrait");
            sendTime = TimePeriod.Past;
            wasMoved = false;
            parseString(saveString, str);
        }

        //present past constructor
        public Portrait(string saveString, TimePeriod period)
        {
            this.sheet = sheetMan.getSheet("portrait");
            wasMoved = false;
            if (period == TimePeriod.Present)
            {
                parseString(saveString, "EndPresentPortrait");
                wasMoved = true;
                inTime = TimePeriod.Present;
                sendTime = TimePeriod.Past;
            }
            else if (period == TimePeriod.Past)
            {
                parseString(saveString, "EndPastPortrait");
                wasMoved = true;
                inTime = TimePeriod.Past;
                sendTime = TimePeriod.Past;
            }
            else if (period == TimePeriod.FarPast)
            {
                parseString(saveString, "EndOldPortrait");
                sendTime = TimePeriod.FarPast;
                sheet = sheetMan.getSheet("oldportrait");
            }
        }
        private void parseString(string saveString, string str)
        {
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            X = 0;
            Y = 0;
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith(str) && lineNum < lines.Length)
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
                lineNum++;
            }
        }

        public void reset(){}

        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            if ((!wasMoved || control.timePeriod == inTime) && !(control.timePeriod == TimePeriod.FarPast && sendTime != TimePeriod.FarPast))
            {
                if (control.timePeriod == TimePeriod.Present)
                    renderer.Draw(sheet.image, position, sheet.getSprite(1), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Background);
                else
                    renderer.Draw(sheet.image, position, sheet.getSprite(0), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Background);
            }
        }

        public Rectangle GetBounds()
        {
            return new Rectangle(X, Y, 35, 30); 
        }

        public bool IsSolid()
        {
            return false;
        }

        public void Interact()
        {
            if (!(control.timePeriod == TimePeriod.FarPast && sendTime != TimePeriod.FarPast))
            {
                Boy player = control.player;
                player.state = Boy.BoyState.TimeTravel;
                player.X = X;

                if (GameTitle.toggleSound)
                {
                    soundMan.playSound("Portrait Travel");
                }
            }
        }

        public string saveData()
        {
            return "StartPortrait\nx:" + X + "\ny:" + Y + "\nEndPortrait";
        }
    }
}
