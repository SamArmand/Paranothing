using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal sealed class Floor : IDrawable, ICollideable, ISaveable
    {
        private readonly GameController _control = GameController.GetInstance();
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        private Vector2 _position;
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

        private readonly int _width;
        private readonly int _height;

        private Rectangle Box => new Rectangle(X, Y, _width, _height);
        private readonly SpriteSheet _sheet;

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

        public Rectangle GetBounds()
        {
            return Box;
        }

        public bool IsSolid()
        {
            return true;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            renderer.Draw(_sheet.Image, Box,
                _control.TimePeriod == TimePeriod.Present ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                new Vector2(), SpriteEffects.None, DrawLayer.Floor);
        }
    }
}
