using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Stairs : IDrawable, ICollideable, IUpdatable, IInteractable, ISaveable
    {
        readonly GameController _control = GameController.GetInstance();
        readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        readonly SpriteSheet _sheet;
        Vector2 _position;
        public int X
        {
            get => (int)_position.X;
            private set => _position.X = value;
        }
        public int Y
        {
            get => (int)_position.Y;
            private set => _position.Y = value;
        }

        readonly bool _startIntact;
        bool _intact;
        public readonly Direction Direction;

        public Stairs(string saveString)
        {
            _sheet = _sheetMan.GetSheet("stair");
            X = 0;
            Y = 0;
            Direction = Direction.Left;
            _intact = true;
            _startIntact = true;
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndStair", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("direction:", StringComparison.Ordinal))
                {
                    var dir = line.Substring(10).Trim();
                    Direction = dir == "Right" ? Direction.Right : Direction.Left;
                }

                if (!line.StartsWith("intact:", StringComparison.Ordinal)) continue;

                try { _startIntact = bool.Parse(line.Substring(7)); }
                catch (FormatException) { }
            }
        }

        public void Reset() { }

        public void Update(GameTime time) => _intact = _control.TimePeriod == TimePeriod.Past || _startIntact;

        public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, _position, _sheet.GetSprite(_intact ? 0 : 1), tint, 0f, new Vector2(), 1f, Direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, DrawLayer.Stairs);

        public bool IsSolid() => _intact;

        public Rectangle GetBounds() => new Rectangle((int)_position.X, (int)_position.Y, 146, 112);

        public Rectangle GetSmallBounds() => new Rectangle((int)_position.X + (Direction == Direction.Left ? 0 : 24), (int)_position.Y + 22, 122, 190);

        public void Interact()
        {
            var player = _control.Player;
            player.State = Direction == Direction.Left ? Boy.BoyState.StairsLeft : Boy.BoyState.StairsRight;
            if (player.X + 30 >= X && player.X + 8 <= X)
            {
                player.Direction = Direction.Right;
                player.X = X - 14;
            }
            else if (player.X + 30 >= X + GetBounds().Width && player.X + 8 <= X + GetBounds().Width)
            {
                player.Direction = Direction.Left;
                player.X = X + GetSmallBounds().Width;
            }
        }
    }
}
