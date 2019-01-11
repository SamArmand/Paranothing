using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Portrait : IDrawable, ICollideable, IInteractable, ISaveable
    {
        readonly GameController _control = GameController.GetInstance();
        readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        readonly SoundManager _soundMan = SoundManager.Instance();
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

        readonly SpriteSheet _sheet;
        public bool WasMoved { get; }
        public Vector2 MovedPos;
        public TimePeriod InTime { get; }
        public TimePeriod SendTime { get; }

        public Portrait(string saveString, string str)
        {
            _sheet = _sheetMan.GetSheet("portrait");
            SendTime = TimePeriod.Past;
            WasMoved = false;
            ParseString(saveString, str);
        }

        //present past constructor
        public Portrait(string saveString, TimePeriod period)
        {
            _sheet = _sheetMan.GetSheet("portrait");
            WasMoved = false;
            switch (period)
            {
                case TimePeriod.Present:
                    ParseString(saveString, "EndPresentPortrait");
                    WasMoved = true;
                    InTime = TimePeriod.Present;
                    SendTime = TimePeriod.Past;
                    break;
                case TimePeriod.Past:
                    ParseString(saveString, "EndPastPortrait");
                    WasMoved = true;
                    InTime = TimePeriod.Past;
                    SendTime = TimePeriod.Past;
                    break;
                case TimePeriod.FarPast:
                    ParseString(saveString, "EndOldPortrait");
                    SendTime = TimePeriod.FarPast;
                    _sheet = _sheetMan.GetSheet("oldportrait");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(period), period, null);
            }
        }

        void ParseString(string saveString, string str)
        {
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            X = 0;
            Y = 0;
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith(str, StringComparison.Ordinal) && lineNum < lines.Length)
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

        public void Reset(){}

        public void Draw(SpriteBatch renderer, Color tint)
        {
            if ((!WasMoved || _control.TimePeriod == InTime) && !(_control.TimePeriod == TimePeriod.FarPast && SendTime != TimePeriod.FarPast))
                renderer.Draw(_sheet.Image, _position,
                    _control.TimePeriod == TimePeriod.Present ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                    new Vector2(), 1f, SpriteEffects.None, DrawLayer.Background);
        }

        public Rectangle GetBounds() => new Rectangle(X, Y, 35, 30);

        public bool IsSolid() => false;

        public void Interact()
        {
            if (_control.TimePeriod == TimePeriod.FarPast && SendTime != TimePeriod.FarPast) return;

            var player = _control.Player;
            player.State = Boy.BoyState.TimeTravel;
            player.X = X;

            if (GameTitle.ToggleSound) _soundMan.PlaySound("Portrait Travel");
        }
    }
}
