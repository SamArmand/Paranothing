using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Shadow : ICollideable, IUpdatable, IDrawable, IResetable
{
    internal ShadowState State = ShadowState.Walk;

    readonly GameController _gameController = GameController.Instance;
    readonly int _patrolDistance;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("shadow");
    readonly Vector2 _startingPosition;

    Direction _direction;
    int _distanceMoved;
    int _frame;
    int _frameLength;
    int _frameTime;
    int _moveSpeedX, _moveSpeedY;
    List<int> _animationFrames;
    string _animationName;
    Vector2 _position;
    Vector2 _soundPosition;

    internal Vector2 Position => _position;

    internal Vector2 SoundPosition
    {
        set
        {
            _soundPosition = value;
            State = ShadowState.SeekSound;
            if (Animation == "walk")
                Animation = "stop_walk";

            _soundManager.StopSound("Shadow");
            _soundManager.PlaySound("Shadow");
        }
    }

    internal Shadow(string saveString)
    {
        Animation = "walk";
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndShadow", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal))
                _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("patrolDist:", StringComparison.Ordinal))
                _ = int.TryParse(line[11..], out _patrolDistance);
        }

        _patrolDistance = Math.Abs(_patrolDistance);
        _distanceMoved = _patrolDistance;
        _startingPosition = _position;
        _soundPosition = _position;
    }

    internal enum ShadowState
    {
        Idle,
        Walk,
        SeekSound
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y + 7, 32, 74);

    public bool IsSolid => false;

    string Animation
    {
        get => _animationName;
        set
        {
            if (!_spriteSheet.HasAnimation(value) || _animationName == value) return;

            _animationName = value;
            _animationFrames = _spriteSheet.GetAnimation(_animationName);
            _frame = 0;
            _frameTime = 0;
        }
    }

    public void Draw(SpriteBatch renderer, Color tint)
    {
        if (_gameController.TimePeriod == TimePeriod.Present)
            renderer.Draw(_spriteSheet.Image, _position, _spriteSheet.GetSprite(_animationFrames.ElementAt(_frame)),
                tint, 0f,
                new(), 1f,
                _direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                DrawLayer.Player + 0.005f);
    }

    public void Reset()
    {
        _frame = 0;
        _frameTime = 0;
        _position = _startingPosition;
        _soundPosition = _startingPosition;
        _distanceMoved = _patrolDistance;
        State = ShadowState.Walk;
        Animation = "walk";
        _direction = Direction.Right;
    }

    public void Update(GameTime time)
    {
        if (_gameController.TimePeriod != TimePeriod.Present) return;

        _frameTime += time.ElapsedGameTime.Milliseconds;
        switch (State)
        {
            case ShadowState.Idle:
                if (Animation == "walk")
                    Animation = "stop_walk";
                if (Animation == "stop_walk" && _frame == 2)
                    Animation = "stand";
                _moveSpeedX = 0;
                _moveSpeedY = 0;
                _frameLength = 80;
                break;
            case ShadowState.Walk:
                if (_patrolDistance != 0)
                    if (Animation == "stop_walk" && _frame == 2 || Animation == "stand" || Animation == "walk")
                    {
                        _frameLength = 80;
                        Animation = "walk";
                        _moveSpeedX = 3;
                        _moveSpeedY = 0;
                    }
                    else
                        _moveSpeedX = 0;
                else
                    State = ShadowState.Idle;

                break;
            case ShadowState.SeekSound:
                Animation = "walk";
                _moveSpeedX = 3;
                _moveSpeedY = 0;
                var soundPositionX = _soundPosition.X;
                var positionX = _position.X;
                _direction = soundPositionX > positionX ? Direction.Right : Direction.Left;
                if (Math.Abs(soundPositionX - positionX) < 3)
                    State = ShadowState.Idle;
                break;
        }

        if (_frameTime < _frameLength) return;

        var flip = _direction == Direction.Left ? -1 : 1;

        _position.X += _moveSpeedX * flip;
        _position.Y += _moveSpeedY * flip;
        _frameTime = 0;
        _frame = (_frame + 1) % _animationFrames.Count;
        if (State == ShadowState.Walk && _patrolDistance != 0)
        {
            _distanceMoved += _moveSpeedX;
            if (_distanceMoved >= _patrolDistance * 2)
            {
                Animation = "stop_walk";
                _position.X -= (_patrolDistance * 2 - _distanceMoved) * flip;
                _direction = _direction == Direction.Left ? Direction.Right : Direction.Left;
                _distanceMoved = 0;
            }
        }

        if (!_gameController.CollidingWithSolid(Bounds, false)) return;

        switch (State)
        {
            case ShadowState.SeekSound:
                State = ShadowState.Idle;
                _position.X -= _moveSpeedX * flip;
                _position.Y -= _moveSpeedY * flip;
                break;
            case ShadowState.Walk:
            {
                _distanceMoved = _patrolDistance * 2 - _distanceMoved - _moveSpeedX;
                Animation = "stop_walk";
                if (_direction == Direction.Left)
                {
                    _position.X += _moveSpeedX;
                    _direction = Direction.Right;
                }
                else
                {
                    _position.X -= _moveSpeedX;
                    _direction = Direction.Left;
                }

                break;
            }
        }
    }
}