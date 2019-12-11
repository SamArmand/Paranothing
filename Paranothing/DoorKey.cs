using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
	sealed class DoorKey : IDrawable, ICollideable, ISaveable
	{
		# region Attributes

		static readonly Dictionary<string, DoorKey> DoorKeys = new Dictionary<string, DoorKey>();
		readonly GameController _gameController = GameController.GetInstance();

		//Collideable
		Vector2 _position;
		Rectangle Bounds => new Rectangle(X, Y, 16, 9);

		//Drawable
		readonly SpriteSheet _sheet = SpriteSheetManager.GetInstance().GetSheet("key");
		internal bool RestrictTime { get; }
		internal TimePeriod InTime { get; } = TimePeriod.Present;
		internal bool PickedUp { get; set; }

		internal string Name { get; } = "Key";

		# endregion

		# region Constructor

		internal DoorKey(string saveString)
		{
			X = 0;
			Y = 0;
			var lines = saveString.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			var lineNum = 0;
			var line = "";
			while (!line.StartsWith("EndKey", StringComparison.Ordinal) && lineNum < lines.Length)
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

				if (line.StartsWith("y:", StringComparison.Ordinal))
					try
					{
						Y = int.Parse(line.Substring(2));
					}
					catch (FormatException)
					{
					}

				if (line.StartsWith("restrictTime:", StringComparison.Ordinal))
				{
					RestrictTime = true;
					var t = line.Substring(13).Trim();
					switch (t)
					{
						case "Present":
							InTime = TimePeriod.Present;
							break;
						case "Past":
							InTime = TimePeriod.Past;
							break;
						case "FarPast":
							InTime = TimePeriod.FarPast;
							break;
						default:
							RestrictTime = false;
							break;
					}
				}

				if (line.StartsWith("name:", StringComparison.Ordinal)) Name = line.Substring(5).Trim();
			}

			if (DoorKeys.ContainsKey(Name))
				DoorKeys.Remove(Name);
			DoorKeys.Add(Name, this);
		}

		public void Reset() => PickedUp = false;

		# endregion

		# region Methods

		//Accessors & Mutators
		int X
		{
			get => (int) _position.X;
			set => _position.X = value;
		}

		int Y
		{
			get => (int) _position.Y;
			set => _position.Y = value;
		}

		//Collideable
		public Rectangle GetBounds() => Bounds;

		public bool IsSolid() => false;

		//Drawable

		public void Draw(SpriteBatch renderer, Color tint)
		{
			if (PickedUp || (RestrictTime && _gameController.TimePeriod != InTime)) return;

			renderer.Draw(_sheet.Image, Bounds,
						  _gameController.TimePeriod == TimePeriod.Present ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint,
						  0f,
						  new Vector2(), SpriteEffects.None, DrawLayer.Key);
		}

		internal static DoorKey GetKey(string name)
		{
			DoorKey doorKey;
			if (DoorKeys.ContainsKey(name))
				DoorKeys.TryGetValue(name, out doorKey);
			else
				doorKey = null;
			return doorKey;
		}

		#endregion
	}
}