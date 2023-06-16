using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Portrait : IDrawable, ICollideable, IInteractable
{
    internal Vector2 MovedPosition;

    readonly GameController _gameController = GameController.Instance;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("portrait");

    Vector2 _position;

    internal Portrait(string saveString, string str) => ParseString(saveString, str);

    internal Portrait(string saveString, TimePeriod period)
    {
        switch (period)
        {
            case TimePeriod.Present:
                ParseString(saveString, "EndPresentPortrait");
                WasMoved = true;
                InTime = TimePeriod.Present;
                SendTime = TimePeriod.Past;
                _spriteSheet = SpriteSheetManager.Instance.GetSheet("portrait");
                break;
            case TimePeriod.Past:
                ParseString(saveString, "EndPastPortrait");
                WasMoved = true;
                InTime = TimePeriod.Past;
                SendTime = TimePeriod.Past;
                _spriteSheet = SpriteSheetManager.Instance.GetSheet("portrait");
                break;
            case TimePeriod.FarPast:
                ParseString(saveString, "EndOldPortrait");
                SendTime = TimePeriod.FarPast;
                _spriteSheet = SpriteSheetManager.Instance.GetSheet("old_portrait");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(period), period, null);
        }
    }

    public Rectangle Bounds => new(X, Y, 35, 30);

    public bool IsSolid => false;

    internal int X => (int)_position.X;

    internal int Y => (int)_position.Y;
    internal bool WasMoved { get; }
    internal TimePeriod InTime { get; }
    internal TimePeriod SendTime { get; } = TimePeriod.Past;

    public void Draw(SpriteBatch renderer, Color tint)
    {
        if ((!WasMoved || _gameController.TimePeriod == InTime) &&
            !(_gameController.TimePeriod == TimePeriod.FarPast && SendTime != TimePeriod.FarPast))
            renderer.Draw(_spriteSheet.Image, _position,
                _gameController.TimePeriod == TimePeriod.Present
                    ? _spriteSheet.GetSprite(1)
                    : _spriteSheet.GetSprite(0), tint, 0f,
                new(), 1f, SpriteEffects.None, DrawLayer.Background);
    }

    public void Interact()
    {
        if (_gameController.TimePeriod == TimePeriod.FarPast && SendTime != TimePeriod.FarPast) return;

        _gameController.Bruce.TimeTravel(this);

        _soundManager.PlaySound("Portrait TimeTravel", false, true);
    }

    void ParseString(string saveString, string str)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith(str, StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _position.Y);
        }
    }
}