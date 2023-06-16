using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Wall : IDrawable, ICollideable, IUpdatable
{
    readonly bool _startIntact = true;
    readonly GameController _gameController = GameController.Instance;
    readonly int _width, _height;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("wall");
    readonly Vector2 _position;

    internal Wall(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndWall", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("width:", StringComparison.Ordinal)) _ = int.TryParse(line[6..], out _width);

            else if (line.StartsWith("height:", StringComparison.Ordinal)) _ = int.TryParse(line[7..], out _height);

            else if (line.StartsWith("intact:", StringComparison.Ordinal))
                _ = bool.TryParse(line[7..], out _startIntact);
        }
    }

    public Rectangle Bounds => new(X, Y, _width, _height);

    public bool IsSolid { get; private set; }

    int X => (int)_position.X;

    int Y => (int)_position.Y;

    public void Draw(SpriteBatch spriteBatch, Color tint)
    {
        switch (_gameController.TimePeriod)
        {
            case TimePeriod.Present:
            case TimePeriod.FarPast:
                spriteBatch.Draw(_spriteSheet.Image, Bounds,
                    !IsSolid ? _spriteSheet.GetSprite(1) : _spriteSheet.GetSprite(0), tint, 0f,
                    new(), SpriteEffects.None, DrawLayer.Background - 0.01f);
                break;
            case TimePeriod.Past:
                spriteBatch.Draw(_spriteSheet.Image, Bounds, _spriteSheet.GetSprite(0), tint, 0f, new(),
                    SpriteEffects.None,
                    DrawLayer.Background - 0.01f);
                break;
        }
    }

    public void Update(GameTime time) => IsSolid = _gameController.TimePeriod == TimePeriod.Past || _startIntact;
}