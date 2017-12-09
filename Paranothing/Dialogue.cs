using System;
using Microsoft.Xna.Framework;

namespace Paranothing
{
    internal sealed class Dialogue : ICollideable, IUpdatable, ISaveable
    {
        private readonly GameController _control = GameController.GetInstance();
        private bool Played { get; set; }
        private readonly string _text;
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
        private Rectangle Bounds => new Rectangle(X, Y, 20, 20);

        public Dialogue(string saveString)
        {
            X = 0;
            Y = 0;
            _text = "...";
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

        public Rectangle GetBounds()
        {
            return Bounds;
        }

        public bool IsSolid()
        {
            return false;
        }

        public void Play()
        {
            if (Played) return;

            _control.ShowDialogue(_text);
            Played = true;
        }

        public void Update(GameTime time) {}

        public void Reset()
        {
            Played = false;
        }
    }
}
