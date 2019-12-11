using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Door : ICollideable, IUpdatable, IDrawable, IInteractable, ISaveable
    {
        # region Attributes

        readonly GameController _gameController = GameController.GetInstance();

        readonly SoundManager _soundManager = SoundManager.Instance();
        //Collidable
        Vector2 _position;
        Rectangle Bounds => new Rectangle(X + 25, Y, 8, 75);

        //Drawable
        readonly SpriteSheet _sheet = SpriteSheetManager.GetInstance().GetSheet("door");
        readonly bool _startLocked;
        int _frameTime, _frameLength, _frame;
        string _animName;
        List<int> _animFrames;

        enum DoorsState { Closed, Opening, Open }

        DoorsState _state;
        readonly string _keyName;

        # endregion

        # region Constructor

        internal Door(string saveString)
        {
            X = 0;
            Y = 0;
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndDoor", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("locked:", StringComparison.Ordinal))
                    try { _startLocked = bool.Parse(line.Substring(7)); }
                    catch (FormatException) { }

                if (line.StartsWith("keyName:", StringComparison.Ordinal)) _keyName = line.Substring(8).Trim();
            }
            IsLocked = _startLocked;

            if (IsLocked)
            {
                Animation = _gameController.TimePeriod == TimePeriod.Present ? "doorclosedpresent" : "doorclosedpast";
                _state = DoorsState.Closed;
            }
            else
            {
                Animation = _gameController.TimePeriod == TimePeriod.Present ? "dooropeningpresent" : "dooropeningpast";
                _state = DoorsState.Open;
            }

        }

        # endregion

        # region Methods

        public void Reset()
        {
            IsLocked = _startLocked;

            if (IsLocked)
            {
                Animation = _gameController.TimePeriod == TimePeriod.Present ? "doorclosedpresent" : "doorclosedpast";
                _state = DoorsState.Closed;
            }
            else
            {
                Animation = _gameController.TimePeriod == TimePeriod.Present ? "dooropeningpresent" : "dooropeningpast";
                _state = DoorsState.Open;
            }
        }

        //Accessors & Mutators
        int X
        {
            get => (int)_position.X;
            set => _position.X = value;
        }

        int Y
        {
            get => (int)_position.Y;
            set => _position.Y = value;
        }

        string Animation
        {
            set
            {
                if (!_sheet.HasAnimation(value) || _animName == value) return;

                _animName = value;
                _animFrames = _sheet.GetAnimation(_animName);
                _frame = 0;
                _frameTime = 0;
            }
        }

        void UnlockObj()
        {
            IsLocked = false;

            _soundManager.PlaySound("Door Unlock");
        }

        internal bool IsLocked { get; private set; }

        //Collideable
        public Rectangle GetBounds() => Bounds;

        public bool IsSolid() => IsLocked;

        //Drawable

        public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, _position, _sheet.GetSprite(_animFrames.ElementAt(_frame)), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Background);

        //Updatable
        public void Update(GameTime time)
        {
            _frameTime += time.ElapsedGameTime.Milliseconds;
            if (!string.IsNullOrEmpty(_keyName) && DoorKey.GetKey(_keyName)?.PickedUp == true && _state == DoorsState.Closed)
            {
                _state = DoorsState.Opening;
                _frameLength = 100;
                UnlockObj();
            }

            var timeP = _gameController.TimePeriod == TimePeriod.Present ? "present" : "past";
            switch (_state)
            {
                case DoorsState.Open:
                    Animation = "dooropen" + timeP;
                    break;
                case DoorsState.Opening:
                    if (_frameTime >= _frameLength)
                    {
                        Animation = "dooropen" + timeP;
                        _state = DoorsState.Open;
                    }
                    else
                        Animation = "dooropening" + timeP;

                    break;
                case DoorsState.Closed:
                    Animation = "doorclosed" + timeP;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_frameTime < _frameLength) return;

            _frameTime = 0;
            _frame = (_frame + 1) % _animFrames.Count;
        }

        //Interactive
        public void Interact()
        {
        }

        # endregion
    }
}
