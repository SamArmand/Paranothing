using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
	sealed class Bookcase : ICollideable, IUpdatable, IDrawable, IInteractable, ISaveable
	{
		# region Attributes

		readonly GameController _gameController = GameController.GetInstance();

		readonly SoundManager _soundManager = SoundManager.Instance();

		//Collidable
		readonly Vector2 _position;
		readonly string _button1 = string.Empty, _button2 = string.Empty;
		int _unlockTimer;

		int X => (int) _position.X;
		int Y => (int) _position.Y;

		Rectangle Bounds => new Rectangle(X, Y, 37, 75);

		//Drawable
		readonly SpriteSheet _spriteSheet = SpriteSheetManager.GetInstance().GetSheet("bookcase");
		int _frameTime, _frame;
		readonly int _frameLength;
		string _animName;
		List<int> _animationFrames;

		public enum BookcaseState
		{
			Closed,
			Closing,
			Opening,
			Open
		}

		internal BookcaseState State;

		string Animation
		{
			get => _animName;
			set
			{
				if (!_spriteSheet.HasAnimation(value) || _animName == value) return;

				_animName = value;
				_animationFrames = _spriteSheet.GetAnimation(_animName);
				_frame = 0;
				_frameTime = 0;
			}
		}

		# endregion

		# region Constructors

		internal Bookcase(string saveString)
		{
			var x = 0;
			var y = 0;
			var lines = saveString.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			var lineNum = 0;
			var line = string.Empty;
			while (!line.StartsWith("EndBookcase", StringComparison.Ordinal) && lineNum < lines.Length)
			{
				line = lines[lineNum++];
				if (line.StartsWith("x:", StringComparison.Ordinal))
					try
					{
						x = int.Parse(line.Substring(2));
					}
					catch (FormatException)
					{
					}

				if (line.StartsWith("y:", StringComparison.Ordinal))
					try
					{
						y = int.Parse(line.Substring(2));
					}
					catch (FormatException)
					{
					}

				if (line.StartsWith("button1:", StringComparison.Ordinal)) _button1 = line.Substring(8).Trim();
				if (line.StartsWith("button2:", StringComparison.Ordinal)) _button2 = line.Substring(8).Trim();
			}

			Animation = "bookcaseopening";
			State = BookcaseState.Open;
			_position = new Vector2(x, y);
			_frameLength = 100;
		}

		# endregion

		# region Methods

		//Collideable
		public Rectangle GetBounds() => Bounds;

		public bool IsSolid() => false;

		//Drawable

		public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_spriteSheet.Image, new Vector2(X, Y),
																			_spriteSheet.GetSprite(_animationFrames
																								.ElementAt(_frame)),
																			tint,
																			0f, new Vector2(), 1f, SpriteEffects.None,
																			DrawLayer.Wardrobe);

		//Updatable
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

			if (_button1 == "" || Button.GetKey(_button1)?.StepOn == true && _button2 == "" ||
				Button.GetKey(_button2)?.StepOn == true)
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
					Animation = "bookcaseopen";
					break;
				case BookcaseState.Opening:
					if (_frame == 4)
					{
						Animation = "bookcaseopen";
						State = BookcaseState.Open;
					}
					else
						Animation = "bookcaseopening";

					break;
				case BookcaseState.Closing:
					_unlockTimer = 0;
					if (Animation == "bookcaseclosing" && _frame == 4)
					{
						Animation = "close";
						State = BookcaseState.Closed;
					}
					else
						Animation = "bookcaseclosing";

					break;
				case BookcaseState.Closed:
					_unlockTimer = 0;
					Animation = "bookcaseclosed";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (_frameTime < _frameLength) return;

			_frameTime = 0;
			_frame = (_frame + 1) % _animationFrames.Count;
		}

		public void Interact() => Game1.EndGame |= State == BookcaseState.Open;

		//reset
		public void Reset()
		{
			if (_gameController.NextLevel())
				_gameController.InitLevel(true);
		}

		# endregion
	}
}