using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
	sealed class ActionBubble : IDrawable
	{
		readonly int _negateIndex;

		readonly SpriteSheet _spriteSheet = SpriteSheetManager.GetInstance().GetSheet("action");

		bool _isNegated;
		string _animationName;
		int _animationIndex;

		BubbleAction _bubbleAction = BubbleAction.None;

		internal enum BubbleAction
		{
			None,
			Wardrobe,
			Push,
			Portrait,
			OldPortrait,
			Stair,
			Chair,
			Bookcase
		}

		internal Boy Boy { private get; set; }
		internal Chair Chair { set; private get; }

		string Animation
		{
			set
			{
				if (!_spriteSheet.HasAnimation(value) || _animationName == value) return;

				_animationName = value;
				_animationIndex = _spriteSheet.GetAnimation(_animationName).First();
			}
		}

		internal ActionBubble()
		{
			if (_spriteSheet.HasAnimation("negate")) _negateIndex = _spriteSheet.GetAnimation("negate").FirstOrDefault();
		}

		internal bool IsVisible { get; private set; }

		internal void Show() => IsVisible = true;

		internal void Hide() => IsVisible = false;

		internal void SetAction(BubbleAction action, bool negated)
		{
			_bubbleAction = action;
			_isNegated = negated;
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

		public void Draw(SpriteBatch spriteBatch, Color tint)
		{
			if (!IsVisible) return;

			var drawPos = _bubbleAction != BubbleAction.Chair && Boy != null ? new Vector2(Boy.X + 11, Boy.Y - 27) : Chair != null ? new Vector2(Chair.X + 11, Chair.Y - 32) : new Vector2();

			spriteBatch.Draw(_spriteSheet.Image, drawPos, _spriteSheet.GetSprite(0), tint, 0f, new Vector2(), 1f, SpriteEffects.None,
						  DrawLayer.ActionBubble);
			spriteBatch.Draw(_spriteSheet.Image, drawPos, _spriteSheet.GetSprite(_animationIndex), tint, 0f, new Vector2(), 1f,
						  SpriteEffects.None, DrawLayer.ActionBubble - 0.001f);
			if (_isNegated)
				spriteBatch.Draw(_spriteSheet.Image, drawPos, _spriteSheet.GetSprite(_negateIndex), tint, 0f, new Vector2(), 1f,
							  SpriteEffects.None, DrawLayer.ActionBubble - 0.002f);
		}
	}
}