using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Button : IDrawable, ICollideable, ISaveable, IInteractable
    {
        # region Attributes

        static readonly Dictionary<string, Button> Buttons = new Dictionary<string, Button>();

        //Collideable
        Vector2 _position;
        Rectangle Bounds => new Rectangle(X, Y, 16, 5);

        //Drawable
        readonly SpriteSheet _sheet = SpriteSheetManager.GetInstance().GetSheet("button");
        internal bool StepOn = false;

        # endregion

        # region Constructors

        internal Button(string saveString)
        {
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

            if (Buttons.ContainsKey(name))
                Buttons.Remove(name);
            Buttons.Add(name, this);
        }

        # endregion

        # region Methods

        //Accessors & Mutators
        int X
        {
            get => (int)_position.X;
            set => _position.X = value;
        }

        int Y
        {
            get => (int)_position.Y;
            set => _position.Y = value;
        }

        //Collideable
        public Rectangle GetBounds() => Bounds;

        public bool IsSolid() => false;

        public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, Bounds, StepOn ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                new Vector2(), SpriteEffects.None, DrawLayer.Key);

        //Interactive
        public void Interact()
        {
        }

        internal static Button GetKey(string name)
        {
            Button button;
            if (Buttons.ContainsKey(name))
                Buttons.TryGetValue(name, out button);
            else
                button = null;
            return button;
        }

        #endregion

        //reset
        public void Reset()
        {
        }
    }
}
