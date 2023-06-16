using Microsoft.Xna.Framework;

namespace Paranothing;

sealed class Camera : IUpdatable
{
    readonly GameController _gameController = GameController.Instance;
    Vector2 _position;

    internal float Scale { get; init; }
    internal int Width { get; init; }
    internal int Height { get; init; }

    internal Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public void Update(GameTime time)
    {
        var brucePosition = _gameController.Bruce.Position;

        var heightToScaleRatio = Height / Scale;
        var widthToScaleRatio = Width / Scale;

        _position.X = brucePosition.X - widthToScaleRatio / 2;
        _position.Y = brucePosition.Y - heightToScaleRatio / 2;

        var level = _gameController.Level;

        var levelWidthLimit = level.Width - Width / Scale;

        if (_position.X > levelWidthLimit)
            _position.X = levelWidthLimit;

        var levelHeight = level.Height;
        var levelHeightLimit = levelHeight - heightToScaleRatio;

        if (_position.Y > levelHeightLimit)
            _position.Y = levelHeightLimit;

        if (_position.X < 0)
            _position.X = 0;

        if (_position.Y < 0 || levelHeight < heightToScaleRatio)
            _position.Y = 0;

        if (heightToScaleRatio > levelHeight)
            _position.Y = -((heightToScaleRatio - levelHeight) / 2);
    }
}