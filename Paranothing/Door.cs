using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Door : ICollideable, IUpdatable, IDrawable, IResetable
{
    readonly bool _startLocked;
    readonly GameController _gameController = GameController.Instance;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("door");
    readonly string _keyName;
    readonly Vector2 _position;

    DoorsState _state;
    int _frameTime, _frameLength, _frame;
    List<int> _animationFrames;
    string _animName;

    internal Door(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndDoor", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("locked:", StringComparison.Ordinal))
                _ = bool.TryParse(line[7..], out _startLocked);

            else if (line.StartsWith("keyName:", StringComparison.Ordinal)) _keyName = line[8..].Trim();
        }

        IsLocked = _startLocked;

        if (IsLocked)
        {
            Animation = _gameController.TimePeriod == TimePeriod.Present ? "door_closed_present" : "door_closed_past";
            _state = DoorsState.Closed;
            return;
        }

        Animation = _gameController.TimePeriod == TimePeriod.Present ? "door_opening_present" : "door_opening_past";
        _state = DoorsState.Open;
    }

    enum DoorsState
    {
        Closed,
        Opening,
        Open
    }

    public Rectangle Bounds => new((int)_position.X + 25, (int)_position.Y, 8, 75);

    public bool IsSolid => IsLocked;

    internal bool IsLocked { get; private set; }

    string Animation
    {
        set
        {
            if (!_spriteSheet.HasAnimation(value) || _animName == value) return;

            _animName = value;
            _animationFrames = _spriteSheet.GetAnimation(_animName);
            _frame = 0;
            _frameTime = 0;
        }
    }


    public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_spriteSheet.Image, _position,
        _spriteSheet.GetSprite(_animationFrames.ElementAt(_frame)), tint, 0f, new(), 1f, SpriteEffects.None, DrawLayer.Background);

    public void Reset()
    {
        IsLocked = _startLocked;

        if (IsLocked)
        {
            Animation = _gameController.TimePeriod == TimePeriod.Present ? "door_closed_present" : "door_closed_past";
            _state = DoorsState.Closed;
            return;
        }

        Animation = _gameController.TimePeriod == TimePeriod.Present ? "door_opening_present" : "door_opening_past";
        _state = DoorsState.Open;
    }

    public void Update(GameTime time)
    {
        _frameTime += time.ElapsedGameTime.Milliseconds;
        if (!string.IsNullOrEmpty(_keyName) && DoorKey.GetKey(_keyName)?.PickedUp == true &&
            _state == DoorsState.Closed)
        {
            _state = DoorsState.Opening;
            _frameLength = 100;
            UnlockObj();
        }

        var timePeriod = _gameController.TimePeriod == TimePeriod.Present ? "present" : "past";
        switch (_state)
        {
            case DoorsState.Open:
                Animation = "door_open_" + timePeriod;
                break;
            case DoorsState.Opening:
                if (_frameTime >= _frameLength)
                {
                    Animation = "door_open_" + timePeriod;
                    _state = DoorsState.Open;
                    break;
                }

                Animation = "door_opening_" + timePeriod;

                break;
            case DoorsState.Closed:
                Animation = "door_closed_" + timePeriod;
                break;
        }

        if (_frameTime < _frameLength) return;

        _frameTime = 0;
        _frame = (_frame + 1) % _animationFrames.Count;
    }

    void UnlockObj()
    {
        IsLocked = false;

        _soundManager.PlaySound("Door Unlock");
    }
}