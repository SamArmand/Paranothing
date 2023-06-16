using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Floor : IDrawable, ICollideable
{
    readonly GameController _gameController = GameController.Instance;
    readonly int _width, _height;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("floor");
    readonly Vector2 _position = Vector2.Zero;

    internal Floor(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndFloor", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("width:", StringComparison.Ordinal)) _ = int.TryParse(line[6..], out _width);

            else if (line.StartsWith("height:", StringComparison.Ordinal)) _ = int.TryParse(line[7..], out _height);
        }
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, _width, _height);

    public bool IsSolid => true;

    public void Draw(SpriteBatch spriteBatch, Color tint) => spriteBatch.Draw(_spriteSheet.Image, Bounds,
        _gameController.TimePeriod == TimePeriod.Present
            ? _spriteSheet.GetSprite(1)
            : _spriteSheet.GetSprite(0), tint, 0f,
        new(), SpriteEffects.None,
        DrawLayer.Floor);
}