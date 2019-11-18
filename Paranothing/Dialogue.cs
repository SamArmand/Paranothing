using System;
using Microsoft.Xna.Framework;

namespace Paranothing
{
    sealed class Dialogue : ICollideable, IUpdatable, ISaveable
    {
        readonly GameController _gameController = GameController.GetInstance();
        bool Played { get; set; }
        readonly string _text = "...";
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

        Rectangle Bounds => new Rectangle(X, Y, 20, 20);

        internal Dialogue(string saveString)
        {
            X = 0;
            Y = 0;
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndDialogue", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (!line.StartsWith("text:", StringComparison.Ordinal)) continue;

                try { _text = line.Substring(5).Trim(); }
                catch (FormatException) { }
            }
        }

        public Rectangle GetBounds() => Bounds;

        public bool IsSolid() => false;

        internal void Play()
        {
            if (Played) return;

            _gameController.ShowDialogue(_text);
            Played = true;
        }

        public void Update(GameTime time) {}

        public void Reset() => Played = false;
    }
}
