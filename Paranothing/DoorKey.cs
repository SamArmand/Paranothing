using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal sealed class DoorKey : IDrawable, ICollideable, ISaveable, IInteractable
    {
        # region Attributes
        private static readonly Dictionary<string, DoorKey> KeyDict = new Dictionary<string, DoorKey>();
        private readonly GameController _control = GameController.GetInstance();
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        //Collideable
        private Vector2 _position;
        private Rectangle Bounds => new Rectangle(X, Y, 16, 9);

        //Drawable
        private readonly SpriteSheet _sheet;
        public bool RestrictTime { get; }
        public TimePeriod InTime { get; }
        public bool PickedUp;

        public string Name { get; }

        # endregion

        # region Constructor

        public DoorKey(string saveString)
        {
            _sheet = _sheetMan.GetSheet("key");
            PickedUp = false;
            RestrictTime = false;
            InTime = TimePeriod.Present;
            X = 0;
            Y = 0;
            Name = "Key";
            var lines = saveString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var lineNum = 0;
            var line = "";
            while (!line.StartsWith("EndKey", StringComparison.Ordinal) && lineNum < lines.Length)
            {
                line = lines[lineNum++];
                if (line.StartsWith("x:", StringComparison.Ordinal))
                    try { X = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

                if (line.StartsWith("y:", StringComparison.Ordinal))
                    try { Y = int.Parse(line.Substring(2)); }
                    catch (FormatException) { }

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

            if (KeyDict.ContainsKey(Name))
                KeyDict.Remove(Name);
            KeyDict.Add(Name, this);
        }

        public void Reset()
        {
            PickedUp = false;
        }

        # endregion

        # region Methods

        //Accessors & Mutators
        private int X
        {
            get => (int)_position.X;
            set => _position.X = value;
        }

        private int Y
        {
            get => (int)_position.Y;
            set => _position.Y = value;
        }

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
            if (PickedUp || (RestrictTime && _control.TimePeriod != InTime)) return;

            renderer.Draw(_sheet.Image, Bounds,
                _control.TimePeriod == TimePeriod.Present ? _sheet.GetSprite(1) : _sheet.GetSprite(0), tint, 0f,
                new Vector2(), SpriteEffects.None, DrawLayer.Key);
        }

        //Interactive
        public void Interact()
        {
        }

        public static DoorKey GetKey(string name)
        {
            DoorKey k;
            if (KeyDict.ContainsKey(name))
                KeyDict.TryGetValue(name, out k);
            else
                k = null;
            return k;
        }

        #endregion
    }
}
