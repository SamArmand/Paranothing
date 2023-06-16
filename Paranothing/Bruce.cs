using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing;

sealed class Bruce : IDrawable, IUpdatable, ICollideable
{
    const int FrameLength = 60;
    const int Height = 58;

    internal BruceState State = BruceState.Idle;
    internal IInteractable Interactor;

    readonly GameController _gameController = GameController.Instance;
    readonly SoundManager _soundManager = SoundManager.Instance;
    readonly SpriteSheet _spriteSheet = SpriteSheetManager.Instance.GetSheet("bruce");

    float _drawLayer = DrawLayer.Player,
        _moveSpeedX,
        _moveSpeedY;
    int _frame, _frameTime;
    List<int> _animationFrames;
    string _animationName;
    TimePeriod _timeTravelTo;
    Vector2 _teleportTo, _position;

    internal Bruce()
    {
        Animation = "stand";
        ActionBubble = new()
        {
            Bruce = this,
            IsVisible = true
        };
    }

    internal enum BruceState
    {
        Idle,
        Walk,
        StairsLeft,
        StairsRight,
        PushWalk,
        PushingStill,
        Teleport,
        TimeTravel,
        ControllingChair,
        Die
    }

    internal static int Width => 38;

    public Rectangle Bounds => new((int)_position.X, (int)_position.Y, Width, Height);

    public bool IsSolid => true;

    internal Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    internal Direction Direction { get; set; } = Direction.Right;
    internal ActionBubble ActionBubble { get; }
    internal Chair NearestChair { get; set; }

    string Animation
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

    public void Push(Wardrobe wardrobe)
    {
        State = _gameController.CollidingWithSolid(Bounds, false)
            ? BruceState.PushingStill
            : BruceState.PushWalk;

        var wardrobePositionX = wardrobe.Position.X;

        if (_position.X > wardrobePositionX)
        {
            _position.X = wardrobePositionX + 67;
            Direction = Direction.Left;
        }
        else
        {
            _position.X = wardrobePositionX - 36;
            Direction = Direction.Right;
        }
    }

    public void StandOn(Floor floor)
    {
        while (Bounds.Intersects(floor.Bounds)) --_position.Y;
    }

    public void Teleport(Wardrobe wardrobe)
    {
        State = BruceState.Teleport;
        _position.X = wardrobe.Position.X + 16;

        _soundManager.PlaySound("Wardrobe TimeTravel", false, true);
    }

