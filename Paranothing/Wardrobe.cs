using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Wardrobe : ICollideable, IUpdatable, IDrawable, IInteractable, IResetable
{
    static readonly Dictionary<string, Wardrobe> Wardrobes = new();

    internal WardrobeState State;

    readonly bool _startLocked;
    readonly GameController _gameController = GameController.Instance;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("wardrobe");
    readonly string _keyName = string.Empty;
    readonly string _linkedName;
    readonly Vector2 _startingPosition;

    int _frame;
    int _frameLength;
    int _frameTime;

    List<int> _animationFrames;
    string _animationName;
    Vector2 _positionPresent, _positionPast, _positionFarPast;

    internal Wardrobe(string saveString)
    {
        var x = 0;
        var y = 0;
        var name = "WR";
        var link = "WR";
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;

        while (!line.StartsWith("EndWardrobe", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = int.TryParse(line[2..], out x);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = int.TryParse(line[2..], out y);

            else if (line.StartsWith("name:", StringComparison.Ordinal)) name = line[5..].Trim();

            else if (line.StartsWith("locked:", StringComparison.Ordinal))
                _ = bool.TryParse(line[7..], out _startLocked);

            if (line.StartsWith("link:", StringComparison.Ordinal)) link = line[5..].Trim();
            if (line.StartsWith("keyName:", StringComparison.Ordinal)) _keyName = line[8..].Trim();
        }

        IsLocked = _startLocked;

        if (IsLocked)
        {
            AnimationName = "wardrobe_closed";
            State = WardrobeState.Closed;
        }
        else
        {
            AnimationName = "wardrobe_opening";
            State = WardrobeState.Open;
        }

        _startingPosition = new(x, y);
        _positionPresent = new(x, y);
        _positionPast = new(x, y);
        _positionFarPast = new(x, y);
        if (Wardrobes.ContainsKey(name))
            Wardrobes.Remove(name);
        Wardrobes.Add(name, this);
        _linkedName = link;
    }

    internal enum WardrobeState
    {
        Closed,
        Opening,
        Open
    }

    public Rectangle Bounds => new((int)Position.X + 2, (int)Position.Y + 2, 65, 78);

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

    internal int X
    {
        set
        {
            switch (_gameController.TimePeriod)
            {
                case TimePeriod.FarPast:
                    _positionFarPast.X = value;
                    _positionPast.X = value;
                    _positionPresent.X = value;
                    break;
                case TimePeriod.Past:
                    _positionPast.X = value;
                    _positionPresent.X = value;
                    break;
                case TimePeriod.Present:
                    _positionPresent.X = value;
                    break;
            }
        }
    }

    internal Rectangle EnterBox => new((int)Position.X + 24, (int)Position.Y + 9, 23, 73);

    internal Wardrobe LinkedWardrobe => Wardrobes.TryGetValue(_linkedName, out var w) ? w : null;

    internal bool IsLocked { get; private set; }

    string AnimationName
    {
        set
        {
            if (!_spriteSheet.HasAnimation(value) || _animationName == value) return;

            _animationName = value;
            _animationFrames = _spriteSheet.GetAnimation(_animationName);
            _frame = 0;
            _frameTime = 0;
        }
    }

    public void Move(float movement)
    {
        switch (_gameController.TimePeriod)
        {
            case TimePeriod.FarPast:
                _positionFarPast.X += movement;
                _positionPast.X = _positionFarPast.X;
                _positionPresent.X = _positionFarPast.X;
                break;
            case TimePeriod.Past:
                _positionPast.X += movement;
                _positionPresent.X = _positionPast.X;
                break;
            case TimePeriod.Present:
                _positionPresent.X += movement;
                break;
        }
    }

    public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_spriteSheet.Image, Position,
        _spriteSheet
            .GetSprite(_animationFrames
                .ElementAt(_frame)), tint,
        0f, new(), 1f, SpriteEffects.None,
        DrawLayer.Wardrobe);

    public void Interact()
    {
        if (_gameController.Bruce.Bounds.Intersects(EnterBox))
        {
            if (!IsLocked && LinkedWardrobe?.IsLocked == false &&
                !_gameController.CollidingWithSolid(LinkedWardrobe.EnterBox))
                _gameController.Bruce.Teleport(this);
        }
        else
            _gameController.Bruce.Push(this);
    }

    public void Reset()
    {
        _positionPresent = new(_startingPosition.X, _startingPosition.Y);
        _positionPast = new(_startingPosition.X, _startingPosition.Y);
        _positionFarPast = new(_startingPosition.X, _startingPosition.Y);
        IsLocked = _startLocked;

        if (_startLocked)
        {
            AnimationName = "wardrobe_closed";
            State = WardrobeState.Closed;
        }
        else
        {
            AnimationName = "wardrobe_opening";
            State = WardrobeState.Open;
        }
    }

    public void Update(GameTime time)
    {
        _frameTime += time.ElapsedGameTime.Milliseconds;

        if (!string.IsNullOrEmpty(_keyName) && DoorKey.GetKey(_keyName)?.PickedUp == true &&
            State == WardrobeState.Closed)
            State = WardrobeState.Opening;

        switch (State)
        {
            case WardrobeState.Open:
                AnimationName = "wardrobe_open";
                break;
            case WardrobeState.Opening:
                _frameLength = 100;
                if (_frame == 2)
                {
                    AnimationName = "wardrobe_open";
                    State = WardrobeState.Open;
                    UnlockObject();
                }
                else
                    AnimationName = "wardrobe_opening";

                break;
            case WardrobeState.Closed:
                AnimationName = "wardrobe_closed";
                break;
        }

        if (_frameTime < _frameLength) return;

        _frameTime = 0;
        _frame = (_frame + 1) % _animationFrames.Count;
    }

    void UnlockObject()
    {
        IsLocked = false;

        _soundManager.PlaySound("Wardrobe Unlock");
    }
}