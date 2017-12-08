using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    class Button : Drawable, Collideable, Saveable, Interactive
    {
        # region Attributes

        private static Dictionary<string, Button> buttonsDict = new Dictionary<string, Button>();
        private GameController control = GameController.getInstance();
        private SpriteSheetManager sheetMan = SpriteSheetManager.getInstance();
        //Collideable
        private Vector2 position;
        private Rectangle bounds { get { return new Rectangle(X, Y, 16, 5); } }
        //Drawable
        private SpriteSheet sheet;
        public bool stepOn;
        private string name;

        # endregion

        # region Constructors

        public Button(int X, int Y, string name)
        {
            this.sheet = sheetMan.getSheet("button");
            position = new Vector2(X, Y);
            stepOn = false;
            this.name = name;

            if (buttonsDict.ContainsKey(name))
                buttonsDict.Remove(name);
            buttonsDict.Add(name, this);
        }

        public Button(string saveString)
        {
            this.sheet = sheetMan.getSheet("button");
            stepOn = false;
            X = 0;
            Y = 0;
            name = "BT";
            string[] lines = saveString.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int lineNum = 0;
            string line = "";
            while (!line.StartsWith("EndButton") && lineNum < lines.Length)
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
                if (line.StartsWith("name:"))
                {
                    name = line.Substring(5).Trim();
                }
                lineNum++;
            }

            if (buttonsDict.ContainsKey(name))
                buttonsDict.Remove(name);
            buttonsDict.Add(name, this);
        }

        # endregion

        # region Methods

        //Accessors & Mutators
        public int X { get { return (int)position.X; } set { position.X = value; } }
        public int Y { get { return (int)position.Y; } set { position.Y = value; } }

        //Collideable
        public Rectangle getBounds()
        {
            return bounds;
        }

        public bool isSolid()
        {
            return false;
        }

        //Drawable
        public Texture2D getImage()
        {
            return sheet.image;
        }

        public void draw(SpriteBatch renderer, Color tint)
        {
            if (stepOn)
                renderer.Draw(sheet.image, bounds, sheet.getSprite(1), tint, 0f, new Vector2(), SpriteEffects.None, DrawLayer.Key);
            else
                renderer.Draw(sheet.image, bounds, sheet.getSprite(0), tint, 0f, new Vector2(), SpriteEffects.None, DrawLayer.Key);
        }

        //Interactive
        public void Interact()
        {
        }

        public static Button getKey(string name)
        {
            Button k;
            if (buttonsDict.ContainsKey(name))
                buttonsDict.TryGetValue(name, out k);
            else
                k = null;
            return k;
        }

        #endregion

        public string saveData()
        {
            return "StartButtons\nx:" + X + "\ny:" + Y + "\nname:" + name + "\nEndButtons"; 
        }

        //reset
        public void reset()
        {

        }
    }
}
