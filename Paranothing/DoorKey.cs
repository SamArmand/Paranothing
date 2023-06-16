using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class DoorKey : IDrawable, ICollideable, IResetable
{
    static readonly Dictionary<string, DoorKey> DoorKeys = new();

    readonly GameController _gameController = GameController.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("key");
    readonly Vector2 _position;

    internal DoorKey(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndKey", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("restrictTime:", StringComparison.Ordinal))
            {
                RestrictTime = true;
                var t = line[13..].Trim();
                switch (t)
                {
                    case "Present":
                        InTime = TimePeriod.Present;
                        break;
                    case "Past":
                        InTime = TimePeriod.Past;
                        break;
                    case "FarPast":
                        InTime = TimePeriod.FarPast;
                        break;
                    default:
                        RestrictTime = false;
                        break;
                }
            }

            if (line.StartsWith("name:", StringComparison.Ordinal)) Name = line[5..].Trim();
        }

        DoorKeys[Name] = this;
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, 16, 9);

    public bool IsSolid => false;

    internal bool RestrictTime { get; }
    internal TimePeriod InTime { get; } = TimePeriod.Present;
    internal bool PickedUp { get; set; }

    internal string Name { get; } = "Key";

    internal static DoorKey GetKey(string name) => DoorKeys.TryGetValue(name, out var doorKey) ? doorKey : null;

    public void Draw(SpriteBatch spriteBatch, Color tint)
    {
        var timePeriod = _gameController.TimePeriod;

        if (PickedUp || RestrictTime && timePeriod != InTime) return;

        spriteBatch.Draw(_spriteSheet.Image, Bounds,
            timePeriod == TimePeriod.Present ? _spriteSheet.GetSprite(1) : _spriteSheet.GetSprite(0), tint,
            0f,
            new(), SpriteEffects.None, DrawLayer.Key);
    }

    public void Reset() => PickedUp = false;
}