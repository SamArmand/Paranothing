using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Paranothing
{
    internal sealed class Boy : IDrawable, IUpdatable, ICollideable
    {
        private readonly GameController _control = GameController.GetInstance();
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        private readonly SoundManager _soundMan = SoundManager.GetInstance();
        private readonly SpriteSheet _sheet;
        private int _pushSoundTimer;
        private int _frame;
        private int _frameLength;
        private int _frameTime;
        private string _animName;
        private List<int> _animFrames;

        private string Animation
        {
            get => _animName;
            set
            {
                if (!_sheet.HasAnimation(value) || _animName == value) return;

                _animName = value;
                _animFrames = _sheet.GetAnimation(_animName);
                _frame = 0;
                _frameTime = 0;
            }
        }

        private float _drawLayer;
        private float _moveSpeedX, _moveSpeedY; // Pixels per animation frame
        private Vector2 _position;
        public int Width;
        private int _height;

        public float X
        {
            get => _position.X;
            set => _position.X = value;
        }
        public float Y
        {
            get => _position.Y;
            set => _position.Y = value;
        }

        public enum BoyState { Idle, Walk, StairsLeft, StairsRight, PushWalk, PushingStill, Teleport, TimeTravel, ControllingChair, Die }
        public BoyState State;
        public Direction Direction;
        public readonly ActionBubble ActionBubble;
        private Vector2 _teleportTo;
        private TimePeriod _timeTravelTo;
        public Chair NearestChair;
        public IInteractable Interactor;

        public Boy(float x, float y, ActionBubble actionBubble)
        {
            _sheet = _sheetMan.GetSheet("boy");
            _frame = 0;
            _frameTime = 0;
            _frameLength = 60;
            _position = new Vector2(x, y);
            Width = 38;
            _height = 58;
            State = BoyState.Idle;
            Animation = "stand";
            Direction = Direction.Right;
            ActionBubble = actionBubble;
            actionBubble.Player = this;
            actionBubble.Show();
            _teleportTo = new Vector2();
            _drawLayer = DrawLayer.Player;
        }

        public void Reset()
        {
            _frame = 0;
            _frameTime = 0;
            _frameLength = 60;
            _position = new Vector2(X, Y);
            Width = 38;
            _height = 58;
            State = BoyState.Idle;
            Animation = "stand";
            Direction = Direction.Right;
            ActionBubble.Player = this;
            ActionBubble.Show();
            _teleportTo = new Vector2();
            NearestChair = null;
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            renderer.Draw(_sheet.Image, _position, _sheet.GetSprite(_animFrames.ElementAt(_frame)), tint, 0f, new Vector2(), 1f, Direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, _drawLayer);
        }

        private void CheckInput(GameController control)
        {
            if (State == BoyState.Die) return;

            if (control.KeyState.IsKeyDown(Keys.Space) || control.PadState.IsButtonDown(Buttons.A) &&
                ((State == BoyState.Walk || State == BoyState.Idle) && control.KeyState.IsKeyUp(Keys.Right) && control.KeyState.IsKeyUp(Keys.Left)
                    && control.PadState.IsButtonUp(Buttons.LeftThumbstickRight) && control.PadState.IsButtonUp(Buttons.LeftThumbstickLeft) || State == BoyState.PushWalk))
                    Interactor?.Interact();

            else if ((State == BoyState.PushingStill || State == BoyState.PushWalk) && Interactor != null)
                State = BoyState.Idle;

            if (control.KeyState.IsKeyDown(Keys.LeftShift) || control.PadState.IsButtonDown(Buttons.RightTrigger) && NearestChair != null)
            {
                State = BoyState.ControllingChair;
                NearestChair.State = Chair.ChairsState.Moving;
            }
            else if (NearestChair != null && NearestChair.State == Chair.ChairsState.Moving)
            {
                NearestChair.State = Chair.ChairsState.Falling;
                State = BoyState.Idle;
            }
            if (control.KeyState.IsKeyUp(Keys.Left) && control.KeyState.IsKeyUp(Keys.Right)
                                                    && control.PadState.IsButtonUp(Buttons.LeftThumbstickLeft) && control.PadState.IsButtonUp(Buttons.LeftThumbstickRight)
                                                    && State != BoyState.Teleport && State != BoyState.TimeTravel)
            {
                if (State != BoyState.ControllingChair)
                    if (Direction == Direction.Right)
                    {
                        if (State != BoyState.StairsRight && State != BoyState.StairsLeft)
                            if ((State == BoyState.PushWalk || State == BoyState.PushingStill) && Interactor != null)
                                State = BoyState.PushingStill;
                            else
                                State = BoyState.Idle;
                    }
                    else if (State != BoyState.StairsRight && State != BoyState.StairsLeft)
                    {
                            if ((State == BoyState.PushWalk || State == BoyState.PushingStill) && Interactor != null)
                                State = BoyState.PushingStill;
                            else
                                State = BoyState.Idle;
                    }
            }
            else
            {
                if (State != BoyState.ControllingChair)
                    if (control.KeyState.IsKeyDown(Keys.Right) || control.PadState.IsButtonDown(Buttons.LeftThumbstickRight))
                    {
                        if ((State != BoyState.PushWalk && State != BoyState.PushingStill) || (Interactor != null && ((Wardrobe)Interactor).X > X))
                            Direction = Direction.Right;
                        if (State == BoyState.Idle)
                            State = BoyState.Walk;
                        if (Interactor != null && State == BoyState.PushingStill && Direction == Direction.Right)
                            State = BoyState.PushWalk;
                    }
                    else if (control.KeyState.IsKeyDown(Keys.Left) || control.PadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                    {
                        if (State != BoyState.PushWalk && State != BoyState.PushingStill || Interactor != null && ((Wardrobe)Interactor).X < X)
                            Direction = Direction.Left;
                        if (State == BoyState.Idle)
                            State = BoyState.Walk;
                        if (Interactor != null && State == BoyState.PushingStill && Direction == Direction.Left)
                            State = BoyState.PushWalk;
                    }
            }

            if (State != BoyState.ControllingChair) return;

            if (NearestChair != null && NearestChair.State == Chair.ChairsState.Moving)
            {
                if (control.KeyState.IsKeyDown(Keys.Right) || control.PadState.IsButtonDown(Buttons.LeftThumbstickRight))
                    NearestChair.Move(Direction.Right);
                else if (control.KeyState.IsKeyDown(Keys.Left) || control.PadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                    NearestChair.Move(Direction.Left);
                if (control.KeyState.IsKeyDown(Keys.Up) || control.PadState.IsButtonDown(Buttons.LeftThumbstickUp))
                    NearestChair.Move(Direction.Up);
                else if (control.KeyState.IsKeyDown(Keys.Down) || control.PadState.IsButtonDown(Buttons.LeftThumbstickDown))
                    NearestChair.Move(Direction.Down);
            }
            else
            {
                State = BoyState.Idle;
            }
        }

        public void Update(GameTime time)
        {
            var elapsed = time.ElapsedGameTime.Milliseconds;
            _frameTime += elapsed;
            CheckInput(_control);
            _drawLayer = DrawLayer.Player;

            switch (State)
            {
                case BoyState.Idle:
                    switch (Animation)
                    {
                        case "pushstill":
                        case "startpush":
                        case "push":
                            Animation = "endpush";
                            break;
                        case "control":
                            Animation = "controlend";
                            break;
                        case "endpush" when _frame == 2:
                        case "controlend" when _frame == 2:
                        case "walk":
                            Animation = "stand";
                            break;
                        default:
                            Animation = Animation;
                            break;
                    }
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    break;
                case BoyState.Walk:
                    Animation = "walk";
                    _moveSpeedX = 3;
                    _moveSpeedY = 0;
                    _control.HideDialogue();
                    break;
                case BoyState.StairsLeft:
                    Animation = "walk";
                    _moveSpeedX = 3;
                    _moveSpeedY = 2;
                    _drawLayer = DrawLayer.PlayerBehindStairs;
                    break;
                case BoyState.StairsRight:
                    Animation = "walk";
                    _moveSpeedX = 3;
                    _moveSpeedY = -2;
                    _drawLayer = DrawLayer.PlayerBehindStairs;
                    break;
                case BoyState.PushingStill:

                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    switch (Animation)
                    {
                        case "walk":
                        case "stand":
                            Animation = "startpush";
                            break;
                        case "startpush" when _frame == 3:
                        case "push":
                            Animation = "pushstill";
                            break;
                        default:
                            Animation = Animation;
                            break;
                    }
                    break;
                case BoyState.PushWalk:
                    _moveSpeedY = 0;
                    switch (Animation)
                    {
                        case "walk":
                        case "stand":
                            _moveSpeedX = 0;
                            Animation = "startpush";
                            break;
                        case "startpush" when _frame == 3:
                        case "pushstill":
                            Animation = "push";
                            _pushSoundTimer = 201;
                            break;
                        case "push":
                            _moveSpeedX = 3;
                            break;
                        default:
                            Animation = Animation;
                            break;
                    }
                    break;
                case BoyState.Teleport:
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    switch (Animation)
                    {
                        case "walk":
                        case "stand":
                            Animation = "enterwardrobe";
                            var targetWr = ((Wardrobe)Interactor).GetLinkedWr();
                            if (targetWr != null)
                            {
                                var target = targetWr.GetBounds();
                                _teleportTo = new Vector2(target.X + 16, target.Y + 24);
                                Interactor = null;
                            }
                            break;
                        case "enterwardrobe" when _frame == 6:
                            _position = new Vector2(_teleportTo.X, _teleportTo.Y);
                            Animation = "leavewardrobe";
                            break;
                        case "leavewardrobe" when _frame == 7:
                            Animation = "stand";
                            State = BoyState.Idle;
                            break;
                        default:
                            Animation = Animation;
                            break;
                    }
                    break;
                case BoyState.TimeTravel:
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    switch (Animation)
                    {
                        case "walk":
                        case "stand":
                            var p = (Portrait)Interactor;
                            _teleportTo = p.WasMoved ? p.MovedPos : new Vector2();
                            Animation = "enterportrait";
                            if (_control.TimePeriod == TimePeriod.Present)
                                _timeTravelTo = ((Portrait)Interactor).SendTime;
                            Interactor = null;
                            break;
                        case "enterportrait" when _frame == 7:
                            _control.TimePeriod = _control.TimePeriod != TimePeriod.Present ? TimePeriod.Present : _timeTravelTo;
                            Animation = "leaveportrait";
                            if (Math.Abs(_teleportTo.X) > 0 && Math.Abs(_teleportTo.Y) > 0)
                            {
                                X = _teleportTo.X;
                                Y = _teleportTo.Y;
                            }
                            break;
                        case "leaveportrait" when _frame == 7:
                            Animation = "stand";
                            State = BoyState.Idle;
                            break;
                        default:
                            Animation = Animation;
                            break;
                    }
                    break;
                case BoyState.ControllingChair:
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    if (Animation != "control") Animation = "controlstart";
                    if (Animation == "controlstart" && _frame == 2)
                        Animation = "control";
                    break;
                case BoyState.Die:
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    Animation = "disappear";
                    if (_frame == 7)
                    {
                        Reset();
                        _control.ResetLevel();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_frameTime < _frameLength) return;

            var flip = Direction == Direction.Left ? -1 : 1;
            X += _moveSpeedX * flip;
            if (State == BoyState.PushWalk && Animation == "push" && Interactor != null)
            {
                _pushSoundTimer += elapsed;
                var w = (Wardrobe)Interactor;
                if (!_control.CollidingWithSolid(w.PushBox, false))
                {
                    w.X += (int)(_moveSpeedX * flip);
                    if (_pushSoundTimer > 200)
                    {
                        _soundMan.PlaySound("Pushing Wardrobe");
                        _pushSoundTimer = 0;
                    }
                }
                else
                {
                    X -= _moveSpeedX * flip;
                }
            }
            if (Math.Abs(_moveSpeedY) <= 0)
            {
                _moveSpeedY = 1;
                flip = 1;
            }
            Y += _moveSpeedY * flip;
            _frameTime = 0;
            _frame = (_frame + 1) % _animFrames.Count;
        }

        public Rectangle GetBounds()
        {
            return new Rectangle((int)(_position.X), (int)(_position.Y), Width, _height);
        }

        public bool IsSolid()
        {
            return true;
        }
    }
}
