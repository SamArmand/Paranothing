using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
	sealed class Portrait : IDrawable, ICollideable, IInteractable, ISaveable
	{
		readonly GameController _gameController = GameController.GetInstance();
		readonly SpriteSheetManager _spriteSheetManager = SpriteSheetManager.GetInstance();
		readonly SoundManager _soundManager = SoundManager.Instance();
		Vector2 _position;

		internal int X
		{
			get => (int) _position.X;
			private set => _position.X = value;
		}

		internal int Y
		{
			get => (int) _position.Y;
			private set => _position.Y = value;
		}

		readonly SpriteSheet _sheet;
		internal bool WasMoved { get; }
		internal Vector2 MovedPos;
		internal TimePeriod InTime { get; }
		internal TimePeriod SendTime { get; }

		internal Portrait(string saveString, string str)
		{
			_sheet = _spriteSheetManager.GetSheet("portrait");
			SendTime = TimePeriod.Past;
			ParseString(saveString, str);
		}

		//present past constructor
		internal Portrait(string saveString, TimePeriod period)
		{
			_sheet = _spriteSheetManager.GetSheet("portrait");
			switch (period)
			{
				case TimePeriod.Present:
					ParseString(saveString, "EndPresentPortrait");
					WasMoved = true;
					InTime = TimePeriod.Present;
					SendTime = TimePeriod.Past;
					break;
				case TimePeriod.Past:
					ParseString(saveString, "EndPastPortrait");
					WasMoved = true;
					InTime = TimePeriod.Past;
					SendTime = TimePeriod.Past;
					break;
				case TimePeriod.FarPast:
					ParseString(saveString, "EndOldPortrait");
					SendTime = TimePeriod.FarPast;
					_sheet = _spriteSheetManager.GetSheet("oldportrait");
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(period), period, null);
			}
		}

		void ParseString(string saveString, string str)
		{
			var lines = saveString.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			X = 0;
			Y = 0;
			var lineNum = 0;
			var line = "";
			while (!line.StartsWith(str, StringComparison.Ordinal) && lineNum < lines.Length)
			{
				line = lines[lineNum++];
				if (line.StartsWith("x:", StringComparison.Ordinal))
					try
					{
						X = int.Parse(line.Substring(2));
					}
					catch (FormatException)
					{
					}

				if (!line.StartsWith("y:", StringComparison.Ordinal)) continue;

				try
				{
					Y = int.Parse(line.Substring(2));
				}
				catch (FormatException)
				{
				}
			}
		}

		public void Reset()
		{
		}

		public void Draw(SpriteBatch renderer, Color tint)
		{
			if ((!WasMoved || _gameController.TimePeriod == InTime) &&
				!(_gameController.TimePeriod == TimePeriod.FarPast && SendTime != TimePeriod.FarPast))
				renderer.Draw(_sheet.Image, _position,
							  _gameController.TimePeriod == TimePeriod.Present
								  ? _sheet.GetSprite(1)
								  : _sheet.GetSprite(0), tint, 0f,
							  new Vector2(), 1f, SpriteEffects.None, DrawLayer.Background);
		}

		public Rectangle GetBounds() => new Rectangle(X, Y, 35, 30);

		public bool IsSolid() => false;

		public void Interact()
		{
			if (_gameController.TimePeriod == TimePeriod.FarPast && SendTime != TimePeriod.FarPast) return;

			var player = _gameController.Player;
			player.State = Boy.BoyState.TimeTravel;
			player.X = X;

			_soundManager.PlaySound("Portrait Travel", false, true);
		}
	}
}