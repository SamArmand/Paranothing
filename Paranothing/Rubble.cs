﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal sealed class Rubble : ICollideable, IDrawable, ISaveable
    {
        # region Attributes
        private readonly GameController _control = GameController.GetInstance();
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        //Collideable
        private Vector2 _position;
        //Drawable
        private readonly SpriteSheet _sheet;

        # endregion

        # region Constructor

        public Rubble(string saveString)
        {
            _sheet = _sheetMan.GetSheet("rubble");
            X = 0;
            Y = 0;
            var lineNum = 0;
            var line = "";
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            while (!line.StartsWith("EndRubble", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (!line.StartsWith("y:", StringComparison.Ordinal)) continue;

                try { Y = int.Parse(line.Substring(2)); }
                catch (FormatException) { }
            }
        }

        public void Reset() { }

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
        private Rectangle Bounds => new Rectangle(X, Y, 37, 28);

        //Collideable
        public Rectangle GetBounds()
        {
            return Bounds;
        }

        public bool IsSolid()
        {
            return _control.TimePeriod == TimePeriod.Present;
        }

        //Drawable

        public void Draw(SpriteBatch renderer, Color tint)
        {
            if (_control.TimePeriod == TimePeriod.Present)
                renderer.Draw(_sheet.Image, Bounds, _sheet.GetSprite(0), tint, 0f, new Vector2(), SpriteEffects.None, DrawLayer.Rubble);
        }

        #endregion
    }
}
