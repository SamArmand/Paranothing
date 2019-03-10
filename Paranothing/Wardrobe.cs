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

		static readonly Dictionary<string, Wardrobe> Wardrobes = new Dictionary<string, Wardrobe>();
		readonly GameController _gameController = GameController.GetInstance();
		readonly SpriteSheetManager _spriteSheetManager = SpriteSheetManager.GetInstance();

		readonly SoundManager _soundManager = SoundManager.Instance();

		//Collidable
		readonly Vector2 _startPos;
		Vector2 _positionPres, _positionPast1, _positionPast2;
		readonly string _keyName = "";

		internal int X
		{
			get
			{
				switch (_gameController.TimePeriod)
				{
					case TimePeriod.FarPast:
						return (int) _positionPast2.X;
					case TimePeriod.Past:
						return (int) _positionPast1.X;
					case TimePeriod.Present:
						return (int) _positionPres.X;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			set
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
				switch (_gameController.TimePeriod)
				{
					case TimePeriod.FarPast:
						return (int) _positionPast2.Y;
					case TimePeriod.Past:
						return (int) _positionPast1.Y;
					case TimePeriod.Present:
						return (int) _positionPres.Y;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		internal Rectangle EnterBox => new Rectangle(X + 24, Y + 9, 23, 73);

		internal Rectangle PushBox => new Rectangle(X + 2, Y + 2, 65, 78);

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

		internal enum WardrobeState
		{
			Closed,
			Opening,
			Open
		}

		internal WardrobeState State;

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

		internal Wardrobe(string saveString)
		{
			_sheet = _spriteSheetManager.GetSheet("wardrobe");
			var x = 0;
			var y = 0;
			_startLocked = false;
			var name = "WR";
			var link = "WR";
			var lines = saveString.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			var lineNum = 0;
			var line = "";
			while (!line.StartsWith("EndWardrobe", StringComparison.Ordinal) && lineNum < lines.Length)
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

				if (line.StartsWith("name:", StringComparison.Ordinal)) name = line.Substring(5).Trim();
				if (line.StartsWith("locked:", StringComparison.Ordinal))
					try
					{
						_startLocked = bool.Parse(line.Substring(7));
					}
					catch (FormatException)
					{
					}

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
			if (Wardrobes.ContainsKey(name))
				Wardrobes.Remove(name);
			Wardrobes.Add(name, this);
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

		public void Draw(SpriteBatch renderer, Color tint) => renderer.Draw(_sheet.Image, new Vector2(X, Y),
																			_sheet
																			   .GetSprite(_animFrames
																							 .ElementAt(_frame)), tint,
																			0f, new Vector2(), 1f, SpriteEffects.None,
																			DrawLayer.Wardrobe);

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
						Animation = "wardrobeopening";

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

		internal Wardrobe GetLinkedWr()
		{
			Wardrobe w = null;
			if (Wardrobes.ContainsKey(_linkedName))
				Wardrobes.TryGetValue(_linkedName, out w);
			return w;
		}

		void UnlockObj()
		{
			_locked = false;

			_soundManager.PlaySound("Wardrobe Unlock");
		}

		internal bool IsLocked() => _locked;

		public void Interact()
		{
			var player = _gameController.Player;
			if (Rectangle.Intersect(player.GetBounds(), EnterBox).Width != 0)
			{
				var linkedWr = GetLinkedWr();
				if (_locked || !linkedWr?.IsLocked() != true ||
					_gameController.CollidingWithSolid(linkedWr.EnterBox)) return;

				player.State = Boy.BoyState.Teleport;
				player.X = X + 16;

				_soundManager.PlaySound("Wardrobe Travel", false, true);
			}
			else
			{
				player.State = _gameController.CollidingWithSolid(PushBox, false)
								   ? Boy.BoyState.PushingStill
								   : Boy.BoyState.PushWalk;
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