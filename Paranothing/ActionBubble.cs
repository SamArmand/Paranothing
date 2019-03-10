using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Paranothing
{
	sealed class ActionBubble : IDrawable
	{
		readonly SpriteSheetManager _sheetManager = SpriteSheetManager.GetInstance();

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

		BubbleAction _action;
		bool _negated;
		readonly SpriteSheet _sheet;
		internal Boy Player { private get; set; }
		internal Chair Chair { set; private get; }

		string _animName;
		int _animIndex;

		string Animation
		{
			set
			{
				if (!_sheet.HasAnimation(value) || _animName == value) return;

				_animName = value;
				_animIndex = _sheet.GetAnimation(_animName).First();
			}
		}

		readonly int _negateInd;

		internal ActionBubble()
		{
			_sheet = _sheetManager.GetSheet("action");
			_action = BubbleAction.None;
			_negated = false;
			if (_sheet.HasAnimation("negate")) _negateInd = _sheet.GetAnimation("negate").First();
		}

		internal bool IsVisible { get; private set; }

		internal void Show() => IsVisible = true;

		internal void Hide() => IsVisible = false;

		internal void SetAction(BubbleAction action, bool negated)
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
			if (!IsVisible) return;

			var drawPos = new Vector2();
			if (_action != BubbleAction.Chair && Player != null)
				drawPos = new Vector2(Player.X + 11, Player.Y - 27);
			else if (Chair != null)
				drawPos = new Vector2(Chair.X + 11, Chair.Y - 32);
			renderer.Draw(_sheet.Image, drawPos, _sheet.GetSprite(0), tint, 0f, new Vector2(), 1f, SpriteEffects.None,
						  DrawLayer.ActionBubble);
			renderer.Draw(_sheet.Image, drawPos, _sheet.GetSprite(_animIndex), tint, 0f, new Vector2(), 1f,
						  SpriteEffects.None, DrawLayer.ActionBubble - 0.001f);
			if (_negated)
				renderer.Draw(_sheet.Image, drawPos, _sheet.GetSprite(_negateInd), tint, 0f, new Vector2(), 1f,
							  SpriteEffects.None, DrawLayer.ActionBubble - 0.002f);
		}
	}
}