    public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_spriteSheet.Image, _position,
        _spriteSheet.GetSprite(_animationFrames.ElementAt(_frame)), tint, 0f, new(), 1f,
        Direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, _drawLayer);

    public void Update(GameTime time)
    {
        var elapsed = time.ElapsedGameTime.Milliseconds;
        _frameTime += elapsed;

        if (State != BruceState.Die)
        {
            var keyboardState = Keyboard.GetState();
            var gamePadState = GamePad.GetState(PlayerIndex.One);

            if (keyboardState.IsKeyDown(Keys.Space) || gamePadState.IsButtonDown(Buttons.A))
            {
                if (State is BruceState.Walk or BruceState.Idle or BruceState.PushWalk &&
                    keyboardState.IsKeyUp(Keys.Right) &&
                    keyboardState.IsKeyUp(Keys.Left)
                    && gamePadState.IsButtonUp(Buttons.LeftThumbstickRight) &&
                    gamePadState.IsButtonUp(Buttons.LeftThumbstickLeft))
                    Interactor?.Interact();
            }
            else if (State is BruceState.PushingStill or BruceState.PushWalk && Interactor != null) State = BruceState.Idle;

            if ((keyboardState.IsKeyDown(Keys.LeftShift) || gamePadState.IsButtonDown(Buttons.RightTrigger)) &&
                State is BruceState.Idle or BruceState.ControllingChair or BruceState.Walk)
            {
                if (NearestChair != null)
                {
                    State = BruceState.ControllingChair;
                    NearestChair.State = Chair.ChairState.Moving;
                }
            }
            else if (NearestChair is { State: Chair.ChairState.Moving })
            {
                NearestChair.State = Chair.ChairState.Falling;
                State = BruceState.Idle;
            }

            if (keyboardState.IsKeyUp(Keys.Left) && keyboardState.IsKeyUp(Keys.Right)
                                                 && gamePadState.IsButtonUp(Buttons.LeftThumbstickLeft) &&
                                                 gamePadState.IsButtonUp(Buttons.LeftThumbstickRight)
                                                 && State != BruceState.Teleport && State != BruceState.TimeTravel && State != BruceState.ControllingChair && State != BruceState.StairsRight &&
                                                 State != BruceState.StairsLeft)
                State = State is BruceState.PushWalk or BruceState.PushingStill &&
                        Interactor != null
                    ? BruceState.PushingStill
                    : BruceState.Idle;

            if (State != BruceState.ControllingChair)
            {
                if (keyboardState.IsKeyDown(Keys.Right) ||
                    gamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
                {
                    if (State != BruceState.PushWalk && State != BruceState.PushingStill ||
                        Interactor != null && ((Wardrobe)Interactor).Position.X > _position.X)
                        Direction = Direction.Right;
                    State = State switch
                    {
                        BruceState.Idle => BruceState.Walk,
                        BruceState.PushingStill when Direction == Direction.Right && Interactor != null &&
                                                     !_gameController.CollidingWithSolid(((Wardrobe)Interactor).Bounds) =>
                            BruceState
                                .PushWalk,
                        _ => State
                    };
                }
                else if (keyboardState.IsKeyDown(Keys.Left) ||
                         gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                {
                    if (State != BruceState.PushWalk && State != BruceState.PushingStill ||
                        Interactor != null && ((Wardrobe)Interactor).Position.X < _position.X)
                        Direction = Direction.Left;
                    State = State switch
                    {
                        BruceState.Idle => BruceState.Walk,
                        BruceState.PushingStill when Direction == Direction.Left && Interactor != null &&
                                                     !_gameController.CollidingWithSolid(((Wardrobe)Interactor).Bounds) =>
                            BruceState
                                .PushWalk,
                        _ => State
                    };
                }
            }
            else if (NearestChair is { State: Chair.ChairState.Moving })
            {
                if (keyboardState.IsKeyDown(Keys.Right) ||
                    gamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
                    NearestChair.Move(Direction.Right);
                else if (keyboardState.IsKeyDown(Keys.Left) ||
                         gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                    NearestChair.Move(Direction.Left);

                if (keyboardState.IsKeyDown(Keys.Up) ||
                    gamePadState.IsButtonDown(Buttons.LeftThumbstickUp))
                    NearestChair.Move(Direction.Up);
                else if (keyboardState.IsKeyDown(Keys.Down) ||
                         gamePadState.IsButtonDown(Buttons.LeftThumbstickDown))
                    NearestChair.Move(Direction.Down);
            }
            else
                State = BruceState.Idle;
        }

        _drawLayer = DrawLayer.Player;

        if (State == BruceState.PushWalk)
            _soundManager.PlaySound("Pushing Wardrobe", true);
        else
            _soundManager.StopSound("Pushing Wardrobe");


        switch (State)
        {
            case BruceState.Idle:
                if (_animationName is "push_still" or "start_push" or "push") Animation = "end_push";

                if (_animationName is "control" or "control_start") Animation = "control_end";

                if (_animationName == "end_push" || _animationName == "control_end" && _frame == 2 ||
                    _animationName == "walk")
                    Animation = "stand";

                _moveSpeedX = 0;
                _moveSpeedY = 0;
                break;
            case BruceState.Walk:
                Animation = "walk";
                _moveSpeedX = 3;
                _moveSpeedY = 0;
                break;
            case BruceState.StairsLeft:
                Animation = "walk";
                _moveSpeedX = 3;
                _moveSpeedY = 2;
                _drawLayer = DrawLayer.PlayerBehindStairs;
                break;
            case BruceState.StairsRight:
                Animation = "walk";
                _moveSpeedX = 3;
                _moveSpeedY = -2;
                _drawLayer = DrawLayer.PlayerBehindStairs;
                break;
            case BruceState.PushingStill:
                _moveSpeedX = 0;
                _moveSpeedY = 0;
                if (_animationName is "walk" or "stand")
                    Animation = "start_push";
                if (_animationName == "start_push" && _frame == 3 || _animationName == "push")
                    Animation = "push_still";
                break;
            case BruceState.PushWalk:
                _moveSpeedY = 0;
                if (_animationName is "walk" or "stand")
                {
                    _moveSpeedX = 0;
                    Animation = "start_push";
                }

                if (_animationName == "start_push" && _frame == 3 || _animationName == "push_still") Animation = "push";

                if (_animationName == "push") _moveSpeedX = 3;

                break;

            case BruceState.Teleport:
                _moveSpeedX = 0;
                _moveSpeedY = 0;
                if (_animationName is "walk" or "stand")
                {
                    Animation = "enter_wardrobe";
                    var targetWardrobe = ((Wardrobe)Interactor)?.LinkedWardrobe;
                    if (targetWardrobe != null)
                    {
                        var (x, y, _, _) = targetWardrobe.Bounds;
                        _teleportTo = new(x + 16, y + 24);
                        Interactor = null;
                    }
                }

                if (_animationName == "enter_wardrobe" && _frame == 6)
                {
                    _position = _teleportTo;
                    Animation = "leave_wardrobe";
                }

                if (_animationName == "leave_wardrobe" && _frame == 7)
                {
                    Animation = "stand";
                    State = BruceState.Idle;
                }

                break;
            case BruceState.TimeTravel:
                _moveSpeedX = 0;
                _moveSpeedY = 0;
                if (_animationName is "walk" or "stand")
                {
                    var portrait = (Portrait)Interactor;
                    if (portrait != null)
                    {
                        _teleportTo = portrait.WasMoved ? portrait.MovedPosition : new();
                        Animation = "enter_portrait";
                        if (_gameController.TimePeriod == TimePeriod.Present)
                            _timeTravelTo = portrait.SendTime;
                    }

                    Interactor = null;
                }

                if (_animationName == "enter_portrait" && _frame == 7)
                {
                    _gameController.TimePeriod = _gameController.TimePeriod != TimePeriod.Present
                        ? TimePeriod.Present
                        : _timeTravelTo;
                    Animation = "leave_portrait";
                    if (Math.Abs(_teleportTo.X) > 0 && Math.Abs(_teleportTo.Y) > 0) _position = _teleportTo;
                }

                if (_animationName == "leave_portrait" && _frame == 7)
                {
                    Animation = "stand";
                    State = BruceState.Idle;
                }

                break;
            case BruceState.ControllingChair:
                _moveSpeedX = 0;
                _moveSpeedY = 0;
                if (_animationName != "control") Animation = "control_start";

                if (_animationName == "control_start" && _frame == 2)
                    Animation = "control";
                break;
            case BruceState.Die:
                _moveSpeedX = 0;
                _moveSpeedY = 0;
                Animation = "disappear";
                if (_frame == 7)
                {
                    Reset();
                    _gameController.ResetLevel();
                }

                break;
        }

        if (_frameTime < FrameLength) return;

        var flip = Direction == Direction.Left ? -1 : 1;

        _position.X += _moveSpeedX * flip;
        if (State == BruceState.PushWalk && _animationName == "push" && Interactor != null)
        {
            var wardrobe = (Wardrobe)Interactor;
            if (!_gameController.CollidingWithSolid(wardrobe.Bounds, false))
                wardrobe.Move(_moveSpeedX * flip);
            else
            {
                _position.X -= _moveSpeedX * flip;
                State = BruceState.PushingStill;
                return;
            }
        }

        if (Math.Abs(_moveSpeedY) <= 0)
        {
            _moveSpeedY = 1;
            flip = 1;
        }

        _position.Y += _moveSpeedY * flip;
        _frameTime = 0;
        _frame = (_frame + 1) % _animationFrames.Count;
    }

    internal void Climb(Stairs stairs)
    {
        State = stairs.Direction == Direction.Left ? BruceState.StairsLeft : BruceState.StairsRight;

        var stairsPositionX = stairs.Position.X;
        var positionX = _position.X;

        if (positionX + 30 >= stairsPositionX && positionX + 8 <= stairsPositionX)
        {
            Direction = Direction.Right;
            _position.X = stairsPositionX - 14;
        }
        else
        {
            Direction = Direction.Left;
            _position.X = stairsPositionX + stairs.SmallBounds.Width;
        }
    }

    internal void Reset()
    {
        _frame = 0;
        _frameTime = 0;
        State = BruceState.Idle;
        Animation = "stand";
        Direction = Direction.Right;
        ActionBubble.IsVisible = true;
        _teleportTo = Vector2.Zero;
        NearestChair = null;
    }

    internal void TimeTravel(Portrait portrait)
    {
        State = BruceState.TimeTravel;
        _position.X = portrait.X;
    }
}