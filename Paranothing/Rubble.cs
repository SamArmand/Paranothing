using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Rubble : ICollideable, IDrawable
{
    readonly GameController _gameController = GameController.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("rubble");
    readonly Vector2 _position;

    internal Rubble(string saveString)
    {
        var lineNum = 0;
        var line = string.Empty;
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        while (!line.StartsWith("EndRubble", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _position.Y);
        }
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, 37, 28);

    public bool IsSolid => _gameController.TimePeriod == TimePeriod.Present;

    public void Draw(SpriteBatch renderer, Color tint)
    {
        if (_gameController.TimePeriod == TimePeriod.Present)
            renderer.Draw(_spriteSheet.Image, Bounds, _spriteSheet.GetSprite(0), tint, 0f, new(),
                SpriteEffects.None,
                DrawLayer.Rubble);
    }
}