using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal sealed class Bookcase : ICollideable, IUpdatable, IDrawable, IInteractable, ISaveable
    {
        # region Attributes

        private readonly GameController _control = GameController.GetInstance();
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        private readonly SoundManager _soundMan = SoundManager.GetInstance();
        //Collidable
        private Vector2 _position;
        private readonly string _button1;
        private readonly string _button2;
        private int _unlockTimer;

        private int X => (int)_position.X;

        private int Y => (int)_position.Y;
        private Rectangle Bounds => new Rectangle(X, Y, 37, 75);

        //Drawable
        private readonly SpriteSheet _sheet;
        private int _frameTime;
        private readonly int _frameLength;
        private int _frame;
        private string _animName;
        private List<int> _animFrames;
        public enum BookcasesState { Closed, Closing, Opening, Open }
        public BookcasesState State;

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

        # endregion

        # region Constructors

        public Bookcase(string saveString)
        {
            _sheet = _sheetMan.GetSheet("bookcase");
            var x = 0;
            var y = 0;
            _button1 = "";
            _button2 = "";
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndBookcase", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { x = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("button1:", StringComparison.Ordinal)) _button1 = line.Substring(8).Trim();
                if (line.StartsWith("button2:", StringComparison.Ordinal)) _button2 = line.Substring(8).Trim();
            }

            Animation = "bookcaseopening";
            State = BookcasesState.Open;
            _position = new Vector2(x, y);
            _frameLength = 100;
        }

        # endregion

        # region Methods

        //Collideable
        public Rectangle GetBounds()
        {
            return Bounds;
        }
        public bool IsSolid()
        {
            return false;
        }

        //Drawable

        public void Draw(SpriteBatch renderer, Color tint)
        {
            Rectangle sprite = _sheet.GetSprite(_animFrames.ElementAt(_frame));
            renderer.Draw(_sheet.Image, new Vector2(X, Y), sprite, tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.Wardrobe);
        }

        //Updatable
        public void Update(GameTime time)
        {
            var elapsed = time.ElapsedGameTime.Milliseconds;
            _frameTime += elapsed;
            if (State == BookcasesState.Opening)
                _unlockTimer += elapsed;
            if (_unlockTimer >= 450)
            {
                _soundMan.PlaySound("Final Door Part 2");
                _unlockTimer = 0;
            }
            var b1Pushed = false;
            var b2Pushed = false;
            if (_button1 != "")
            {
                if (Button.GetKey(_button1)?.StepOn == true) b1Pushed = true;
            }
            else
            {
                b1Pushed = true;
            }

            if (_button2 != "")
            {
                if (Button.GetKey(_button2)?.StepOn == true) b2Pushed = true;
            }
            else
            {
                b2Pushed = true;
            }

            if (b1Pushed && b2Pushed)
            {
                if (State == BookcasesState.Closed)
                {
                    State = BookcasesState.Opening;
                    if (_unlockTimer == 0) _soundMan.PlaySound("Final Door Part 1");
                }
            }
            else
            {
                _unlockTimer = 0;
                State = State != BookcasesState.Closed ? BookcasesState.Closing : BookcasesState.Closed;
            }

            switch (State)
            {
                case BookcasesState.Open:
                    Animation = "bookcaseopen";
                    break;
                case BookcasesState.Opening:
                    if (_frame == 4)
                    {
                        Animation = "bookcaseopen";
                        State = BookcasesState.Open;
                    }
                    else
                    {
                        Animation = "bookcaseopening";
                    }
                    break;
                case BookcasesState.Closing:
                    _unlockTimer = 0;
                    if (Animation == "bookcaseclosing" && _frame == 4)
                    {
                        Animation = "close";
                        State = BookcasesState.Closed;
                    }
                    else
                    {
                        Animation = "bookcaseclosing";
                    }
                    break;
                case BookcasesState.Closed:
                    _unlockTimer = 0;
                    Animation = "bookcaseclosed";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_frameTime < _frameLength) return;

            _frameTime = 0;
            _frame = (_frame + 1) % _animFrames.Count;
        }

        public void Interact()
        {
            if (State == BookcasesState.Open)
                Game1.EndGame = true;
        }

        //reset
        public void Reset()
        {
            if (_control.NextLevel())
                _control.InitLevel(true);
        }

        # endregion
    }
}
