using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Floor : IDrawable, ICollideable, ISaveable
    {
        readonly GameController _control = GameController.GetInstance();
        readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        Vector2 _position;

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

        readonly int _width;
        readonly int _height;

        Rectangle Box => new Rectangle(X, Y, _width, _height);
        readonly SpriteSheet _sheet;

        public Floor(string saveString)
        {
            _sheet = _sheetMan.GetSheet("floor");
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            X = 0;
            Y = 0;
            _width = 0;
            _height = 0;
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndFloor", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("width:", StringComparison.Ordinal))
                    try { _width = int.Parse(line.Substring(6)); }
                    catch (FormatException) { }

                if (!line.StartsWith("height:", StringComparison.Ordinal)) continue;

                try { _height = int.Parse(line.Substring(7)); }
                catch (FormatException) { }
            }
        }

        public void Reset() { }

        public Rectangle GetBounds() => Box;

        public bool IsSolid() => true;

        public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, Box,
                _control.TimePeriod == TimePeriod.Present ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                new Vector2(), SpriteEffects.None, DrawLayer.Floor);
    }
}
