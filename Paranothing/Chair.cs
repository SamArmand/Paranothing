using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Chair : ICollideable, IUpdatable, IDrawable, IResetable
{
    const int MoveLength = 70;
    const int Speed = 3;

    internal ChairState State;

    readonly ActionBubble _actionBubble = new();
    readonly GameController _gameController = GameController.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("chair");
    readonly Vector2 _startingPosition;

    int _moveTime;
    Vector2 _positionPast, _positionFarPast, _positionPresent, _movementDirection;

    internal Chair(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;

        while (!line.StartsWith("EndChair", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _startingPosition.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _startingPosition.Y);
        }

        _positionPresent = _startingPosition;
        _positionPast = _startingPosition;
        _positionFarPast = _startingPosition;
        _actionBubble.Chair = this;
    }

    internal enum ChairState
    {
        Idle,
        Falling,
        Moving
    }

    public Rectangle Bounds => new((int)Position.X, (int)Position.Y, 40, 52);

    public bool IsSolid => false;

    internal Vector2 Position
    {
        get
        {
            return _gameController.TimePeriod switch
            {
                TimePeriod.FarPast => _positionFarPast,
                TimePeriod.Past => _positionPast,
                TimePeriod.Present => _positionPresent,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public void Draw(SpriteBatch renderer, Color tint)
    {
        renderer.Draw(_spriteSheet.Image, Position,
            _gameController.TimePeriod == TimePeriod.Present ? _spriteSheet.GetSprite(1) : _spriteSheet.GetSprite(0),
            tint, 0f,
            new(), 1f, SpriteEffects.None, DrawLayer.Chairs);
        _actionBubble.Draw(renderer, tint);
    }

    public void Reset()
    {
        _positionPresent = new(_startingPosition.X, _startingPosition.Y);
        _positionPast = new(_startingPosition.X, _startingPosition.Y);
        _positionFarPast = new(_startingPosition.X, _startingPosition.Y);
    }

    public void Update(GameTime time)
    {
        switch (State)
        {
            case ChairState.Idle:

                var bruce = _gameController.Bruce;
                var nearestChair = bruce.NearestChair;

                if (nearestChair != null && nearestChair != this)
                {
                    if (nearestChair.State == ChairState.Idle)
                    {
                        var brucePosition = bruce.Position;
                        var brucePositionX = brucePosition.X;
                        var brucePositionY = brucePosition.Y;
                        var nearestChairPosition = nearestChair.Position;

                        if (new Vector2(brucePositionX - Position.X, brucePositionY - Position.Y).LengthSquared() <
                            new Vector2(brucePositionX - nearestChairPosition.X,
                                brucePositionY - nearestChairPosition.Y).LengthSquared())
                        {
                            bruce.NearestChair = this;
                            _actionBubble.IsVisible = true;
                            _actionBubble.SetAction(ActionBubble.BubbleAction.Chair, false);
                        }
                        else
                            _actionBubble.IsVisible = false;
                    }
                }
                else
                {
                    bruce.NearestChair = this;
                    _actionBubble.IsVisible = true;
                    _actionBubble.SetAction(ActionBubble.BubbleAction.Chair, false);
                }

                break;
            case ChairState.Falling:
                _actionBubble.IsVisible = false;
                Move(Vector2.UnitY);
                break;
            case ChairState.Moving:
                _actionBubble.IsVisible = false;
                _moveTime += time.ElapsedGameTime.Milliseconds;
                if (_moveTime >= MoveLength)
                {
                    Move(_movementDirection * Speed);
                    _moveTime = 0;

                    var bounds = Bounds;

                    var smallerBound = new Rectangle((int)Position.X + 2, (int)Position.Y + 2, bounds.Width - 4,
                        bounds.Height - 4);
                    if (_gameController.CollidingWithSolid(smallerBound, false))
                        Move(_movementDirection * -Speed);

                    _movementDirection = Vector2.Zero;
                }

                break;
        }
    }

    internal void Move(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                _movementDirection.Y = -1;
                break;
            case Direction.Down:
                _movementDirection.Y = 1;
                break;
            case Direction.Left:
                _movementDirection.X = -1;
                break;
            case Direction.Right:
                _movementDirection.X = 1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    internal void Move(Vector2 velocity)
    {
        switch (_gameController.TimePeriod)
        {
            case TimePeriod.FarPast:
                _positionFarPast += velocity;
                _positionPast = _positionFarPast;
                _positionPresent = _positionFarPast;
                break;
            case TimePeriod.Past:
                _positionPast += velocity;
                _positionPresent = _positionPast;
                break;
            case TimePeriod.Present:
                _positionPresent += velocity;
                break;
        }
    }
}