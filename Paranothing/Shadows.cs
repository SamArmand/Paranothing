using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Shadows : ICollideable, IUpdatable, IDrawable, ISaveable
    {
        # region Attributes

        readonly GameController _control = GameController.GetInstance();
        readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();

        readonly SoundManager _soundMan = SoundManager.Instance();
        //Drawable
        readonly SpriteSheet _sheet;
        int _frame;
        int _frameLength;
        int _frameTime;
        string _animName;
        List<int> _animFrames;

        string Animation
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
        //Collideable
        int _moveSpeedX, _moveSpeedY; // Pixels per animation frame
        readonly Vector2 _startPos;
        Vector2 _position;
        Vector2 _soundPos;
        readonly int _patrolDistance;
        int _distMoved;
        Rectangle Bounds => new Rectangle(X, Y+7, 32, 74);

        public enum ShadowState { Idle, Walk, SeekSound }
        public ShadowState State;
        Direction _direction;

        # endregion

        # region Constructor

        public Shadows(string saveString)
        {
            _sheet = _sheetMan.GetSheet("shadow");
            Animation = "walk";
            State = ShadowState.Walk;
            X = 0;
            Y = 0;
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndShadow", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (!line.StartsWith("patrolDist:", StringComparison.Ordinal)) continue;

                try { _patrolDistance = int.Parse(line.Substring(11)); }
                catch (FormatException) { }
            }
            if (_patrolDistance < 0)
                _patrolDistance = -_patrolDistance;
            _distMoved = _patrolDistance;
            _startPos = new Vector2(X, Y);
            _soundPos = new Vector2(X, Y);
        }

        public void Reset()
        {
            _frame = 0;
            _frameTime = 0;
            _position = new Vector2(_startPos.X, _startPos.Y);
            _soundPos = new Vector2(_startPos.X, _startPos.Y);
            _distMoved = _patrolDistance;
            State = ShadowState.Walk;
            Animation = "walk";
            _direction = Direction.Right;
        }

        # endregion

        # region Methods

        //Accessors & Mutators
        public int X
        {
            get => (int)_position.X;
            private set => _position.X = value;
        }
        public int Y
        {
            get => (int)_position.Y;
            private set => _position.Y = value;
        }

        //Updatable
        public void Update(GameTime time)
        {
            if (_control.TimePeriod != TimePeriod.Present) return;

            _frameTime += time.ElapsedGameTime.Milliseconds;
            switch (State)
            {
                case ShadowState.Idle:
                    if (Animation == "walk")
                        Animation = "stopwalk";
                    if (Animation == "stopwalk" && _frame == 2)
                        Animation = "stand";
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    _frameLength = 80;
                    break;
                case ShadowState.Walk:
                    if (_patrolDistance != 0)
                        if (Animation == "stopwalk" && _frame == 2 || Animation == "stand" || Animation == "walk")
                        {
                            _frameLength = 80;
                            Animation = "walk";
                            _moveSpeedX = 3;
                            _moveSpeedY = 0;
                        }
                        else
                        {
                            _moveSpeedX = 0;
                        }
                    else
                        State = ShadowState.Idle;
                    break;
                case ShadowState.SeekSound:
                    Animation = "walk";
                    _moveSpeedX = 3;
                    _moveSpeedY = 0;
                    _direction = _soundPos.X > X ? Direction.Right : Direction.Left;
                    if (Math.Abs(_soundPos.X - X) < 3)
                        State = ShadowState.Idle;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_frameTime < _frameLength) return;

            var flip = 1;
            if (_direction == Direction.Left)
                flip = -1;
            X += _moveSpeedX * flip;
            Y += _moveSpeedY * flip;
            _frameTime = 0;
            _frame = (_frame + 1) % _animFrames.Count;
            if (State == ShadowState.Walk && _patrolDistance != 0)
            {
                _distMoved += _moveSpeedX;
                if (_distMoved >= _patrolDistance * 2)
                {
                    Animation = "stopwalk";
                    X -= (_patrolDistance * 2 - _distMoved) * flip;
                    _direction = _direction == Direction.Left ? Direction.Right : Direction.Left;
                    _distMoved = 0;
                }
            }

            if (!_control.CollidingWithSolid(GetBounds(), false)) return;

            if (State == ShadowState.SeekSound)
            {
                State = ShadowState.Idle;
                X -= _moveSpeedX * flip;
                Y -= _moveSpeedY * flip;
            }
            else if (State == ShadowState.Walk)
            {
                _distMoved = _patrolDistance * 2 - _distMoved;
                _distMoved -= _moveSpeedX;
                Animation = "stopwalk";
                if (_direction == Direction.Left)
                {
                    X += _moveSpeedX;
                    _direction = Direction.Right;
                }
                else
                {
                    X -= _moveSpeedX;
                    _direction = Direction.Left;
                }
            }
        }

        public void StalkNoise(int x, int y)
        {
            _soundPos = new Vector2(x, y);
            State = ShadowState.SeekSound;
            if (Animation == "walk")
                Animation = "stopwalk";

            _soundMan.PlaySound("Shadow");
        }

        //Drawable

        public void Draw(SpriteBatch renderer, Color tint)
        {
            if (_control.TimePeriod == TimePeriod.Present) renderer.Draw(_sheet.Image, _position, _sheet.GetSprite(_animFrames.ElementAt(_frame)), tint, 0f, new Vector2(), 1f, _direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, DrawLayer.Player + 0.005f);
        }

        //Collideable
        public Rectangle GetBounds() => Bounds;

        public bool IsSolid() => false;

        # endregion
    }
}
