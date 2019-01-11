using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Wardrobe : ICollideable, IUpdatable, IDrawable, IInteractable, ISaveable
    {
        # region Attributes

        static readonly Dictionary<string, Wardrobe> WardrobeDict = new Dictionary<string, Wardrobe>();
        readonly GameController _control = GameController.GetInstance();
        readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();

        readonly SoundManager _soundMan = SoundManager.Instance();
        //Collidable
        readonly Vector2 _startPos;
        Vector2 _positionPres;
        Vector2 _positionPast1;
        Vector2 _positionPast2;
        readonly string _keyName = "";
        public int X
        {
            get
            {
                switch (_control.TimePeriod)
                {
                    case TimePeriod.FarPast:
                        return (int)_positionPast2.X;
                    case TimePeriod.Past:
                        return (int)_positionPast1.X;
                    case TimePeriod.Present:
                        return (int)_positionPres.X;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            set
            {
                switch (_control.TimePeriod)
                {
                    case TimePeriod.FarPast:
                        _positionPast2.X = value;
                        _positionPast1.X = value;
                        _positionPres.X = value;
                        break;
                    case TimePeriod.Past:
                        _positionPast1.X = value;
                        _positionPres.X = value;
                        break;
                    case TimePeriod.Present:
                        _positionPres.X = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        public int Y
        {
            get
            {
                switch (_control.TimePeriod)
                {
                    case TimePeriod.FarPast:
                        return (int)_positionPast2.Y;
                    case TimePeriod.Past:
                        return (int)_positionPast1.Y;
                    case TimePeriod.Present:
                        return (int)_positionPres.Y;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Rectangle EnterBox => new Rectangle(X + 24, Y + 9, 23, 73);

        public Rectangle PushBox => new Rectangle(X+2, Y+2, 65, 78);

        //Drawable
        readonly SpriteSheet _sheet;
        string _linkedName;
        bool _locked;
        readonly bool _startLocked;
        int _frameTime;
        int _frameLength;
        int _frame;
        string _animName;
        List<int> _animFrames;
        public enum WardrobeState { Closed, Opening, Open }
        public WardrobeState State;

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

        # endregion

        # region Constructor

        public Wardrobe(string saveString)
        {
            _sheet = _sheetMan.GetSheet("wardrobe");
            var x = 0;
            var y = 0;
            _startLocked = false;
            var name = "WR";
            var link = "WR";
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndWardrobe", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { x = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("name:", StringComparison.Ordinal)) name = line.Substring(5).Trim();
                if (line.StartsWith("locked:", StringComparison.Ordinal))
                    try { _startLocked = bool.Parse(line.Substring(7)); }
                    catch (FormatException) { }

                if (line.StartsWith("link:", StringComparison.Ordinal)) link = line.Substring(5).Trim();
                if (line.StartsWith("keyName:", StringComparison.Ordinal)) _keyName = line.Substring(8).Trim();
            }
            _locked = _startLocked;
            if (_startLocked)
            {
                Animation = "wardrobeclosed";
                State = WardrobeState.Closed;
            }
            else
            {
                Animation = "wardrobeopening";
                State = WardrobeState.Open;
            }
            _startPos = new Vector2(x, y);
            _positionPres = new Vector2(x, y);
            _positionPast1 = new Vector2(x, y);
            _positionPast2 = new Vector2(x, y);
            if (WardrobeDict.ContainsKey(name))
                WardrobeDict.Remove(name);
            WardrobeDict.Add(name, this);
            SetLinkedWr(link);
        }

        public void Reset()
        {
            _positionPres = new Vector2(_startPos.X, _startPos.Y);
            _positionPast1 = new Vector2(_startPos.X, _startPos.Y);
            _positionPast2 = new Vector2(_startPos.X, _startPos.Y);
            _locked = _startLocked;

            if (_startLocked)
            {
                Animation = "wardrobeclosed";
                State = WardrobeState.Closed;
            }
            else
            {
                Animation = "wardrobeopening";
                State = WardrobeState.Open;
            }
        }

        # endregion

        # region Methods

        //Collideable
        public Rectangle GetBounds() => PushBox;

        public bool IsSolid() => false;

        //Drawable

        public void Draw(SpriteBatch renderer, Color tint)
        {
            renderer.Draw(_sheet.Image, new Vector2(X, Y), _sheet.GetSprite(_animFrames.ElementAt(_frame)), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Wardrobe);
        }

        //Updatable
        public void Update(GameTime time)
        {
            _frameTime += time.ElapsedGameTime.Milliseconds;

            if (_keyName != "")
            {
                var k = DoorKey.GetKey(_keyName);
                if (k?.PickedUp == true && State == WardrobeState.Closed) State = WardrobeState.Opening;
            }

            switch (State)
            {
                case WardrobeState.Open:
                    Animation = "wardrobeopen";
                    break;
                case WardrobeState.Opening:
                    _frameLength = 100;
                    if (_frame == 2)
                    {
                        Animation = "wardrobeopen";
                        State = WardrobeState.Open;
                        UnlockObj();
                    }
                    else
                    {
                        Animation = "wardrobeopening";
                    }

                    break;
                case WardrobeState.Closed:
                    Animation = "wardrobeclosed";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_frameTime < _frameLength) return;

            _frameTime = 0;
            _frame = (_frame + 1) % _animFrames.Count;
        }

        void SetLinkedWr(string linkedName) => _linkedName = linkedName;

        public Wardrobe GetLinkedWr()
        {
            Wardrobe w = null;
            if (WardrobeDict.ContainsKey(_linkedName))
                WardrobeDict.TryGetValue(_linkedName, out w);
            return w;
        }

        void UnlockObj()
        {
            _locked = false;

            if (GameTitle.ToggleSound) _soundMan.PlaySound("Wardrobe Unlock");
        }

        public bool IsLocked() => _locked;

        public void Interact()
        {
            var player = _control.Player;
            if (Rectangle.Intersect(player.GetBounds(), EnterBox).Width != 0)
            {
                var linkedWr = GetLinkedWr();
                if (_locked || !linkedWr?.IsLocked() != true || _control.CollidingWithSolid(linkedWr.EnterBox)) return;

                player.State = Boy.BoyState.Teleport;
                player.X = X + 16;

                _soundMan.PlaySound("Wardrobe Travel");
            }
            else
            {
                player.State = _control.CollidingWithSolid(PushBox, false) ? Boy.BoyState.PushingStill : Boy.BoyState.PushWalk;
                if (player.X > X)
                {
                    player.X = X + 67;
                    player.Direction = Direction.Left;
                }
                else
                {
                    player.X = X - 36;
                    player.Direction = Direction.Right;
                }
            }
        }

        # endregion
    }
}
