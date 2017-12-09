using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal sealed class Button : IDrawable, ICollideable, ISaveable, IInteractable
    {
        # region Attributes

        private static readonly Dictionary<string, Button> ButtonsDict = new Dictionary<string, Button>();

        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        //Collideable
        private Vector2 _position;
        private Rectangle Bounds => new Rectangle(X, Y, 16, 5);

        //Drawable
        private readonly SpriteSheet _sheet;
        public bool StepOn;

        # endregion

        # region Constructors

        public Button(string saveString)
        {
            _sheet = _sheetMan.GetSheet("button");
            StepOn = false;
            X = 0;
            Y = 0;
            var name = "BT";
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndButton", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("name:", StringComparison.Ordinal)) name = line.Substring(5).Trim();
            }

            if (ButtonsDict.ContainsKey(name))
                ButtonsDict.Remove(name);
            ButtonsDict.Add(name, this);
        }

        # endregion

        # region Methods

        //Accessors & Mutators
        private int X
        {
            get => (int)_position.X;
            set => _position.X = value;
        }

        private int Y
        {
            get => (int)_position.Y;
            set => _position.Y = value;
        }

        //Collideable
        public Rectangle GetBounds()
        {
            return Bounds;
        }

        public bool IsSolid()
        {
            return false;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            renderer.Draw(_sheet.Image, Bounds, StepOn ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                new Vector2(), SpriteEffects.None, DrawLayer.Key);
        }

        //Interactive
        public void Interact()
        {
        }

        public static Button GetKey(string name)
        {
            Button k;
            if (ButtonsDict.ContainsKey(name))
                ButtonsDict.TryGetValue(name, out k);
            else
                k = null;
            return k;
        }

        #endregion

        //reset
        public void Reset()
        {

        }
    }
}
