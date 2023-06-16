using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing;

sealed class Bookcase : ICollideable, IUpdatable, IDrawable, IInteractable, IResetable
{
    const int FrameLength = 100;

    internal BookcaseState State;

    readonly GameController _gameController = GameController.Instance;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("bookcase");
    readonly string _button1, _button2;
    readonly Vector2 _position;

    int _frame;
    int _frameTime;
    int _unlockTimer;

    List<int> _animationFrames;
    string _animationName;

    internal Bookcase(string saveString)
    {
        var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var lineNum = 0;
        var line = string.Empty;
        while (!line.StartsWith("EndBookcase", StringComparison.Ordinal) && lineNum < lines.Length)
        {
            line = lines[lineNum++];
            if (line.StartsWith("x:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.X);

            else if (line.StartsWith("y:", StringComparison.Ordinal)) _ = float.TryParse(line[2..], out _position.Y);

            else if (line.StartsWith("button1:", StringComparison.Ordinal)) _button1 = line[8..].Trim();
            else if (line.StartsWith("button2:", StringComparison.Ordinal)) _button2 = line[8..].Trim();
        }

        Animation = "bookcase_opening";
        State = BookcaseState.Open;
    }

    public enum BookcaseState
    {
        Closed,
        Closing,
        Opening,
        Open
    }

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, 37, 75);

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

    public void Draw(SpriteBatch spriteBatch, Color tint) => spriteBatch.Draw(_spriteSheet.Image,
        _position,
        _spriteSheet.GetSprite(_animationFrames
            .ElementAt(_frame)),
        tint,
        0f, new(), 1f, SpriteEffects.None,
        DrawLayer.Wardrobe);

    public void Interact() => ParanothingGame.EndGame |= State == BookcaseState.Open;

    public void Reset()
    {
        if (_gameController.NextLevel)
            _gameController.InitLevel(true);
    }

    public void Update(GameTime time)
    {
        var elapsed = time.ElapsedGameTime.Milliseconds;
        _frameTime += elapsed;
        if (State == BookcaseState.Opening)
            _unlockTimer += elapsed;
        if (_unlockTimer >= 450)
        {
            _soundManager.PlaySound("Final Door Part 2");
            _unlockTimer = 0;
        }

        if ((_button1 == string.Empty || Button.GetKey(_button1)?.StepOn == true) && (_button2 == string.Empty ||
                Button.GetKey(_button2)?.StepOn == true))
        {
            if (State == BookcaseState.Closed)
            {
                State = BookcaseState.Opening;
                if (_unlockTimer == 0) _soundManager.PlaySound("Final Door Part 1");
            }
        }
        else
        {
            _unlockTimer = 0;
            State = State != BookcaseState.Closed ? BookcaseState.Closing : BookcaseState.Closed;
        }

        switch (State)
        {
            case BookcaseState.Open:
                Animation = "bookcase_open";
                break;
            case BookcaseState.Opening:
                if (_frame == 4)
                {
                    Animation = "bookcase_open";
                    State = BookcaseState.Open;
                }
                else
                    Animation = "bookcase_opening";

                break;
            case BookcaseState.Closing:
                _unlockTimer = 0;
                if (Animation == "bookcase_closing" && _frame == 4)
                {
                    Animation = "close";
                    State = BookcaseState.Closed;
                }
                else
                    Animation = "bookcase_closing";

                break;
            case BookcaseState.Closed:
                _unlockTimer = 0;
                Animation = "bookcase_closed";
                break;
        }

        if (_frameTime < FrameLength) return;

        _frameTime = 0;
        _frame = (_frame + 1) % _animationFrames.Count;
        _frame = (_frame + 1) % _animationFrames.Count;
    }
}