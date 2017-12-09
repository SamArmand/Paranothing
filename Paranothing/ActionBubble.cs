using System.Linq;
 using Microsoft.Xna.Framework;
 using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
    internal sealed class ActionBubble : IDrawable
    {
        private readonly SpriteSheetManager _sheetMan = SpriteSheetManager.GetInstance();
        public enum BubbleAction { None, Wardrobe, Push, Portrait, OldPortrait, Stair, Chair, Bookcase }
        private BubbleAction _action;
        private bool _negated;
        private bool _visible;
        private readonly SpriteSheet _sheet;
        private Boy _player;
        public Boy Player
        {
            set => _player = value;
        }
        public Chair Chair { set; private get; }

        private string _animName;
        private int _animIndex;

        private string Animation
        {
            set
            {
                if (!_sheet.HasAnimation(value) || _animName == value) return;

                _animName = value;
                _animIndex = _sheet.GetAnimation(_animName).First();
            }
        }
        private readonly int _negateInd;

        public ActionBubble()
        {
            _sheet = _sheetMan.GetSheet("action");
            _action = BubbleAction.None;
            _visible = false;
            _negated = false;
            if (_sheet.HasAnimation("negate")) _negateInd = _sheet.GetAnimation("negate").First();
        }

        public bool IsVisible()
        {
            return _visible;
        }

        public void Show()
        {
            _visible = true;
        }

        public void Hide()
        {
            _visible = false;
        }

        public void SetAction(BubbleAction action, bool negated)
        {
            _action = action;
            _negated = negated;
            switch (action)
            {
                case BubbleAction.Wardrobe:
                    Animation = "wardrobe";
                    break;
                case BubbleAction.Portrait:
                    Animation = "portrait";
                    break;
                case BubbleAction.OldPortrait:
                    Animation = "oldportrait";
                    break;
                case BubbleAction.Push:
                    Animation = "push";
                    break;
                case BubbleAction.Chair:
                    Animation = "chair";
                    break;
                case BubbleAction.Stair:
                    Animation = "stair";
                    break;
                case BubbleAction.Bookcase:
                    Animation = "bookcase";
                    break;
                case BubbleAction.None:
                    break;
                default:
                    Animation = "negate";
                    break;
            }
        }

        public void Draw(SpriteBatch renderer, Color tint)
        {
            if (!_visible) return;

            var drawPos = new Vector2();
            if (_action != BubbleAction.Chair && _player != null)
                drawPos = new Vector2(_player.X + 11, _player.Y - 27);
            else if (Chair != null)
                drawPos = new Vector2(Chair.X + 11, Chair.Y - 32);
            renderer.Draw(_sheet.Image, drawPos, _sheet.GetSprite(0), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.ActionBubble);
            renderer.Draw(_sheet.Image, drawPos, _sheet.GetSprite(_animIndex), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.ActionBubble - 0.001f);
            if (_negated)
                renderer.Draw(_sheet.Image, drawPos, _sheet.GetSprite(_negateInd), tint, 0f, new Vector2(), 1f, SpriteEffects.None, DrawLayer.ActionBubble - 0.002f);
        }
    }
}
