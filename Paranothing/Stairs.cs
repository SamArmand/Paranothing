using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Stairs : IDrawable, ICollideable, IUpdatable, IInteractable
{
    internal readonly Direction Direction = Direction.Left;

    readonly bool _startIntact = true;
    readonly GameController _gameController = GameController.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("stairs");
    readonly Vector2 _position;

    internal Stairs(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndStairs", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("direction:", StringComparison.Ordinal))
                Direction = line[10..].Trim() == "Right" ? Direction.Right : Direction.Left;

            else if (line.StartsWith("intact:", StringComparison.Ordinal))
                _ = bool.TryParse(line[7..], out _startIntact);
        }
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, 146, 112);

    public bool IsSolid { get; private set; } = true;

    internal Vector2 Position => _position;

    internal Rectangle SmallBounds =>
        new((int)_position.X + (Direction is Direction.Left ? 0 : 24),
            (int)_position.Y + 22, 122, 190);

    public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_spriteSheet.Image, _position,
        _spriteSheet.GetSprite(IsSolid ? 0 : 1), tint, 0f,
        new(), 1f,
        Direction == Direction.Left
            ? SpriteEffects.FlipHorizontally
            : SpriteEffects.None, DrawLayer.Stairs);

    public void Interact() => _gameController.Bruce.Climb(this);

    public void Update(GameTime time) => IsSolid = _gameController.TimePeriod == TimePeriod.Past || _startIntact;
}