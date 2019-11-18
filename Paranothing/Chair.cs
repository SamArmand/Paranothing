using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    sealed class Chair : ICollideable, IUpdatable, IDrawable, IInteractable, ISaveable
    {
        # region Attributes

        readonly GameController _gameController = GameController.GetInstance();

        //Collidable
        readonly Vector2 _startPos;
        Vector2 _positionPres;
        Vector2 _positionPast1;
        Vector2 _positionPast2;
        int _tX, _tY;
        const int Speed = 3;
        int _moveTime;
        const int Movelength = 70;

        Rectangle Bounds => new Rectangle(X, Y, 40, 52);

        //Drawable
        readonly SpriteSheet _sheet = SpriteSheetManager.GetInstance().GetSheet("chair");

        internal enum ChairsState { Idle, Falling, Moving }

        internal ChairsState State;
        readonly ActionBubble _bubble = new ActionBubble();

        # endregion

        # region Constructor

        internal Chair(string saveString)
        {
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var x = 0;
            var y = 0;
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndChairs", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { x = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (!line.StartsWith("y:", StringComparison.Ordinal)) continue;

                try { y = int.Parse(line.Substring(2)); }
                catch (FormatException) { }
            }
            _startPos = new Vector2(x, y);
            _positionPres = new Vector2(x, y);
            _positionPast1 = new Vector2(x, y);
            _positionPast2 = new Vector2(x, y);
            _bubble.Chair = this;
        }

        # endregion

        # region Methods

        //Accessors & Mutators
        internal int X
        {
            get
            {
				return _gameController.TimePeriod switch
				{
					TimePeriod.FarPast => (int)_positionPast2.X,
					TimePeriod.Past => (int)_positionPast1.X,
					TimePeriod.Present => (int)_positionPres.X,
					_ => 0,
				};
			}
            private set
            {
                switch (_gameController.TimePeriod)
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
        internal int Y
        {
            get
            {
				return _gameController.TimePeriod switch
				{
					TimePeriod.FarPast => (int)_positionPast2.Y,
					TimePeriod.Past => (int)_positionPast1.Y,
					TimePeriod.Present => (int)_positionPres.Y,
					_ => 0,
				};
			}
            set
            {
                switch (_gameController.TimePeriod)
                {
                    case TimePeriod.FarPast:
                        _positionPast2.Y = value;
                        _positionPast1.Y = value;
                        _positionPres.Y = value;
                        break;
                    case TimePeriod.Past:
                        _positionPast1.Y = value;
                        _positionPres.Y = value;
                        break;
                    case TimePeriod.Present:
                        _positionPres.Y = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        //Collideable
        public Rectangle GetBounds() => Bounds;
        public bool IsSolid() => false;

        public void Draw(SpriteBatch renderer, Color tint)
        {
            renderer.Draw(_sheet.Image, new Vector2(X, Y),
                _gameController.TimePeriod == TimePeriod.Present ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                new Vector2(), 1f, SpriteEffects.None, DrawLayer.Chairs);
            _bubble.Draw(renderer, tint);
        }

        //Updatable
        public void Update(GameTime time)
        {
            var player = _gameController.Player;
            switch (State)
            {
                case ChairsState.Idle:
                    if (player.NearestChair != null && player.NearestChair != this)
                    {
                        if (player.NearestChair.State == ChairsState.Idle)
                        {
                            var oldDist = new Vector2(player.X - player.NearestChair.X, player.Y - player.NearestChair.Y);
                            var newDist = new Vector2(player.X - X, player.Y - Y);
                            if (newDist.LengthSquared() < oldDist.LengthSquared())
                            {
                                player.NearestChair = this;
                                _bubble.Show();
                                _bubble.SetAction(ActionBubble.BubbleAction.Chair, false);
                            }
                            else
                                _bubble.Hide();
                        }
                    }
                    else
                    {
                        player.NearestChair = this;
                        _bubble.Show();
                        _bubble.SetAction(ActionBubble.BubbleAction.Chair, false);
                    }
                    break;
                case ChairsState.Falling:
                    _bubble.Hide();
                    Y++;
                    break;
                case ChairsState.Moving:
                    _bubble.Hide();
                    _moveTime += time.ElapsedGameTime.Milliseconds;
                    if (_moveTime >= Movelength)
                    {
                        X += _tX * Speed;
                        Y += _tY * Speed;
                        _moveTime = 0;

                        var smallerBound = new Rectangle(X + 2, Y + 2, Bounds.Width - 4, Bounds.Height - 4);
                        if (_gameController.CollidingWithSolid(smallerBound, false))
                        {
                            X -= _tX * Speed;
                            Y -= _tY * Speed;
                        }
                        _tX = 0;
                        _tY = 0;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal void Move(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    _tY = -1;
                    break;
                case Direction.Down:
                    _tY = 1;
                    break;
                case Direction.Left:
                    _tX = -1;
                    break;
                case Direction.Right:
                    _tX = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        //Interactive
        public void Interact()
        {
        }

        //reset
        public void Reset()
        {
            _positionPres = new Vector2(_startPos.X, _startPos.Y);
            _positionPast1 = new Vector2(_startPos.X, _startPos.Y);
            _positionPast2 = new Vector2(_startPos.X, _startPos.Y);
        }

        # endregion
    }
}
